using AntDesign;
using BlaScaf.Components;
using BlaScaf.Components.Shared;
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

            // 添加 HttpContextAccessor
            services.AddHttpContextAccessor();

            //注入用户信息解析服务
            services.AddScoped<UserService>();

            //添加api的支持
            services.AddControllers();

            // 添加认证和授权
            services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    // 指定未登录时自动重定向的路径
                    options.LoginPath = "/login"; // 例如：访问需要登录的页面时自动跳转到此路径

                    // 设置为 HttpOnly，防止客户端 JavaScript 访问 Cookie（提升安全性）
                    options.Cookie.HttpOnly = true;

                    // 设置 Cookie 的安全策略：仅在 HTTPS 时才设置 Secure 标志（推荐 SameAsRequest）
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                    // 设置 SameSite 策略，限制跨站点请求时是否发送 Cookie
                    options.Cookie.SameSite = SameSiteMode.Strict;

                    // 设置 Cookie 的有效时间（如果是持久 Cookie，写入到硬盘）
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(BsConfig.CookieTimeOutMinutes);

                    // 设置为滑动过期：
                    // 如果用户在 30 分钟内有活动，将重新设置 Cookie 有效期，从而延长会话
                    options.SlidingExpiration = true;

                    if (BsConfig.UseSessionCookie) // 你自定义的配置开关
                    {
                        // 不设置 Cookie 的 Expiration，相当于浏览器关闭就清除 Cookie（Session Cookie）
                        options.Cookie.Expiration = null; // 关键点：不设置具体过期时间，浏览器关闭即失效
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
