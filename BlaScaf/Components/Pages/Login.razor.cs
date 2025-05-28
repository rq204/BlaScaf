using AntDesign;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using static System.Net.WebRequestMethods;
using System.Net;
using Microsoft.JSInterop;

namespace BlaScaf.Components.Pages
{
    public partial class Login : IAsyncDisposable
    {
        private BsUser loginModel = new();
        private bool isLoading = false;

        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; }
        [Inject] public MessageService MessageService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }

        private RenderFragment fragment = null;

        private async Task HandleLogin()
        {
            isLoading = true;

            if (BsConfig.Users.Find(f => f.Username == loginModel.Username) == null)
            {
                this.MessageService.Error("用户名或密码错误", 3);
            }
            else
            {
                BsUser dto = new BsUser() { Username = loginModel.Username, Token = loginModel.Token };
                dto.Password = Utility.MD5(loginModel.Password);
                string key = Guid.NewGuid().ToString("N");
                dto.Password = Utility.DESEncrypt(dto.Password, key.Substring(0, 8), key.Substring(8, 8));
                BsSecurity.UserHashKey[dto.Username] = key;

                await JSRuntime.InvokeVoidAsync("bsLogin", dto.Username, dto.Password, dto.Token, objRef);
            }
            isLoading = false;
            await InvokeAsync(StateHasChanged);
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
            isLoading = false;

            if (success)
            {
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
            else
            {
                MessageService.Error(message, 3);
            }

            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            objRef?.Dispose();
        }

    }
}