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
        public static List<string> UserRoles { get; set; }

        /// <summary>
        /// 密码最小长度
        /// </summary>
        public static string PwdLessLength { get; set; }

        /// <summary>
        /// JWT 令牌的有效接收者（audience）
        /// </summary>
        public static string ValidAudience = Assembly.GetExecutingAssembly().FullName;

        /// <summary>
        /// JWT 令牌的颁发者（issuer）
        /// </summary>
        public static string ValidIssuer = "BlaServer"; // 例如："MyApp.Server"

        /// <summary>
        /// 用于对 JWT 令牌进行签名和验证的密钥（对称加密）
        /// </summary>
        public static string IssuerSigningKey = "9cacbe110527992a71295a3e4bfc3f2d";

    }
}