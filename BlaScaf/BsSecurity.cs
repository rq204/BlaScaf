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
        public static Dictionary<string,List<DateTime>> PasswordError=new Dictionary<string, List<DateTime>>();
    }
}
