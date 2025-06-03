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
            BsConfig.AppName = "BlaScaf����ϵͳ��ʾ";
            BsConfig.CookieTimeOutMinutes = 30;
            BsConfig.ChangePwdDays = 1;

            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "home", Icon = "home", Roles = new List<string>() { "����Ա" }, RouterLink = "/", Title = "��ҳ" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "users", Icon = "user", Roles = new List<string>() { "����Ա" }, RouterLink = "/users", Title = "�û�����" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "optlogs", Icon = "edit", Roles = new List<string>() { "����Ա" }, RouterLink = "/optlogs", Title = "������־" });
            BsConfig.MenuItems.Add(new BsMenuItem() { Key = "syslogs", Icon = "highlight", Roles = new List<string>() { "����Ա" }, RouterLink = "/syslogs", Title = "ϵͳ��־" });

            RenderFragment fragment = builder =>
            {
                builder.OpenComponent<Shared.DemoFragment>(0);
                builder.AddAttribute(1, "Title", "���Ƕ�̬���");
                builder.AddAttribute(2, "Content", $"������ {DateTime.Now:T}");
                builder.CloseComponent();
            };
            BsConfig.HeaderFragments.Add(fragment);
            BsConfig.UserAuthFragment = (BsUser user) => builder =>
            {
                builder.OpenComponent(0, typeof(UserFragment));
                builder.AddAttribute(1, "User", user); // ���������
                builder.AddAttribute(2, "Visible", true);
                builder.CloseComponent();
            };

            ///����Ȩ��
            BsConfig.Roles = new List<string>() { "����Ա", "����Ա" };
            //���ʾ���ʺ�
            BsConfig.Users.Add(new BsUser() { UserId = 1, UserName = "admin", Password = Utility.MD5("admin"), AddTime = DateTime.Now, Enable = true, EndTime = DateTime.Now.AddYears(10), LastChangePwd = DateTime.Now.AddDays(-130), Role = "����Ա", LastLogin = DateTime.Now.AddDays(-1) });

            BsConfig.GetOptLogs = new Func<int, int, int, QueryRsp<List<BsOptLog>>>((a, b, c) =>
            {
                QueryRsp<List<BsOptLog>> datas = new QueryRsp<List<BsOptLog>>() { Value = new List<BsOptLog>() };
                for (int i = 0; i < 100; i++)
                {
                    datas.Value.Add(new BsOptLog() { OptLogId = i, OptTime = DateTime.Now.AddMinutes(0 - i), Summary = "ʾ������" });
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
