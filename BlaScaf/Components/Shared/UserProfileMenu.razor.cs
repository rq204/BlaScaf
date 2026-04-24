using AntDesign;
using BlaScaf.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlaScaf.Components.Shared
{
    public partial class UserProfileMenu : IDisposable
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

        private async Task OnLogout()
        {
            var result = await JSRuntime.InvokeAsync<BsJsMsg>("bsLogout");

            if (result.Success)
            {
                NavigationManager.NavigateTo("/", true); // 强制刷新
            }
            else
            {
                await this.MessageService.ErrorAsync(result.Message, 3);
            }
        }


        protected override void OnInitialized()
        {
            this.showName = string.IsNullOrEmpty(UserService.FullName) ? UserService.UserName : UserService.FullName;
        }

        private string showName = "";

        public void Dispose()
        {
            
        }

        private bool changepwdVisible = false;
    }
}