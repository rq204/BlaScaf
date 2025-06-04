
using AntDesign;
using BlaScaf.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

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
        List<RenderFragment> fragments = new List<RenderFragment>();

        protected override void OnInitialized()
        {
            objRef = DotNetObjectReference.Create(this);
            this.fragments = BsConfig.HeaderFragments.ToArray().ToList();
        }

        protected override async Task OnInitializedAsync()
        {
            objRef = DotNetObjectReference.Create(this);
            this.fragments = BsConfig.HeaderFragments.ToArray().ToList();

            // 首先加载用户信息
            await CheckSession();

            UpdatePageTitle(this.NavigationManager.Uri);

            NavigationManager.LocationChanged += OnLocationChanged;
            // 每 30 秒检查一次
            _timer = new Timer(async _ => await CheckSession(), null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
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
            BsMenuItem bsMenu = BsConfig.MenuItems.Find(f => f.RouterLink == relativePath);
            NavTitle = bsMenu?.Title;

            ///权限不足
            if (bsMenu == null || (this.UserService.Role != null && !bsMenu.Roles.Contains(this.UserService.Role)))
            {
                NavigationManager.NavigateTo("/", forceLoad: true);
            }
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
            objRef?.Dispose();
            _timer?.Dispose();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // 初始化 JavaScript 交互
                await JSRuntime.InvokeVoidAsync("keepAlive", BsConfig.CookieTimeOutMinutes, objRef);
            }
        }

        private DotNetObjectReference<MainLayout> objRef;

        [JSInvokable]
        public async Task OnTimeOut()
        {
            NavigationManager.NavigateTo("/", forceLoad: true);
        }

        public async ValueTask DisposeAsync()
        {
            objRef?.Dispose();
            _timer?.Dispose();
        }

        Timer _timer;
        private bool changepwdVisible = false;
        private bool onlyfirstchange = true;
        private async Task CheckSession()
        {
            await this.UserService.LoadUserInfoAsync();
            if (this.UserService.UserId == 0)
            {
                NavigationManager.NavigateTo("/bsapi/logout?kicked=true", forceLoad: true);
            }
            else if (onlyfirstchange)
            {
                onlyfirstchange = false;
                BsUser bu = BsConfig.Users.Find(f => f.UserId == this.UserService.UserId);
                if (bu != null && bu.LastChangePwd.AddDays(BsConfig.ChangePwdDays) < DateTime.Now)
                {
                    changepwdVisible = true;
                }
            }
            else
            {
                BsUser bu = BsConfig.Users.Find(f => f.UserId == this.UserService.UserId);
                if (bu != null && bu.EndTime < DateTime.Now)
                {
                    NavigationManager.NavigateTo("/bsapi/logout?kicked=true", forceLoad: true);
                }
            }
        }
    }
}
