using AntDesign;
using AntDesign.Locales;
using BlaScaf.Components;
using BlaScaf.Components.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Reflection;
using System.Text;

namespace BlaScaf
{
    public static class Startup
    {
        public static void AddBsService(this IServiceCollection services)
        {
            // 添加 Razor 组件服务
            services.AddRazorComponents()
                .AddInteractiveServerComponents(options =>
                {
                    // 低并发后台场景下，尽量保留断线重连窗口，避免短暂抖动后直接丢失页面状态。
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(30);
                    options.DisconnectedCircuitMaxRetained = 256;
                    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(2);
                })
                .AddHubOptions(options =>
                {
                    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
                    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
                    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                });

            // 添加 AntDesign UI 框架
            services.AddAntDesign();
            // 设置全局语言
            AntDesign.LocaleProvider.SetLocale("zh-CN");

            // 添加 HttpContextAccessor
            services.AddHttpContextAccessor();

            //注入用户信息解析服务
            services.AddScoped<UserService>();
            services.AddScoped<BsDbAdminService>();

            //添加api的支持
            services.AddControllers();

            services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                // 登录路径保持根相对路径（不带子目录前缀）：
                // Cookie 认证重定向时会自动拼接 Request.PathBase（即 /root），
                // 若此处再带前缀会造成 /root/root/login 的双重前缀。
                options.LoginPath = "/login";
                // 设置为 HttpOnly，防止客户端 JavaScript 访问 Cookie（提升安全性）
                options.Cookie.HttpOnly = true;
                // 设置 Cookie 的安全策略：仅在 HTTPS 时才设置 Secure 标志（推荐 SameAsRequest）
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                // 设置 SameSite 策略，限制跨站点请求时是否发送 Cookie
                options.Cookie.SameSite = SameSiteMode.Strict;

                // 最长 24 天（防止前端 keep-alive 机制或 setInterval 超出浏览器/JS 最大间隔）
                int timemin = BsConfig.CookieTimeOutMinutes > 34560 ? 34560 : BsConfig.CookieTimeOutMinutes;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(timemin); // 永远需要设这个值

                // 设置为滑动过期：
                options.SlidingExpiration = true;
                ///该cookie是必需的
                options.Cookie.IsEssential = true;

                // 关键：控制是否持久化 Cookie
                if (BsConfig.UseSessionCookie)
                {
                    // 不写入硬盘，关闭浏览器 Cookie 消失
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnSigningIn = context =>
                        {
                            context.Properties.IsPersistent = false; // 不持久化
                            return Task.CompletedTask;
                        }
                    };
                }
                else
                {
                    // 写入硬盘，关闭浏览器后仍保留 Cookie
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnSigningIn = context =>
                        {
                            context.Properties.IsPersistent = true;
                            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(timemin);
                            return Task.CompletedTask;
                        }
                    };
                }
            });


            // 添加授权服务，用于控制访问权限（配合 [Authorize] 特性使用）
            // 这是 ASP.NET Core 授权系统的核心服务注册
            services.AddAuthorization();

            // 注册 Blazor Server 专用的身份状态提供器，用于获取当前用户的身份信息
            // AuthenticationStateProvider 是 Blazor 中用于提供用户认证状态的抽象基类
            // ServerAuthenticationStateProvider 是 Blazor Server 的默认实现
            services.AddScoped<AuthenticationStateProvider, BsAuthStateProvider>();

            // 启用级联身份验证状态，使 Blazor 组件树可以通过 [CascadingParameter] 注入 AuthenticationState
            // 这样组件中可以使用 <AuthorizeView>、[AuthorizeView]、[CascadingAuthenticationState] 等功能
            services.AddCascadingAuthenticationState();
        }

        /// <summary>
        /// 扩展方法：注册和配置 Blazor Server 服务管道。
        /// </summary>
        /// <param name="app">WebApplication 实例。</param>
        public static void UseBsService(this WebApplication app)
        {
            // 子目录访问模式：配置 PathBase 后，管理端限定在 /root/* 下访问。
            // 根目录保留给宿主自己的前台（如 Vue3 静态站点部署在 wwwroot）：
            // - /root/*：管理端 Blazor 页面及其静态资源
            // - 根目录下 wwwroot 中存在的文件：直接访问，"/" 返回 wwwroot/index.html 作为前台入口
            // - 根目录下配置在 BsConfig.RootApiPrefixes 中的 API 前缀：放行，供前台前端调用根级 WebAPI
            // - 其余根目录请求（如管理端页面 /login、不存在的文件）：直接 404，不重定向
            if (!string.IsNullOrWhiteSpace(BsConfig.PathBase))
            {
                var prefix = "/" + BsConfig.PathBase.Trim('/');
                var rootApiPrefixes = (BsConfig.RootApiPrefixes ?? new List<string>())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => "/" + p.Trim('/'))
                    .ToArray();

                app.Use(async (context, next) =>
                {
                    var path = context.Request.Path;

                    // 子目录请求：放行，由后续 UsePathBase 剥离前缀后进入管理端管道
                    if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        await next();
                        return;
                    }

                    // 根级 WebAPI：仅放行显式配置的前缀
                    if (rootApiPrefixes.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase)))
                    {
                        await next();
                        return;
                    }

                    // 根目录静态文件：仅 wwwroot 中真实存在的文件放行；
                    // "/"（前台站点入口）重写为 /index.html 后由静态文件中间件返回
                    var fileProvider = context.RequestServices.GetRequiredService<IWebHostEnvironment>().WebRootFileProvider;
                    var relative = path.Value?.TrimStart('/');
                    if (string.IsNullOrEmpty(relative)) relative = "index.html";
                    if (!fileProvider.GetFileInfo(relative).Exists)
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }
                    if (path.Value == "/" || string.IsNullOrEmpty(path.Value))
                    {
                        context.Request.Path = "/index.html";
                    }
                    await next();
                });

                app.UsePathBase(prefix);
            }

            // 启用静态文件中间件，用于服务 wwwroot 下的静态资源（如 JS、CSS、图片等）
            app.UseStaticFiles();

            // 添加路由中间件，启用路由功能，后续的中间件（如控制器）才能基于路由工作
            app.UseRouting();

            // 添加身份验证中间件，用于处理用户登录、认证等功能
            app.UseAuthentication();

            // 添加授权中间件，基于当前用户的权限来限制访问特定资源
            app.UseAuthorization();

            // 启用防跨站请求伪造（CSRF）攻击的保护（通常用于表单提交）
            app.UseAntiforgery();

            // 映射控制器路由（如 Web API 或传统 MVC 控制器），这句必须有，否则控制器不会生效
            app.MapControllers();

            // 映射 Razor 组件（Blazor）入口点，并启用 Blazor Server 的交互式渲染模式
            // 其中 App 是组件的根组件
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode().AddAdditionalAssemblies(GetAdditionalAssemblies()!); // 启用 Blazor Server 模式（非 WebAssembly）

            // 未匹配任何端点的请求（包括子目录下不存在的文件）保持 Blazor 默认的 404 行为，
            // 不做任何兜底页面。
        }

        /// <summary>
        /// 初始化BsConfig中使用freesql的方法
        /// 这个默认是要外部使用不同orm来实现的
        /// 因为freesql用的多所以使用它来做示例
        /// </summary>
        /// <param name="fsql"></param>
        public static void InitFreeSqlActionFunc(IFreeSql fsql)
        {
            BsConfig.GetOptLogs = new Func<int, int, int, BlaScaf.QueryRsp<List<BsOptLog>>>((pageIndex, pageSize, userId) =>
            {
                BlaScaf.QueryRsp<List<BsOptLog>> datas = new BlaScaf.QueryRsp<List<BsOptLog>>() { Value = new List<BsOptLog>() };
                if (userId == 0)
                {
                    datas.Value = fsql.Select<BsOptLog>().Count(out var tatol).Page(pageIndex, pageSize).OrderByDescending(b => b.OptLogId).ToList();
                    datas.Total = (int)tatol;
                }
                else
                {
                    datas.Value = fsql.Select<BsOptLog>().Where(w => w.UserId == userId).Count(out var tatol).Page(pageIndex, pageSize).OrderByDescending(b => b.OptLogId).ToList();
                    datas.Total = (int)tatol;
                }
                return datas;
            });
            BsConfig.GetSysLogs = new Func<int, int, BlaScaf.QueryRsp<List<BsSysLog>>>((pageIndex, pageSize) =>
            {
                BlaScaf.QueryRsp<List<BsSysLog>> datas = new BlaScaf.QueryRsp<List<BsSysLog>>() { Value = new List<BsSysLog>() };
                datas.Value = fsql.Select<BsSysLog>().Count(out var count).Page(pageIndex, pageSize).OrderByDescending(b => b.SysLogId).ToList();
                datas.Total = (int)count;
                return datas;
            });

            BsConfig.AddOrUpdateUser = new Action<BsUser>((u) =>
            {
                if (u.UserId == 0)
                {
                    u.UserId = (int)fsql.Insert(u).ExecuteIdentity();
                    BsConfig.Users.Insert(0, u);
                }
                else
                {
                    var repo = fsql.GetRepository<BsUser>(); //可以从 IOC 容器中获取
                    var item = repo.Where(a => a.UserId == u.UserId).First();  //此时快照 item
                    BlaScaf.Utility.UpdateDifferentProperties<BsUser>(u, item);
                    repo.Update(item); //对比快照时的变化

                    BsUser cache = BsConfig.Users.Find(f => f.UserId == u.UserId);

                    ///更新字段
                    BlaScaf.Utility.UpdateDifferentProperties<BsUser>(u, cache);
                }
            });

            BsConfig.AddSysLog = new Action<BsSysLog>((x) =>
            {
                fsql.Insert(x).ExecuteAffrows();
            });
            BsConfig.AddOptLog = new Action<BsOptLog>((x) =>
            {
                fsql.Insert(x).ExecuteAffrows();
            });
        }

        /// <summary>
        /// 检测配置是否正确
        /// </summary>
        /// <exception cref="Exception"></exception>
        public static void CheckBsConfig()
        {
            if (string.IsNullOrEmpty(BsConfig.AppName)) throw new Exception("AppName不能为空");
            if (BsConfig.CookieTimeOutMinutes == 0) throw new Exception("CookieTimeOutMinutes不能为0");
            if (BsConfig.MenuItems == null || BsConfig.MenuItems.Count == 0) throw new Exception("BsConfig.MenuItems 不能为空");
            if (BsConfig.MenuItems.Find(f => !f.RouterLink.StartsWith("/")) != null) throw new Exception("所有路由请求必须以/开始");
            if (BsConfig.Roles.Count == 0) throw new Exception("用户角色不能为空");
            if (BsConfig.Users.Count == 0) throw new Exception("用户数不能为空");
            if (BsConfig.AddOptLog == null) throw new Exception("AddOptLog不能为空");
            if (BsConfig.AddSysLog == null) throw new Exception("AddSysLog不能为空");
            if (BsConfig.AddOrUpdateUser == null) throw new Exception("AddOrUpdateUser不能为空");
            if (BsConfig.GetOptLogs == null) throw new Exception("GetOptLogs不能为空");
            if (BsConfig.GetSysLogs == null) throw new Exception("GetSysLogs不能为空");

            foreach (var field in BsConfig.UserEditorFields)
            {
                if (string.IsNullOrWhiteSpace(field.DisplayName)) throw new Exception("BsConfig.UserEditorFields.DisplayName不能为空");
                if (string.IsNullOrWhiteSpace(field.FieldName)) throw new Exception("BsConfig.UserEditorFields.FieldName不能为空");

                var property = typeof(BsUser).GetProperty(field.FieldName);
                if (property == null) throw new Exception($"BsConfig.UserEditorFields 中字段 {field.FieldName} 在 BsUser 中不存在");

                if (field.ControlType == BsUserEditorFieldControlType.String && property.PropertyType != typeof(string))
                    throw new Exception($"字段 {field.FieldName} 不是 string，不能使用 String 控件");

                if (field.ControlType == BsUserEditorFieldControlType.Int && property.PropertyType != typeof(int))
                    throw new Exception($"字段 {field.FieldName} 不是 int，不能使用 Int 控件");

                if (field.ControlType == BsUserEditorFieldControlType.Bool && property.PropertyType != typeof(bool))
                    throw new Exception($"字段 {field.FieldName} 不是 bool，不能使用 Bool 控件");

                if (field.ControlType == BsUserEditorFieldControlType.ListString)
                {
                    if (property.PropertyType != typeof(string))
                        throw new Exception($"字段 {field.FieldName} 不是 string，不能使用 ListString 控件");

                    if (field.GetListValues == null)
                        throw new Exception($"字段 {field.FieldName} 使用 ListString 控件时必须提供 GetListValues");
                }
            }
        }

        /// <summary>
        /// 获取其它程序集
        /// </summary>
        /// <returns></returns>
        public static Assembly[] GetAdditionalAssemblies()
        {
            var mainAppAssembly = typeof(App).Assembly;
            var entryAssembly = Assembly.GetEntryAssembly();

            // 构建一个不重复的列表
            var additionalAssemblies = new[]
            {
    entryAssembly,
    // 其它你想加的程序集
}
            .Where(asm => asm != null && asm != mainAppAssembly) // 去重
            .Distinct()
            .ToArray();
            return additionalAssemblies;
        }
    }
}
