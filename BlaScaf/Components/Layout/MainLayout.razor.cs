
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

        private string PageTitle = "首页";
        List<RenderFragment> fragments = new List<RenderFragment>();

        protected override void OnInitialized()
        {
            objRef = DotNetObjectReference.Create(this);
            NavigationManager.LocationChanged += OnLocationChanged;
            UpdatePageTitle(NavigationManager.Uri);
            this.fragments = BsConfig.HeaderFragments.ToArray().ToList();
            // 每 30 秒检查一次
            _timer = new Timer(async _ => await CheckSession(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            UpdatePageTitle(e.Location);
            StateHasChanged();
        }

        private void UpdatePageTitle(string uri)
        {
            var relativePath = NavigationManager.ToBaseRelativePath(uri).TrimEnd('/');
            PageTitle = BsConfig.MenuItems.Find(f => f.RouterLink.Trim('/') == relativePath)?.Title;
        }

        public void Dispose()
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
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
                ///todo 问题没找到
                try
                {
                    NavigationManager.NavigateTo("/bsapi/logout?kicked=true", forceLoad: true);
                }
                catch { }
            }
            else if (onlyfirstchange)
            {
                onlyfirstchange = false;
                BsUser bu = BsConfig.Users.Find(f => f.UserId == this.UserService.UserId);
                if (bu != null && bu.LastChangePwd < DateTime.Now)
                {
                    changepwdVisible = true;
                }
            }
            else
            {
                BsUser bu = BsConfig.Users.Find(f => f.UserId == this.UserService.UserId);
                if (bu != null && bu.EndTime < DateTime.Now)
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
}
