using BlaScaf;
using BlaScaf.Components.Shared;
using Microsoft.AspNetCore.Components;

namespace DemoApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "admin", "����Ա" }, RouterLink = "/", Title = "��ҳ" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "users", Icon = "user", Roles = new List<string>() { "admin", "����Ա" }, RouterLink = "/users", Title = "�û�����" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "optlogs", Icon = "user", Roles = new List<string>() { "admin", "����Ա" }, RouterLink = "/optlogs", Title = "������־" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "syslogs", Icon = "user", Roles = new List<string>() { "admin", "����Ա" }, RouterLink = "/syslogs", Title = "ϵͳ��־" });
            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<DemoFragment>(0);
                builder.AddAttribute(1, "Title", "���Ƕ�̬���");
                builder.AddAttribute(2, "Content", $"������ {DateTime.Now:T}");
                builder.CloseComponent();
            };

            BsConfig.HeaderFragments.Add(fragment);
            BsConfig.AddOrUpdateUser = new Action<BsUser>((u) => { });

            //���ʾ���ʺ�
            BsConfig.Users.Add(new BsUser() { UserId = 1, UserName = "admin", Password = Utility.MD5("admin"), AddTime = DateTime.Now, Enable = true, EndTime = DateTime.Now.AddYears(10), LastChangePwd = DateTime.Now.AddDays(-130), Role = "����Ա", LastLogin = DateTime.Now.AddDays(-1) });

            BsConfig.CookieTimeOutMinutes = 30;
            BsConfig.ChangePwdDays = 1;

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddBsService();

            var app = builder.Build();

            // ����ҳ������
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBsService();

            app.Run();
        }

    }
}
