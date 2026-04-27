using AntDesign;
using BlaScaf.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlaScaf.Components.Layout
{
    public partial class MainLayout : IAsyncDisposable
    {
        [Inject] public UserService UserService { get; set; }

        private bool collapsed = false;

        private void ToggleSidebar()
        {
            collapsed = !collapsed;
        }

        private string NavTitle = "首页";

        protected override async Task OnInitializedAsync()
        {
            await UserService.LoadUserInfoAsync();
            if (UserService.UserId == 0)
            {
                NavigationManager.NavigateTo("/login", forceLoad: true);
                return;
            }

            //先更改密码
            BsUser bu = BsConfig.Users.Find(f => f.UserId == UserService.UserId);
            if (BsConfig.ChangePwdDays > 0 && bu.LastChangePwd.AddDays(BsConfig.ChangePwdDays) < DateTime.Now)
            {
                changepwdVisible = true;
            }

            UpdatePageTitle(this.NavigationManager.Uri);

            NavigationManager.LocationChanged += OnLocationChanged;
        }


        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            UpdatePageTitle(e.Location);
            StateHasChanged();
        }

        private void UpdatePageTitle(string uri)
        {
            var ui = new Uri(NavigationManager.Uri);
            //currentPath = uri.AbsolutePath; // 例如 "/users"
            var relativePath = ui.AbsolutePath;// NavigationManager.ToBaseRelativePath(uri);
            var bsMenu = FindMenuByRoute(BsConfig.MenuItems, relativePath);
            NavTitle = bsMenu?.Title;

            ///权限不足
            if (bsMenu == null || (this.UserService.Role != null && !bsMenu.Roles.Contains(this.UserService.Role)))
            {
                NavigationManager.NavigateTo("/api/denied", forceLoad: true);
            }
        }

        private BsMenuItem FindMenuByRoute(List<BsMenuItem> menus, string route)
        {
            foreach (var menu in menus)
            {
                // 当前节点匹配
                if (!string.IsNullOrEmpty(menu.RouterLink) &&
                    menu.RouterLink.Equals(route, StringComparison.OrdinalIgnoreCase))
                {
                    return menu;
                }

                // 子节点递归查找
                if (menu.Children != null && menu.Children.Any())
                {
                    var found = FindMenuByRoute(menu.Children, route);
                    if (found != null)
                        return found;
                }
            }

            return null;
        }

        private IJSObjectReference jsRef;
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // 不能超过js的最长时间24天
                int timemin = BsConfig.CookieTimeOutMinutes > 34560 ? 34560 : BsConfig.CookieTimeOutMinutes;

                // 初始化 JavaScript 交互
                jsRef = await JSRuntime.InvokeAsync<IJSObjectReference>(
           "keepAlive", timemin);
                await JSRuntime.InvokeVoidAsync("setTitle", BsConfig.AppName);
            }
        }


        public async ValueTask DisposeAsync()
        {
            _timer?.Dispose();
            NavigationManager.LocationChanged -= OnLocationChanged;
            if (jsRef != null)
            {
                try
                {
                    await jsRef.InvokeVoidAsync("dispose");
                    await jsRef.DisposeAsync();
                }
                catch
                {

                }
            }
        }

        Timer _timer;
        private bool changepwdVisible = false;
    }
}