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

        public int UserId { get; private set; }
        public string UserName { get; private set; }
        public string Role { get; private set; }
        public string FullName { get; private set; }


        public UserService(AuthenticationStateProvider authProvider)
        {
            _authProvider = authProvider;
        }

        public async Task LoadUserInfoAsync()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                string token = user.FindFirst("Token")?.Value;
                if (string.IsNullOrEmpty(token) || token != BsConfig.Users.Find(f => f.Token == token)?.Token)
                {
                    UserId = 0;
                    UserName = null;
                    FullName = null;
                    Role = null;
                }
                else
                {
                    UserName = user.Identity.Name;
                    Role = user.FindFirst(ClaimTypes.Role)?.Value;
                    var userIdStr = user.FindFirst("UserId")?.Value;
                    UserId = int.TryParse(userIdStr, out var uid) ? uid : 0;
                }
            }
            else
            {
                // 未登录用户清空属性
                UserId = 0;
                UserName = null;
                FullName = null;
                Role = null;
            }
        }
    }
}