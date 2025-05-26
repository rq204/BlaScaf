using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Reflection;

namespace BlaScaf
{
    public class BsConfig
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public static string AppName = "BlaScaf信息管理系统";

        /// <summary>
        /// 用户角色，在用户组中可以选择
        /// </summary>
        public static List<string> UserRoles = new List<string>();

        /// <summary>
        /// 菜单当中
        /// </summary>
        public static List<BsMenuItem> MenuItems = new List<BsMenuItem>();

        /// <summary>
        /// Cookie超时时间分钟
        /// </summary>
        public static int CookieTimeOutMinutes = 30;

        /// <summary>
        /// 用于对 JWT 令牌进行签名和验证的密钥（对称加密）
        /// </summary>
        public static string JwtKey32 = "9cacbe110527992a71295a3e4bfc3f2d";

        /// <summary>
        /// 头部的外部的组件
        /// </summary>
        public static List<RenderFragment> HeaderFragments = new List<RenderFragment>();

        /// <summary>
        /// 默认登录验证方法，有问题抛异常即可
        /// </summary>
        public static Func<BsUser, UserService> LoginAction = new Func<BsUser, UserService>(DemoSys.Login);


        /// <summary>
        /// 添加默认的服务
        /// </summary>
        /// <param name="builder"></param>
        public static void AddBsService(WebApplicationBuilder builder)
        {
            // 添加 Razor 组件服务
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // 添加 AntDesign UI 框架
            builder.Services.AddAntDesign();

            // 添加 HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // 添加 HttpClient（可用于后端调用）
            builder.Services.AddHttpClient();

            // 添加用户服务
            builder.Services.AddScoped<UserService>();

            // 添加自定义身份状态提供器（Blazor 内部用）
            builder.Services.AddScoped<AuthenticationStateProvider, BsAuthProvider>();

            // 添加身份认证服务（仅使用 JWT）
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = BsAuthProvider.CreateTokenValidationParameters();
                });

            // 添加授权服务
            builder.Services.AddAuthorization();
        }
    }
}