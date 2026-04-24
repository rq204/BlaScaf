using BlaScaf.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace BlaScaf
{
    public class BsAuthStateProvider
        : RevalidatingServerAuthenticationStateProvider
    {
        public BsAuthStateProvider(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        protected override TimeSpan RevalidationInterval
            => TimeSpan.FromSeconds(30);

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState,
            CancellationToken cancellationToken)
        {
            var user = authenticationState.User;

            if (!user.Identity.IsAuthenticated)
                return false;

            // 👉 这里调用你的 CheckSession 逻辑
            var userIdStr = user.FindFirst("UserId")?.Value;
            var token = user.FindFirst("Token")?.Value;

            if (string.IsNullOrEmpty(userIdStr)) return false;
            if (string.IsNullOrEmpty(token)) return false;

            if (!int.TryParse(userIdStr, out int userId) || userId == 0) return false;

            BsUser bu = BsConfig.Users.Find(f => f.UserId == userId);

            ///token不存在要T出去
            if (bu == null || bu.Token != token)
            {
                return false;
            }
            else
            {
                if (bu.EndTime < DateTime.Now)//超过使用期限也T出去
                {
                    return false;
                }
            }

            return true;
        }
    }
}
