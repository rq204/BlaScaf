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
            BsConfig.ChangePwdDays = 90;

            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "管理员", "操作员" }, RouterLink = "/", Title = "首页" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "users", Icon = "user", Roles = new List<string>() { "管理员" }, RouterLink = "/users", Title = "用户管理" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "optlogs", Icon = "edit", Roles = new List<string>() { "管理员", "操作员" }, RouterLink = "/optlogs", Title = "操作日志" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "syslogs", Icon = "highlight", Roles = new List<string>() { "管理员", "操作员" }, RouterLink = "/syslogs", Title = "系统日志" });

            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<Shared.DemoFragment>(0);
                builder.AddAttribute(1, "Title", "我是动态组件");
                builder.AddAttribute(2, "Content", $"生成于 {DateTime.Now:T}");
                builder.CloseComponent();
            };
            BsConfig.HeaderFragments.Add(fragment);
            BsConfig.UserAuthFragment = (BsUser user, Func<Task> onCloseCallback) => builder =>
            {
                builder.OpenComponent(0, typeof(UserFragment));
                builder.AddAttribute(1, "User", user); // 给组件传参
                builder.AddAttribute(2, "Visible", true);
                // 如果有关闭回调，添加VisibleChanged事件处理
                if (onCloseCallback != null)
                {
                    builder.AddAttribute(3, "VisibleChanged", EventCallback.Factory.Create<bool>(new object(), async (visible) =>
                    {
                        if (!visible)
                        {
                            await onCloseCallback();
                        }
                    }));
                }
                builder.CloseComponent();
            };

            BsConfig.HeadInjectRawHtmls.Add("<script src='test.js'></script>");

            ///设置权限
            BsConfig.Roles = new List<string>() { "管理员", "操作员" };
            //添加示例帐号
            BsConfig.Users.Add(new BsUser() { UserId = 1, UserName = "admin", FullName = "张三", Password = Utility.MD5("admin"), AddTime = DateTime.Now, Enable = true, EndTime = DateTime.Now.AddYears(10), LastChangePwd = DateTime.Now.AddDays(-1), Role = "管理员", LastLogin = DateTime.Now.AddDays(-1) });
            BsConfig.Users.Add(new BsUser() { UserId = 2, UserName = "test", FullName = "李四", Password = Utility.MD5("Test1234"), AddTime = DateTime.Now, Enable = true, EndTime = DateTime.Now.AddYears(10), LastChangePwd = DateTime.Now.AddDays(-1), Role = "操作员", LastLogin = DateTime.Now.AddDays(-1) });

            BsConfig.GetOptLogs = new Func<int, int, int, QueryRsp<List<BsOptLog>>>((a, b, c) =>
            {
                QueryRsp<List<BsOptLog>> datas = new QueryRsp<List<BsOptLog>>() { Value = new List<BsOptLog>() };
                for (int i = 0; i < 100; i++)
                {
                    datas.Value.Add(new BsOptLog() { OptLogId = i+1, OptTime = DateTime.Now.AddMinutes(0 - i), Summary = "示例操作" });
                }
                datas.Total = datas.Value.Count;
                return datas;
            });
            BsConfig.GetSysLogs = new Func<int, int, QueryRsp<List<BsSysLog>>>((pageindex, pagesize) =>
            {
                QueryRsp<List<BsSysLog>> datas = new QueryRsp<List<BsSysLog>>() { Value = new List<BsSysLog>() };
                for (int i = 0; i < 100; i++)
                {
                    datas.Value.Add(new BsSysLog() { SysLogId = i + 1, SysTime = DateTime.Now.AddMinutes(0 - i), LogType = "登录成功", Message = $"帐号{i}在XX点XX分登录成功" });
                }
                datas.Total = datas.Value.Count;
                return datas;
            });

            BsConfig.AddLogin = new Action<BsUser>((u) =>
            {
                //可以在数据库等地方更新登录记录
            });
            BsConfig.AddOrUpdateUser = new Action<BsUser>((u) =>
            {
                if (u.UserId == 0)
                {
                    if (u.Role == "管理员" && u.Password.Length < 15) throw new Exception("管理员密码长度必须大于等于15位");
                    u.Password = Utility.MD5(u.Password);
                    BsConfig.Users.Insert(0, u);
                    u.UserId = BsConfig.Users.Count;
                }
                else
                {
                    BsUser old = BsConfig.Users.Find(f => f.UserId == u.UserId);
                    if (!string.IsNullOrEmpty(u.Password) && u.Password.Length != 32)
                    {
                        if (u.Role == "管理员" && u.Password.Length < 15) throw new Exception("管理员密码长度必须大于等于15位");
                        u.Password = Utility.MD5(u.Password);
                    }
                    else
                    {
                        u.Password = old.Password;
                    }

                    ///更新字段
                    Utility.UpdateDifferentProperties<BsUser>(u, old);
                }
            });
            BsConfig.AddSysLog = new Action<BsSysLog>((x) => { });
            BsConfig.AddOptLog = new Action<BsOptLog>((x) => { });

            ///验证码组件
            BsConfig.CaptchaRoles = new List<string>() { "管理员" };
            BsConfig.CaptchaFragment = () => builder =>
            {
                builder.OpenComponent<Shared.CaptchaFragment>(0);
                builder.CloseComponent();
            };

            BlaScaf.Startup.CheckBsConfig();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddBsService();

            var app = builder.Build();

            app.UseBsService();

            app.Run();
        }

    }
}