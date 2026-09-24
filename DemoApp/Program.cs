using BlaScaf;
using DemoApp.Shared;
using FreeSql;
using Microsoft.AspNetCore.Components;

namespace DemoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var adminRole = "管理员";
            var auditRole = "审计员";

            BsConfig.AppName = "BlaScaf后台系统演示";
            BsConfig.CookieTimeOutMinutes = 30;
            BsConfig.ChangePwdDays = 90;

            // 访问模式二选一：
            // - 根目录访问：留空（默认），通过 /login 访问
            // - 子目录访问：设置为子目录名（不带斜杠），此时管理端只能通过 /root/login 访问；
            //   根目录保留给前台静态站点（wwwroot 中的文件可直接访问，/ 返回 index.html），
            //   根目录下不存在的文件返回 Blazor 默认的 404
            BsConfig.PathBase = "";

            // 根级 WebAPI 前缀：Vue3 等前台静态前端直接调用 /api/*（不带 /root 前缀），
            // 管理端页面则只能通过 /root/* 访问
            BsConfig.RootApiPrefixes = new List<string> { "/api" };

            BsConfig.Roles = new List<string> { adminRole, auditRole };
            BsConfig.DbAdminEntityTypes = new List<Type> { typeof(BsUser), typeof(BsOptLog), typeof(BsSysLog) };

            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "home",
                Icon = "home",
                Roles = new List<string> { adminRole, auditRole },
                RouterLink = "/",
                Title = "首页"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "users",
                Icon = "user",
                Roles = new List<string> { adminRole },
                RouterLink = "/users",
                Title = "用户管理"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "dbadmin",
                Icon = "database",
                Roles = new List<string> { adminRole },
                RouterLink = "/dbadmin",
                Title = "数据库管理"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "optlogs",
                Icon = "edit",
                Roles = new List<string> { adminRole, auditRole },
                RouterLink = "/optlogs",
                Title = "操作日志"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "syslogs",
                Icon = "highlight",
                Roles = new List<string> { adminRole, auditRole },
                RouterLink = "/syslogs",
                Title = "系统日志"
            });

            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<DemoFragment>(0);
                builder.AddAttribute(1, "Title", "这是动态内容");
                builder.AddAttribute(2, "Content", $"当前时间 {DateTime.Now:T}");
                builder.CloseComponent();
            };
            BsConfig.HeaderFragments.Add(fragment);

            BsConfig.UserAuthFragment = (BsUser user, Func<Task> onCloseCallback) => builder =>
            {
                builder.OpenComponent<UserFragment>(0);
                builder.AddAttribute(1, "User", user);
                builder.AddAttribute(2, "Visible", true);

                if (onCloseCallback != null)
                {
                    builder.AddAttribute(3, "VisibleChanged", EventCallback.Factory.Create<bool>(new object(), async visible =>
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
            BsConfig.CaptchaRoles = new List<string> { auditRole };
            BsConfig.CaptchaFragment = () => builder =>
            {
                builder.OpenComponent<CaptchaFragment>(0);
                builder.CloseComponent();
            };

            var builder = WebApplication.CreateBuilder(args);

            var dbPath = Path.Combine(AppContext.BaseDirectory, "blascaf_demo.db");
            var fsql = new FreeSqlBuilder()
                .UseConnectionString(DataType.Sqlite, $"Data Source={dbPath}")
                .UseAutoSyncStructure(true)
                .Build();

            fsql.CodeFirst.SyncStructure(new[] { typeof(BsUser), typeof(BsOptLog), typeof(BsSysLog) });
            SeedUsers(fsql, adminRole, auditRole);

            BsConfig.Users = fsql.Select<BsUser>().OrderByDescending(x => x.UserId).ToList();
            Startup.InitFreeSqlActionFunc(fsql);
            BsConfig.AddLogin = user =>
            {
                fsql.Update<BsUser>().SetSource(user).ExecuteAffrows();
            };

            Startup.CheckBsConfig();

            builder.Services.AddSingleton<IFreeSql>(fsql);
            builder.Services.AddBsService();

            var app = builder.Build();
            app.UseBsService();
            app.Run();
        }

        private static void SeedUsers(IFreeSql fsql, string adminRole, string auditRole)
        {
            if (!fsql.Select<BsUser>().Any(x => x.UserName == "admin"))
            {
                fsql.Insert(new BsUser
                {
                    UserName = "admin",
                    FullName = "系统管理员",
                    Password = Utility.MD5("admin"),
                    AddTime = DateTime.Now,
                    Enable = true,
                    EndTime = DateTime.Now.AddYears(10),
                    LastChangePwd = DateTime.Now.AddDays(-1),
                    Role = adminRole,
                    LastLogin = DateTime.Now.AddDays(-1),
                    RegIP = "127.0.0.1",
                    LastIP = "127.0.0.1"
                }).ExecuteAffrows();
            }

            if (!fsql.Select<BsUser>().Any(x => x.UserName == "test"))
            {
                fsql.Insert(new BsUser
                {
                    UserName = "test",
                    FullName = "审计示例",
                    Password = Utility.MD5("Test1234"),
                    AddTime = DateTime.Now,
                    Enable = true,
                    EndTime = DateTime.Now.AddYears(10),
                    LastChangePwd = DateTime.Now.AddDays(-1),
                    Role = auditRole,
                    LastLogin = DateTime.Now.AddDays(-1),
                    RegIP = "127.0.0.1",
                    LastIP = "127.0.0.1"
                }).ExecuteAffrows();
            }
        }
    }
}
