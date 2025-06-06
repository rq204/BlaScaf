namespace BlaScaf
{
    /// <summary>
    /// ///安全相关设置
    /// </summary>
    public class BsSecurity
    {
        /// <summary>
        /// 用户登录的随机值加密请求过程
        /// </summary>
        public static Dictionary<string, string> UserHashKey = new Dictionary<string, string>();

        /// <summary>
        /// 密码错误5分钟内可以错3次
        /// </summary>
        public static Dictionary<string, List<DateTime>> PasswordError = new Dictionary<string, List<DateTime>>();

        /// <summary>
        /// 同一IP下的登录错误10分钟可以错10次
        /// </summary>
        public static Dictionary<string, List<DateTime>> IPLoginError = new Dictionary<string, List<DateTime>>();

        /// <summary>
        /// 标准代理请求头
        /// </summary>
        public static string XForwardedFor = "X-Forwarded-For";

        /// <summary>
        /// 验证码和生成时间90秒超时
        /// </summary>
        public static Dictionary<string, DateTime> CaptchaCode = new Dictionary<string, DateTime>();
    }
}