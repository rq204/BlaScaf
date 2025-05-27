using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlaScaf
{
    [Microsoft.AspNetCore.Mvc.Route("bsapi")]
    [ApiController]
    public class BsApiController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] BsUser dto)
        {
            BsUser bu = BsConfig.Users.Find(f => f.Username == dto.Username);
            if (bu != null)
            {
                if (BsSecurity.UserHashKey.ContainsKey(dto.Username))
                {
                    string key=BsSecurity.UserHashKey[dto.Username];
                    BsSecurity.UserHashKey.Remove(dto.Username);

                    string md5pw = null;
                    try
                    { 
                        md5pw = Utility.DESDecrypt(dto.Password, key.Substring(0, 8), key.Substring(8, 8));
                    }
                    catch
                    {

                    }

                    if (md5pw == bu.Password)
                    {
                        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,bu.Username),
                new Claim("UserId",bu.UserId.ToString()),
                new Claim(ClaimTypes.Role, bu.Role)
            };
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                        };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties
                        );
                        return Ok();
                    }
                    else
                    {
                        return Unauthorized("用户名或密码错误");
                    }
                }
                else
                {
                    return Unauthorized("用户信息校验失败");
                }
            }
            else
            {
                return Unauthorized("用户名或密码错误");
            }
        }
    }
}
