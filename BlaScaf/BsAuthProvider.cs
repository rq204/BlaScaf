using AntDesign;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace BlaScaf
{
    /// <summary>
    /// JwtToken的验证
    /// </summary>
    public class BsAuthProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJSRuntime _jsRuntime;

        public BsAuthProvider(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime)
        {
            _httpContextAccessor = httpContextAccessor;
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// 获取用户状态
        /// </summary>
        /// <returns></returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null || !context.Request.Cookies.TryGetValue("JwtToken", out var token))
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            }

            var result = new BsUser(); //AccountManager.GetAccount(token);
            if (result == null || string.IsNullOrEmpty(result.UserName))
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            }
      
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, result.UserName),
        new Claim(ClaimTypes.Role, result.Role),
        new Claim("UserId", result.UserId.ToString())
    };

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "apiauth"));
            return Task.FromResult(new AuthenticationState(user));
        }

        /// <summary>
        /// 标记用户为已认证
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task MarkUserAsAuthenticated(BsUser user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserId", user.UserId.ToString())
        };

            foreach (var role in user.Role.Split(','))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "apiauth"));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));

            // 可选（推荐使用服务端设置 Cookie）
            await _jsRuntime.InvokeVoidAsync("SetCookie", "JwtToken", user.Token);
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public async Task MarkUserAsLoggedOut()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
            await _jsRuntime.InvokeVoidAsync("SetCookie", "JwtToken", "");
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        { // 此方法用解码字符串token，并返回秘钥的信息对象
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler(); // 创建一个JwtSecurityTokenHandler类，用来后续操作
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken; // 将字符串token解码成token对象
                if (jwtToken == null)
                    return null;
                var symmetricKey = System.Text.Encoding.UTF8.GetBytes(BsConfig.IssuerSigningKey); // 生成编码对应的字节数组
                var validationParameters = new TokenValidationParameters() // 生成验证token的参数
                {
                    RequireExpirationTime = true, // token是否包含有效期
                    ValidateIssuer = true, // 验证秘钥发行人，如果要验证在这里指定发行人字符串即可
                    ValidateAudience = true, // 验证秘钥的接受人，如果要验证在这里提供接收人字符串即可
                    ValidAudience = BsConfig.ValidAudience,//Audience
                    ValidIssuer = BsConfig.ValidIssuer,//Issuer，这两项和签发jwt的设置一致
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey) // 生成token时的安全秘钥
                };
                SecurityToken securityToken; // 接受解码后的token对象
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
                return principal; // 返回秘钥的主体对象，包含秘钥的所有相关信息
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 生成JwtToken
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static string CreateToken(BsUser user)
        {
            //此处加入账号密码验证代码
            var claims = new Claim[]
            {
            new Claim(ClaimTypes.Name,user.UserName),
            new Claim(ClaimTypes.Role,user.Role),
            new Claim("Token",user.Token),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(BsConfig.IssuerSigningKey));
            var expires = DateTime.Now.AddDays(30);
            var token = new JwtSecurityToken(
                issuer: BsConfig.ValidIssuer,
                audience: BsConfig.ValidAudience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
