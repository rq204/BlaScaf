using AntDesign;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using static System.Net.WebRequestMethods;
using System.Net;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;

namespace BlaScaf.Components.Pages
{
    public partial class Login : IAsyncDisposable
    {
        private BsUser loginModel = new();
        private bool isLoading = false;
        private bool isDisposed = false; // 添加disposed标志

        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; }
        [Inject] public MessageService MessageService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        private RenderFragment fragment = null;

        private async Task HandleLogin()
        {
            if (isDisposed) return; // 检查是否已销毁

            isLoading = true;

            if (BsConfig.Users.Find(f => f.UserName == loginModel.UserName) == null)
            {
                if (!isDisposed) // 检查是否已销毁
                {
                    await this.MessageService.ErrorAsync("用户名或密码错误", 3);
                }
            }
            else
            {
                BsUser dto = new BsUser() { UserName = loginModel.UserName, Token = loginModel.Token };
                dto.Password = Utility.MD5(loginModel.Password);
                string key = Guid.NewGuid().ToString("N");
                dto.Password = Utility.DESEncrypt(dto.Password, key.Substring(0, 8), key.Substring(8, 8));
                BsSecurity.UserHashKey[dto.UserName] = key;

                if (!isDisposed) // 检查是否已销毁
                {
                    await JSRuntime.InvokeVoidAsync("bsLogin", dto.UserName, dto.Password, dto.Token, objRef);
                }
            }

            if (!isDisposed) // 检查是否已销毁
            {
                isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            if (BsConfig.CaptchaFragment != null) fragment = BsConfig.CaptchaFragment();
        }

        private DotNetObjectReference<Login> objRef;

        protected override void OnInitialized()
        {
            objRef = DotNetObjectReference.Create(this);
        }

        [JSInvokable]
        public async Task OnLoginResult(bool success, string message)
        {
            // 检查组件是否已经被销毁
            if (isDisposed) return;

            isLoading = false;

            if (success)
            {
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
            else
            {
                await MessageService.ErrorAsync(message, 3);
            }

            // 安全地调用StateHasChanged
            try
            {
                if (!isDisposed)
                {
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (ObjectDisposedException)
            {
                // 组件已被销毁，忽略此异常
            }
        }

        public async ValueTask DisposeAsync()
        {
            isDisposed = true; // 设置销毁标志
            objRef?.Dispose();
        }
    }
}