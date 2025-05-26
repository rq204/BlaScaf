using AntDesign;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace BlaScaf.Components.Pages
{
    public partial class Login
    {
        private BsUser loginModel = new();
        private bool isLoading = false;
        [Inject] public MessageService msgSrv { get; set; }

        private string captchaImage = "/captcha?ts=" + DateTime.Now.Ticks;

        [Inject] public AuthenticationStateProvider auth { get; set; }

        private async Task HandleLogin()
        {
            isLoading = true;
            try
            {
                UserService us = BsConfig.LoginAction(loginModel); ;
                msgSrv.Info("登录成功", 1);
                await ((BsAuthProvider)auth).MarkUserAsAuthenticated(us);
            }
            catch (Exception ex)
            {
                msgSrv.Info("登录失败:" + ex.Message, 3);
            }
            isLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }
}