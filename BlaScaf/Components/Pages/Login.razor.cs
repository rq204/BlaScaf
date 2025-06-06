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

            BsUser bu = BsConfig.Users.Find(f => f.UserName == loginModel.UserName);
            if (bu == null)
            {
                if (!isDisposed) // 检查是否已销毁
                {
                    await this.MessageService.ErrorAsync("用户名或密码错误", 3);
                }
            }
            else
            {
                ///如果部分人要验证码，点击时出验证码
                if (BsConfig.CaptchaRoles.Contains(bu.Role) && BsConfig.CaptchaFragment != null && this.fragment == null)
                {
                    fragment = BsConfig.CaptchaFragment();
                }
                else if (BsConfig.CaptchaRoles.Contains(bu.Role) && BsConfig.CaptchaFragment != null && this.fragment != null && string.IsNullOrEmpty(loginModel.Token))
                {
                    await this.MessageService.ErrorAsync("验证码不能为空", 3);
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
            }

            if (!isDisposed) // 检查是否已销毁
            {
                isLoading = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            ///如果所有人要验证码，就直接显示
            if (BsConfig.CaptchaFragment != null && BsConfig.CaptchaRoles.Count > 0 && AreListsEqual(BsConfig.Roles, BsConfig.CaptchaRoles))
            {
                fragment = BsConfig.CaptchaFragment();
            }
        }

        public static bool AreListsEqual(List<string> list1, List<string> list2)
        {
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            var grouped1 = list1.GroupBy(x => x)
                                .ToDictionary(g => g.Key, g => g.Count());

            var grouped2 = list2.GroupBy(x => x)
                                .ToDictionary(g => g.Key, g => g.Count());

            return grouped1.Count == grouped2.Count &&
                   grouped1.All(kvp => grouped2.TryGetValue(kvp.Key, out var count) && count == kvp.Value);
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

        private bool showcaptha = false;
    }
}