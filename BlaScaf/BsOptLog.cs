using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaScaf
{
    /// <summary>
    /// 用户操作日志
    /// </summary>
    [FreeSql.DataAnnotations.Table(Name = "bsoptlog")]
    [FreeSql.DataAnnotations.Index("UK_UserId", "UserId", false)]
    [FreeSql.DataAnnotations.Index("UK_OptTime_OptType", "OptTime,OptType", false)]
    public class BsOptLog
    {
        /// <summary>
        /// 操作日志ID
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsPrimary = true, IsIdentity = true)]
        public int OptLogId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsIgnore = true)]
        public string FullName { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OptTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 操作类型
        /// </summary>
        [FreeSql.DataAnnotations.Column(MapType = typeof(int))]
        public int OptType { get; set; }

        /// <summary>
        /// 操作的对像的Id
        /// </summary>
        public int OptObjId { get; set; }

        /// <summary>
        /// 操作内容,长字符
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = -2)]
        public string OptData { get; set; }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string Summary { get; set; }
    }
}