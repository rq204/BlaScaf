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
                .AddInteractiveServerComponents();

            // 添加 AntDesign UI 框架
            services.AddAntDesign();
            // 设置全局语言
            AntDesign.LocaleProvider.SetLocale("zh-CN");

            // 添加 HttpContextAccessor
            services.AddHttpContextAccessor();

            //注入用户信息解析服务
            services.AddScoped<UserService>();

            //添加api的支持
            services.AddControllers();

            services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
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
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            // 启用级联身份验证状态，使 Blazor 组件树可以通过 [CascadingParameter] 注入 AuthenticationState
            // 这样组件中可以使用 <AuthorizeView>、[AuthorizeView]、[CascadingAuthenticationState] 等功能
            services.AddCascadingAuthenticationState();

            services.AddHttpContextAccessor();
        }

        /// <summary>
        /// 扩展方法：注册和配置 Blazor Server 服务管道。
        /// </summary>
        /// <param name="app">WebApplication 实例。</param>
        public static void UseBsService(this WebApplication app)
        {
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
                    u.Password = BlaScaf.Utility.MD5(u.Password);
                    u.UserId = (int)fsql.Insert(u).ExecuteIdentity();
                    BsConfig.Users.Insert(0, u);
                }
                else
                {
                    BsUser old = BsConfig.Users.Find(f => f.UserId == u.UserId);
                    if (!string.IsNullOrEmpty(u.Password) && u.Password.Length != 32)
                    {
                        u.Password = BlaScaf.Utility.MD5(u.Password);
                    }
                    else
                    {
                        u.Password = old.Password;
                    }

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
