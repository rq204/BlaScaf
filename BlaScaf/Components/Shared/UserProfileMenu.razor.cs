﻿using AntDesign;
using BlaScaf.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlaScaf.Components.Shared
{
    public partial class UserProfileMenu : IAsyncDisposable
    {
        [Inject] public UserService UserService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public MessageService MessageService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        private void OnChangePassword()
        {
            // 导航到修改密码页（根据你的实际路由）
            this.changepwdVisible = true;
        }

        private void OnLogout()
        {
            JSRuntime.InvokeVoidAsync("bsLogout", objRef);
        }

        private DotNetObjectReference<UserProfileMenu> objRef;

        protected override void OnInitialized()
        {
            this.showName = string.IsNullOrEmpty(UserService.FullName) ? UserService.UserName : UserService.FullName;
            objRef = DotNetObjectReference.Create(this);
        }

        private string showName = "";

        [JSInvokable]
        public async Task OnLogoutResult(bool success, string message)
        {
            if (success)
            {
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
            else
            {
                this.MessageService.Error(message, 3);
            }

            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            objRef?.Dispose();
        }

        private bool changepwdVisible = false;
    }
}