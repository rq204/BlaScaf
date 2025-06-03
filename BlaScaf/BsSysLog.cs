namespace BlaScaf
{
    /// <summary>
    /// 系统相关日志，比如登录
    /// </summary>
    public class BsSysLog
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public int SysLogId { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public int SysLogType { get; set; } 

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime SysTime { get; set; }
    }
}
