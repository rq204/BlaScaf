using AntDesign;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
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
        private UserService userService;


        public BsAuthProvider(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime, UserService userService)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._jsRuntime = jsRuntime;
            this.userService = userService;
        }

        /// <summary>
        /// 获取用户状态
        /// </summary>
        /// <returns></returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null || !context.Request.Cookies.TryGetValue("blascaf", out var bstoken))
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            }

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = CreateTokenValidationParameters();
            SecurityToken securityToken; // 接受解码后的token对象
            var princ = tokenHandler.ValidateToken(bstoken, validationParameters, out securityToken);
            ///如果错误或过期
            if (princ == null || securityToken == null || securityToken.ValidTo > DateTime.Now)
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal()));
            }

            if (userService == null || string.IsNullOrEmpty(userService.Username))
            {
                userService = new UserService();
                userService.Username = princ.Claims.First(x => x.Type == "Username").Value;
                userService.UserId = int.Parse(princ.Claims.First(x => x.Type == "UserId").Value);
                userService.Token = princ.Claims.First(x => x.Type == "Token").Value;
                userService.Roles = princ.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            }

            ///5分钟一更新jwt
            DateTime upcookieTime = securityToken.ValidTo.AddMinutes(5 - BsConfig.CookieTimeOutMinutes);
            if (upcookieTime > DateTime.Now)
            {
                    var newToken = BsAuthProvider.CreateToken(this.userService);
                context.Response.Cookies.Append("blascaf", newToken, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                    HttpOnly = true,
                    Secure = true
                });
            }
            return Task.FromResult(new AuthenticationState(princ));
        }

        public static TokenValidationParameters CreateTokenValidationParameters()
        {
            var symmetricKey = System.Text.Encoding.UTF8.GetBytes(BsConfig.JwtKey32); // 生成编码对应的字节数组
            var validationParameters = new TokenValidationParameters() // 生成验证token的参数
            {
                RequireExpirationTime = true, // token是否包含有效期
                ValidateLifetime = true,//验证过期时间
                ValidateIssuer = true, // 验证秘钥发行人，如果要验证在这里指定发行人字符串即可
                ValidateAudience = true, // 验证秘钥的接受人，如果要验证在这里提供接收人字符串即可
                ValidAudience = validAudience,//Audience
                ValidIssuer = "BlaScaf",//Issuer，这两项和签发jwt的设置一致
                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey) // 生成token时的安全秘钥
            };
            return validationParameters;
        }

        /// <summary>
        /// 标记用户为已认证
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task MarkUserAsAuthenticated(UserService user)
        {
            var claims = new List<Claim>
        {
            new Claim("Username", user.Username),
            new Claim("UserId", user.UserId.ToString()),
             new Claim("Token", user.Token)
        };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims));
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(authenticatedUser)));

            string jwtoken = CreateToken(user);

            // 可选（推荐使用服务端设置 Cookie）
            await _jsRuntime.InvokeVoidAsync("SetCookie", "blascaf", jwtoken);
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public async Task MarkUserAsLoggedOut()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal())));
            await _jsRuntime.InvokeVoidAsync("SetCookie", "blascaf", "");
        }

        private static string validAudience = Assembly.GetExecutingAssembly().FullName;

        /// <summary>
        /// 生成JwtToken
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        internal static string CreateToken(UserService user)
        {
            //此处加入账号密码验证代码
            var claims = new List<Claim>()
            {
            new Claim("Username",user.Username),
              new Claim("UserId",user.UserId.ToString()),
            new Claim("Token",user.Token),
            };

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(BsConfig.JwtKey32));
            var expires = DateTime.Now.AddMinutes(BsConfig.CookieTimeOutMinutes);
            var token = new JwtSecurityToken(
                issuer: "BlaScaf",
                audience: validAudience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: expires,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
