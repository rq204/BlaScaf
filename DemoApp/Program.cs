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
            var adminRole = "\u7ba1\u7406\u5458";
            var auditRole = "\u5ba1\u8ba1\u5458";

            BsConfig.AppName = "BlaScaf\u540e\u53f0\u7cfb\u7edf\u6f14\u793a";
            BsConfig.CookieTimeOutMinutes = 30;
            BsConfig.ChangePwdDays = 90;

            BsConfig.Roles = new List<string> { adminRole, auditRole };
            BsConfig.DbAdminEntityTypes = new List<Type> { typeof(BsUser), typeof(BsOptLog), typeof(BsSysLog) };

            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "home",
                Icon = "home",
                Roles = new List<string> { adminRole, auditRole },
                RouterLink = "/",
                Title = "\u9996\u9875"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "users",
                Icon = "user",
                Roles = new List<string> { adminRole },
                RouterLink = "/users",
                Title = "\u7528\u6237\u7ba1\u7406"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "dbadmin",
                Icon = "database",
                Roles = new List<string> { adminRole },
                RouterLink = "/dbadmin",
                Title = "\u6570\u636e\u5e93\u7ba1\u7406"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "optlogs",
                Icon = "edit",
                Roles = new List<string> { adminRole, auditRole },
                RouterLink = "/optlogs",
                Title = "\u64cd\u4f5c\u65e5\u5fd7"
            });
            BsConfig.MenuItems.Add(new BsMenuItem
            {
                Key = "syslogs",
                Icon = "highlight",
                Roles = new List<string> { adminRole, auditRole },
                RouterLink = "/syslogs",
                Title = "\u7cfb\u7edf\u65e5\u5fd7"
            });

            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<DemoFragment>(0);
                builder.AddAttribute(1, "Title", "\u8fd9\u662f\u52a8\u6001\u5185\u5bb9");
                builder.AddAttribute(2, "Content", $"\u5f53\u524d\u65f6\u95f4 {DateTime.Now:T}");
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
                    FullName = "\u7cfb\u7edf\u7ba1\u7406\u5458",
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
                    FullName = "\u5ba1\u8ba1\u793a\u4f8b",
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
