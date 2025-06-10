namespace BlaScaf
{
    /// <summary>
    /// 系统相关日志
    /// </summary>
    [FreeSql.DataAnnotations.Table(Name = "bssyslog")]
    public class BsSysLog
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsPrimary = true, IsIdentity = true)]
        public int SysLogId { get; set; }

        /// <summary>
        /// 日志类型，如登录成功，登录失败
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 10)]
        public string LogType { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 100)]
        public string Message { get; set; }

        /// <summary>
        /// 日志详情
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = -1)]
        public string Details { get; set; }

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime SysTime { get; set; }
    }
}