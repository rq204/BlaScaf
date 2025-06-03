using BlaScaf;
using BlaScaf.Components.Pages;
using BlaScaf.Components.Shared;
using DemoApp.Shared;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace DemoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BsConfig.AppName = "BlaScaf管理系统演示";
            BsConfig.CookieTimeOutMinutes = 30;
            BsConfig.ChangePwdDays = 1;

            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "管理员" }, RouterLink = "/", Title = "首页" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "users", Icon = "user", Roles = new List<string>() { "管理员" }, RouterLink = "/users", Title = "用户管理" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "optlogs", Icon = "edit", Roles = new List<string>() { "管理员" }, RouterLink = "/optlogs", Title = "操作日志" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "syslogs", Icon = "highlight", Roles = new List<string>() { "管理员" }, RouterLink = "/syslogs", Title = "系统日志" });

            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<Shared.DemoFragment>(0);
                builder.AddAttribute(1, "Title", "我是动态组件");
                builder.AddAttribute(2, "Content", $"生成于 {DateTime.Now:T}");
                builder.CloseComponent();
            };
            BsConfig.HeaderFragments.Add(fragment);
            BsConfig.UserAuthFragment = (BsUser user) => builder =>
            {
                builder.OpenComponent(0, typeof(UserFragment));
                builder.AddAttribute(1, "User", user); // 给组件传参
                builder.AddAttribute(2, "Visible", true);
                builder.CloseComponent();
            };

            ///设置权限
            BsConfig.Roles = new List<string>() { "管理员", "操作员" };
            //添加示例帐号
            BsConfig.Users.Add(new BsUser() { UserId = 1, UserName = "admin", Password = Utility.MD5("admin"), AddTime = DateTime.Now, Enable = true, EndTime = DateTime.Now.AddYears(10), LastChangePwd = DateTime.Now.AddDays(-130), Role = "管理员", LastLogin = DateTime.Now.AddDays(-1) });

            BsConfig.GetOptLogs = new Func<int, int, int, QueryRsp<List<BsOptLog>>>((a, b, c) =>
            {
                QueryRsp<List<BsOptLog>> datas = new QueryRsp<List<BsOptLog>>() { Value = new List<BsOptLog>() };
                for (int i = 0; i < 100; i++)
                {
                    datas.Value.Add(new BsOptLog() { OptLogId = i, OptTime = DateTime.Now.AddMinutes(0 - i), Summary = "示例操作" });
                }
                datas.Total= datas.Value.Count;
                return datas;
            });
            BsConfig.GetSysLogs = new Func<int, int, QueryRsp<List<BsSysLog>>>((pageindex, pagesize) =>
            {
                QueryRsp<List<BsSysLog>> datas = new QueryRsp<List<BsSysLog>>() { Value = new List<BsSysLog>() };
                for (int i = 0; i < 100; i++)
                {
                    datas.Value.Add(new BsSysLog() { SysLogId = i, SysTime = DateTime.Now.AddMinutes(0 - i) });
                }
                datas.Total = datas.Value.Count;
                return datas;
            });

            BsConfig.AddOrUpdateUser = new Action<BsUser>((u) => { });
            BsConfig.AddSysLog = new Action<BsSysLog>((x) => { });
            BsConfig.AddOptLog = new Action<BsOptLog>((x) => { });

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
