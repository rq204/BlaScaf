using AntDesign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;

namespace BlaScaf.Components.Pages
{
    public partial class Users
    {
        [Inject] public MessageService MessageService { get; set; }

        ITable table;

        private bool isloading = false;

        protected override async Task OnInitializedAsync()
        {
            await PageIndexSizeChange();
            await base.OnInitializedAsync();
        }

        private int pageIndex = 0;
        private int pageSize = 15;
        private bool showFullName = false;
        private QueryRsp<List<BsUser>> userrsp = new QueryRsp<List<BsUser>>();

        private async void PageIndexChange(int pageIndex)
        {
            this.pageIndex = pageIndex;
            await PageIndexSizeChange();
        }

        private bool drawerVisible = false;

        private BsUser editUser = new BsUser();

        async void SaveUser(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
        {
            string err = editUser.Validate();
            if (err != null)
            {
                await MessageService.ErrorAsync(err, 3);
            }
            else
            {
                try
                {
                    if (editUser.UserId == 0)
                    {
                        BsUser find = BsConfig.Users.Find(f => f.UserName == editUser.UserName || (!string.IsNullOrEmpty(editUser.FullName) && f.FullName == editUser.FullName));
                        if (find != null) throw new Exception("已经存在同名用户名或姓名");
                    }
                    else
                    {
                        BsUser find = BsConfig.Users.Find(f => f.UserId != editUser.UserId && (f.UserName == editUser.UserName || (!string.IsNullOrEmpty(editUser.FullName) && f.FullName == editUser.FullName)));
                        if (find != null) throw new Exception("已经存在同名用户名或姓名");
                    }

                    if (!string.IsNullOrEmpty(editUser.Password) && (editUser.Password.Length < 8 || !Utility.IsValidPassword(editUser.Password))) throw new Exception("密码至少要为8位且包含大小写和数字");
                    if (!string.IsNullOrEmpty(editUser.Password) && editUser.Password.Length >= 32) throw new Exception("密码长度不能大于32位");
                    this.editUser.LastEdit = DateTime.Now;
                    if (!string.IsNullOrEmpty(this.editUser.Password)) this.editUser.LastChangePwd = DateTime.Now;

                    BsConfig.AddOrUpdateUser(this.editUser);
                    if (editUser.UserId == 0) this.pageIndex = 0;
                    drawerVisible = false;
                    await this.PageIndexSizeChange();
                }
                catch (Exception ex)
                {
                    await MessageService.ErrorAsync(ex.Message, 3);
                }
            }
        }

        private async Task PageIndexSizeChange()
        {
            isloading = true;

            this.userrsp.Value = BsConfig.Users
                     .Skip((pageIndex - 1) * pageSize)
                     .Take(pageSize)
                     .OrderByDescending(x => x.UserId)
                     .ToList();
            this.userrsp.Total = BsConfig.Users.Count;

            this.showFullName = this.userrsp.Value.FindAll(f => !string.IsNullOrEmpty(f.FullName)).Count > 0;

            isloading = false;

            await base.InvokeAsync(base.StateHasChanged);
        }

        void CloseDrawer()
        {
            this.drawerVisible = false;
        }

        string editTitle = "添加用户";

        void NewDrawer()
        {
            editUser = new BsUser();
            this.drawerVisible = true;
            this.editTitle = "添加用户";
        }

        internal async Task OnEdit(BsUser user)
        {
            _userFragment = null;
            var json = JsonSerializer.Serialize(user);
            this.editUser = JsonSerializer.Deserialize<BsUser>(json);
            this.editUser.Password = "";
            this.drawerVisible = true;
            this.editTitle = "编辑用户";
            await base.InvokeAsync(base.StateHasChanged);
        }

        private RenderFragment _userFragment = null;

        internal async Task SetUserAuth(BsUser user)
        {
            _userFragment = null;
            var json = JsonSerializer.Serialize(user);
            BsUser bu = JsonSerializer.Deserialize<BsUser>(json);
            _userFragment = BsConfig.UserAuthFragment(bu, CloseUserAuth);
            await base.InvokeAsync(base.StateHasChanged);
        }

        // 关闭权限窗口的方法
        internal async Task CloseUserAuth()
        {
            _userFragment = null;
            await base.InvokeAsync(base.StateHasChanged);
        }
    }
}