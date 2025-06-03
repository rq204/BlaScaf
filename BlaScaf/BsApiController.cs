using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

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
            BsUser bu = BsConfig.Users.Find(f => f.UserName == dto.UserName);
            if (bu != null)
            {
                if (BsSecurity.UserHashKey.ContainsKey(dto.UserName))
                {
                    string key = BsSecurity.UserHashKey[dto.UserName];
                    BsSecurity.UserHashKey.Remove(dto.UserName);

                    if (BsSecurity.PasswordError.TryGetValue(dto.UserName, out var errorList))
                    {
                        var thresholdTime = DateTime.Now.AddMinutes(-5);
                        var recentErrors = errorList.Where(t => t > thresholdTime).ToList();

                        if (recentErrors.Count >= 3)
                        {
                            var mostRecentErrorTime = recentErrors.Max();
                            var waitMinutes = (int)Math.Ceiling((mostRecentErrorTime.AddMinutes(5) - DateTime.Now).TotalMinutes);
                            waitMinutes = Math.Max(waitMinutes, 1); // 至少提示 1 分钟，避免显示 0

                            return Unauthorized($"密码多次错误，请{waitMinutes}分钟后再试");
                        }
                    }

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
                        BsSecurity.PasswordError.Remove(dto.UserName);

                        if (bu.EndTime > DateTime.Now)
                        {
                            var json = JsonSerializer.Serialize(bu);
                            BsUser user = JsonSerializer.Deserialize<BsUser>(json);
                            user.Token = Guid.NewGuid().ToString();
                            user.LastLogin = DateTime.Now;
                            BsConfig.AddOrUpdateUser(user);

                            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim("FullName",string.IsNullOrEmpty(user.FullName)?"":user.FullName),
                new Claim("UserId",user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Token",user.Token),
            };
                            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var authProperties = new AuthenticationProperties
                            {
                                //空为使用默认设置
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
                            return Unauthorized("帐号已过期");
                        }
                    }
                    else
                    {
                        if (!BsSecurity.PasswordError.TryGetValue(dto.UserName, out var list))
                        {
                            list = new List<DateTime>();
                            BsSecurity.PasswordError[dto.UserName] = list;
                        }
                        list.Add(DateTime.Now);

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

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="kicked">是否被挤下线</param>
        /// <returns></returns>
        [HttpGet("logout")]
        public async Task<IActionResult> Logout([FromQuery] bool kicked)
        {
            await HttpContext.SignOutAsync();
            if (kicked)
            {
                return Redirect("/");
            }
            else
            {
                return Ok();
            }
        }

        [HttpPost("keepalive")]
        public IActionResult KeepAlive()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return Ok(); // 会话有效
            }

            // 会话过期，清除认证 Cookie
            Response.Cookies.Delete(".AspNetCore.Cookies"); // ← 改成你配置的 Cookie 名

            return Unauthorized(); // 返回 401 Unauthorized
        }
    }
}
