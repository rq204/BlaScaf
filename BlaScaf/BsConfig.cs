using System.Reflection;

namespace BlaScaf
{
    public class BsConfig
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public static string AppName { get; set; } = "BlaScaf信息管理系统";

        /// <summary>
        /// 用户角色，在用户组中可以选择
        /// </summary>
        public static List<string> UserRoles { get; set; } = new List<string>();

        /// <summary>
        /// 菜单当中
        /// </summary>
        public static List<BsMenuItem> MenuItems { get; set; } = new List<BsMenuItem>();

        /// <summary>
        /// Cookie超时时间分钟
        /// </summary>
        public static int CookieTimeOutMinutes { get; set; } = 30;

        /// <summary>
        /// 用于对 JWT 令牌进行签名和验证的密钥（对称加密）
        /// </summary>
        public static string JwtKey32 = "9cacbe110527992a71295a3e4bfc3f2d";

        /// <summary>
        /// 默认登录验证方法，有问题抛异常即可
        /// </summary>
        public static Func<BsUser, UserService> LoginAction = new Func<BsUser, UserService>(DemoSys.Login);
    }
}