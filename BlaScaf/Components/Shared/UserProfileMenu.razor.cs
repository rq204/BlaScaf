using AntDesign;
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
            JSRuntime.InvokeVoidAsync("bsLogout");
        }


        protected override void OnInitialized()
        {
            this.showName = string.IsNullOrEmpty(UserService.FullName) ? UserService.UserName : UserService.FullName;
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
                await this.MessageService.ErrorAsync(message, 3);
            }

            StateHasChanged();
        }


        private bool changepwdVisible = false;
    }
}