using System.ComponentModel.DataAnnotations.Schema;

namespace BlaScaf
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [FreeSql.DataAnnotations.Table(Name = "bs_user")]
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
        /// 密码md5的
        /// </summary>
        [FreeSql.DataAnnotations.Column(StringLength = 32)]
        public string Password { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [FreeSql.DataAnnotations.Column(ServerTime = DateTimeKind.Local)]
        public DateTime AddTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// 邮件
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>

        public string Phone { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 最后密码修改时间
        /// </summary>
        [FreeSql.DataAnnotations.Column(ServerTime = DateTimeKind.Local)]
        public DateTime LastChangePwd { get; set; }

        /// <summary>
        /// 页面登录后的Token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [FreeSql.DataAnnotations.Column(ServerTime = DateTimeKind.Local)]
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
        [FreeSql.DataAnnotations.Column(ServerTime = DateTimeKind.Local)]
        public DateTime EndTime { get; set; }

    }
}