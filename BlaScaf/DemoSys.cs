using BlaScaf.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace BlaScaf
{

    public class DemoSys
    {
        public static UserService Login(BsUser bsUser)
        {
            if (bsUser.Username != "admin" && bsUser.Password != "admin")
            {
                throw new Exception("用户名或密码错误");
            }
            UserService userService = new UserService();
            userService.Username= bsUser.Username;
            userService.UserId= bsUser.UserId;
            userService.Roles = new List<string>() { "admin" };

            return userService;
        }

        public static void LoadConfig()
        {
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "admin" }, RouterLink = "/", Title = "首页" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "demo", Icon = "user", Roles = new List<string>() { "admin" }, RouterLink = "/demo", Title = "演示页面" });

            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<DemoFragment>(0);
                builder.AddAttribute(1, "Title", "我是动态组件");
                builder.AddAttribute(2, "Content", $"生成于 {DateTime.Now:T}");
                builder.CloseComponent();
            };

            BsConfig.HeaderFragments.Add(fragment);
        }
    }
}
