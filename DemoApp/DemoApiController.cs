using Microsoft.AspNetCore.Mvc;

namespace DemoApp
{
    /// <summary>
    /// 根级 WebAPI 示例：子目录模式下由 BsConfig.RootApiPrefixes = ["/api"] 放行，
    /// 前台静态前端（Vue3 等）直接调用 /api/demo/ping，不带 /root 前缀。
    /// 管理端 BlaScaf 自身的 api/login 等接口同样位于 /api 下，前后台共用。
    /// </summary>
    [Route("api/demo")]
    [ApiController]
    public class DemoApiController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { ok = true, time = DateTime.Now });
        }
    }
}
