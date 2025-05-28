using AntDesign;
using BlaScaf.Components;
using BlaScaf.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BlaScaf
{
    public static class Startup
    {
        public static void Main(string[] args)
        {
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "admin", "管理员" }, RouterLink = "/", Title = "首页" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "demo", Icon = "user", Roles = new List<string>() { "admin", "管理员" }, RouterLink = "/demo", Title = "演示页面" });

            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<DemoFragment>(0);
                builder.AddAttribute(1, "Title", "我是动态组件");
                builder.AddAttribute(2, "Content", $"生成于 {DateTime.Now:T}");
                builder.CloseComponent();
            };

            BsConfig.HeaderFragments.Add(fragment);

            //添加示例帐号
            BsConfig.Users.Add(new BsUser() { UserId = 1, Username = "admin", Password = Utility.MD5("admin"), AddTime = DateTime.Now, Enable = true, EndTime = DateTime.Now.AddYears(10), LastChangePwd = DateTime.Now.AddDays(-30), Role = "管理员", LastLogin = DateTime.Now.AddDays(-1) });

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddBsService();

            var app = builder.Build();

            // 错误页面配置
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBsService();

            app.Run();
        }

        public static void AddBsService(this IServiceCollection services)
        {
            // 添加 Razor 组件服务
            services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // 添加 AntDesign UI 框架
            services.AddAntDesign();

            // 添加 HttpContextAccessor
            services.AddHttpContextAccessor();

            services.AddScoped<UserService>();

            services.AddControllers();

            // 添加认证和授权
            services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/login"; // 未登录跳转
                    options.AccessDeniedPath = "/forbidden"; // 无权限跳转
                    //options.Cookie.Name = "bsauth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

            // 添加授权服务
            services.AddAuthorization();
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
            services.AddCascadingAuthenticationState();
        }

        public static void UseBsService(this WebApplication app)
        {
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAntiforgery();
            app.MapControllers(); // 必须要有

            // 映射 Razor 组件入口
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();
        }
    }
}
