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

        private CancellationTokenSource _cts = new();
        Timer _timer;
        protected override async Task OnInitializedAsync()
        {
        }

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
            objRef = DotNetObjectReference.Create(this);
            // 每 30 秒检查一次
            _timer = new Timer(async _ => await CheckSession(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

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
            _timer?.Dispose();
        }

        private bool changepwdVisible = false;

        private async Task CheckSession()
        {
            await this.UserService.LoadUserInfoAsync();
            if (this.UserService.UserId == 0)
            {
                ///todo 问题没找到
                try
                {
                    NavigationManager.NavigateTo("/bsapi/logout?kicked=true", forceLoad: true);
                }
                catch { }
            }
        }
    }
}