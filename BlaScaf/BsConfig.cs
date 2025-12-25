using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Reflection;

namespace BlaScaf
{
    public class BsConfig
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public static string AppName = "BlaScaf信息管理系统";

        /// <summary>
        /// Cookie超时时间分钟
        /// </summary>
        public static int CookieTimeOutMinutes = 30;

        /// <summary>
        /// 使用session cookie
        /// </summary>
        public static bool UseSessionCookie = true;

        /// <summary>
        /// 多少天必须修改密码
        /// </summary>
        public static int ChangePwdDays = 365;

        /// <summary>
        /// 用户角色，添加用户时用户组中可以选择
        /// </summary>
        public static List<string> Roles = new List<string>();

        /// <summary>
        /// 菜单当中
        /// </summary>
        public static List<BsMenuItem> MenuItems = new List<BsMenuItem>();

        /// <summary>
        /// 所有的用户
        /// </summary>
        public static List<BsUser> Users = new List<BsUser>();

        /// <summary>
        /// 插入html的head中代码
        /// </summary>
        public static List<string> HeadInjectRawHtmls = new List<string>();

        /// <summary>
        /// 添加操作日志
        /// </summary>
        public static Action<BsOptLog> AddOptLog;

        /// <summary>
        /// 添加系统日志
        /// </summary>
        public static Action<BsSysLog> AddSysLog;

        /// <summary>
        /// 添加或更新用户信息,可以做一些检查
        /// 比如密码强度不够可以抛异常出去
        /// 在这个方法中要注意更新BsConfig.Users
        /// </summary>
        public static Action<BsUser> AddOrUpdateUser;

        /// <summary>
        /// 新增用户登录的处理已是验证过帐号密码
        /// </summary>
        public static Action<BsUser> AddLogin;

        /// <summary>
        /// 获取操作日志,PageIndex,PageSize,UserId
        /// </summary>
        public static Func<int, int, int, QueryRsp<List<BsOptLog>>> GetOptLogs;

        /// <summary>
        /// 获取系统日志,PageIndex,PageSize
        /// </summary>
        public static Func<int, int, QueryRsp<List<BsSysLog>>> GetSysLogs;

        /// <summary>
        /// 头部的外部的组件
        /// </summary>
        public static List<RenderFragment> HeaderFragments = new List<RenderFragment>();

        /// <summary>
        /// 验证码组件
        /// </summary>
        public static Func<RenderFragment> CaptchaFragment = null;

        /// <summary>
        /// 哪些用户组使用验证码
        /// </summary>
        public static List<string> CaptchaRoles = new List<string>();

        /// <summary>
        /// 用户相关权限设置
        /// </summary>
        public static Func<BsUser, Func<Task>, RenderFragment> UserAuthFragment = null;

        /// <summary>
        /// 不需要登录的页面
        /// </summary>
        public static HashSet<Type> AnonymousPages = new HashSet<Type>()
        {
           typeof(BlaScaf.Components.Pages.Login)
        };
    }
}