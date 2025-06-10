using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlaScaf
{
    /// <summary>
    /// 当前登录用户信息
    /// </summary>
    public class UserService
    {
        private readonly AuthenticationStateProvider _authProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public int UserId { get; private set; }
        public string UserName { get; private set; }
        public string Role { get; private set; }
        public string FullName { get; private set; }
        public string Token { get; private set; }
        public string IP { get; private set; }

        public UserService(AuthenticationStateProvider authProvider, IHttpContextAccessor httpContextAccessor)
        {
            _authProvider = authProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LoadUserInfoAsync()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            UserId = 0;
            UserName = null;
            FullName = null;
            Role = null;
            Token = null;

            //获取客户端IP
            IP = Utility.GetRealClientIP(_httpContextAccessor.HttpContext);

            if (user.Identity?.IsAuthenticated == true)
            {
                string token = user.FindFirst("Token")?.Value;
                UserName = user.Identity.Name;
                Role = user.FindFirst(ClaimTypes.Role)?.Value;
                var userIdStr = user.FindFirst("UserId")?.Value;
                UserId = int.TryParse(userIdStr, out var uid) ? uid : 0;
                Token = token;
                FullName = user.FindFirst("FullName")?.Value;
            }
        }
    }
}