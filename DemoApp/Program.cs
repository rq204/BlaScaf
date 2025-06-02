using BlaScaf;
using BlaScaf.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace DemoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "admin", "管理员" }, RouterLink = "/", Title = "首页" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "users", Icon = "user", Roles = new List<string>() { "admin", "管理员" }, RouterLink = "/users", Title = "用户管理" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "optlogs", Icon = "user", Roles = new List<string>() { "admin", "管理员" }, RouterLink = "/optlogs", Title = "操作日志" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "syslogs", Icon = "user", Roles = new List<string>() { "admin", "管理员" }, RouterLink = "/syslogs", Title = "系统日志" });
            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<DemoFragment>(0);
                builder.AddAttribute(1, "Title", "我是动态组件");
                builder.AddAttribute(2, "Content", $"生成于 {DateTime.Now:T}");
                builder.CloseComponent();
            };

            BsConfig.HeaderFragments.Add(fragment);
            BsConfig.AddOrUpdateUser = new Action<BsUser>((u) => { });

            //添加示例帐号
            BsConfig.Users.Add(new BsUser() { UserId = 1, UserName = "admin", Password = Utility.MD5("admin"), AddTime = DateTime.Now, Enable = true, EndTime = DateTime.Now.AddYears(10), LastChangePwd = DateTime.Now.AddDays(-130), Role = "管理员", LastLogin = DateTime.Now.AddDays(-1) });

            BsConfig.CookieTimeOutMinutes = 30;
            BsConfig.ChangePwdDays = 1;

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

    }
}
