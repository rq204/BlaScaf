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
        /// 用户角色，在用户组中可以选择
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
        /// 添加或更新用户信息
        /// </summary>
        public static Action<BsUser> AddOrUpdateUser;

        /// <summary>
        /// Cookie超时时间分钟
        /// </summary>
        public static int CookieTimeOutMinutes = 30;

        /// <summary>
        /// 头部的外部的组件
        /// </summary>
        public static List<RenderFragment> HeaderFragments = new List<RenderFragment>();

        /// <summary>
        /// 验证码组件
        /// </summary>
        public static Func<RenderFragment> CaptchaFragment = null;
    }
}