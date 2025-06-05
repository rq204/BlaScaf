using System.ComponentModel.DataAnnotations.Schema;

namespace BlaScaf
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [FreeSql.DataAnnotations.Table(Name = "bsuser")]
    [FreeSql.DataAnnotations.Index("bs_username", "UserName", IsUnique = true)]
    public class BsUser
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [FreeSql.DataAnnotations.Column(IsPrimary = true, IsIdentity = true)]
        public int UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 30)]
        public string UserName { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 20)]
        public string FullName { get; set; }

        /// <summary>
        /// 密码md5的
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 32)]
        public string Password { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        public DateTime AddTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 角色
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 10)]
        public string Role { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 11)]
        public string Phone { get; set; }

        /// <summary>
        /// 邮件
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 50)]
        public string Email { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 最后密码修改时间
        /// </summary>
        public DateTime LastChangePwd { get; set; } = DateTime.Now;

        /// <summary>
        /// 页面登录后的Token
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 50)]
        public string Token { get; set; }

        /// <summary>
        /// 注册ip
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 39)]
        public string RegIP { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime LastLogin { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEdit { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 最后登录IP
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 39)]
        public string LastIP { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.Now.AddYears(100);

        /// <summary>
        /// 扩展字段1
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 200)]
        public string ExtField1 { get; set; }

        /// <summary>
        /// 扩展字段2
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 200)]
        public string ExtField2 { get; set; }

        /// <summary>
        /// 扩展字段3
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 200)]
        public string ExtField3 { get; set; }

        /// <summary>
        /// 扩展字段4
        /// </summary>
        public int ExtField4 { get; set; }

        /// <summary>
        /// 扩展数据用，使用Json保存数据
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = -2)]
        public string ExtJson { get; set; }

        internal string Validate()
        {
            if (string.IsNullOrEmpty(this.UserName)) return "用户名不能为空";
            if (this.UserName.Length < 3 || this.UserName.Length > 24) return "用户名长度应为3至24位";
            if (!System.Text.RegularExpressions.Regex.IsMatch(this.UserName, "^[A-Za-z0-9]+$")) return "用户名只能为大小写字母和数字";

            if (this.UserId == 0 && string.IsNullOrEmpty(this.Password)) return "密码不能为空";
            if (string.IsNullOrEmpty(this.Role)) return "角色不能为空";
            return null;
        }
    }
}