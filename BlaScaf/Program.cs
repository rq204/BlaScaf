using AntDesign;
using BlaScaf.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace BlaScaf
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddAntDesign();

            //添加 Cookie 身份验证
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<HttpContextAccessor>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<HttpClient>();


            //添加用户信息
            builder.Services.AddScoped<UserService>();

            builder.Services.AddControllers();

            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                                  });
            });

            builder.Services.AddScoped<AuthenticationStateProvider, BsAuthProvider>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = BsAuthProvider.CreateTokenValidationParameters();
});

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "admin" }, RouterLink = "/", Title = "首页" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "demo", Icon = "user", Roles = new List<string>() { "admin" }, RouterLink = "/demo", Title = "演示页面" });

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}