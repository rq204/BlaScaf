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
        public string Role { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 邮件
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 最后密码修改时间
        /// </summary>
        public DateTime LastChangePwd { get; set; }

        /// <summary>
        /// 页面登录后的Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastLogin { get; set; }

        /// <summary>
        /// 注册ip
        /// </summary>
        public string RegIP { get; set; }

        /// <summary>
        /// 最后登录IP
        /// </summary>
        public string LastIP { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 扩展数据用
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = -2)]
        public string ExtStr { get; set; }


        internal string Validate()
        {
            if (string.IsNullOrEmpty(this.UserName)) return "用户名不能为空";
            if (string.IsNullOrEmpty(this.Password)) return "密码不能为空";
            if (string.IsNullOrEmpty(this.Role)) return "角色不能为空";
            return null;
        }
    }
}