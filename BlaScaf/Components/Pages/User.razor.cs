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
    public partial class User
    {
        ITable table;

        private bool isloading = false;

        [Inject] public AuthenticationStateProvider AuthProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await PageIndexSizeChange();
            await base.OnInitializedAsync();
        }

        private int pageIndex = 0;
        private int pageSize = 15;
        private List<BsUser> userlist = new List<BsUser>();


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
                msg.Error(err, 3);
            }
            else
            {
                try
                {
                    BsConfig.AddOrUpdateUser(this.editUser);
                    if (editUser.UserId == 0) this.pageIndex = 0;
                    drawerVisible = false;
                    await this.PageIndexSizeChange();
                }
                catch (Exception ex)
                {
                    msg.Error(ex.Message, 3);
                }
            }
        }

        private async Task PageIndexSizeChange()
        {
            isloading = true;

            this.userlist = BsConfig.Users
              .Skip((pageIndex - 1) * pageSize)
              .Take(pageSize)
              .OrderByDescending(x => x.UserId)
              .ToList();

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
            editUser = new BsUser() { Enable = true };
            this.drawerVisible = true;
            this.editTitle = "添加用户";
        }

        internal async Task OnEdit(BsUser user)
        {
            var json = JsonSerializer.Serialize(user);
            this.editUser = JsonSerializer.Deserialize<BsUser>(json);
            this.drawerVisible = true;
            this.editTitle = "编辑用户";
            await base.InvokeAsync(base.StateHasChanged);
        }
    }
}