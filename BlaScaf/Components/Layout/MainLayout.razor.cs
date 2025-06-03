
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
        }

    }
}
