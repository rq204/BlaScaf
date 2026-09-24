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
        /// 用户编辑页的扩展字段配置，字段值直接绑定到 BsUser 对应属性
        /// </summary>
        public static List<BsUserEditorField> UserEditorFields = new List<BsUserEditorField>();

        /// <summary>
        /// 不需要登录的页面
        /// </summary>
        public static HashSet<Type> AnonymousPages = new HashSet<Type>()
        {
           typeof(BlaScaf.Components.Pages.Login)
        };

        /// <summary>
        /// 路由链接，不显示在菜单当中
        /// </summary>
        public static List<BsMenuItem> RouterLinkPages = new List<BsMenuItem>();

        /// <summary>
        /// 数据库管理页允许操作的实体类型，留空时自动扫描所有带 Table 特性的实体
        /// </summary>
        public static List<Type> DbAdminEntityTypes = new List<Type>();

        /// <summary>
        /// 设置浏览器标题，传入的是导航标题
        /// </summary>
        public static Func<string, string> SetBrowserTitle;

        /// <summary>
        /// 子目录访问前缀（不带斜杠，如 "root"）。
        /// 设置后管理端限定在该子目录下访问：
        /// - /root/login、/root/users 等正常访问，子目录下存在的文件正常返回
        /// - 根目录下 wwwroot 中存在的文件仍可直接访问（供 Vue3 等前台静态站点部署），
        ///   "/" 返回 wwwroot/index.html 作为前台入口
        /// - 根目录下 RootApiPrefixes 中配置的 API 前缀放行（供前台前端调用根级 WebAPI）
        /// - 其余根目录请求（如管理端页面 /login、不存在的文件）返回 404，不重定向
        /// - 子目录下不存在的文件/页面返回 Blazor 默认的 404
        /// 为空（默认）表示只在根目录下访问，保持旧行为。
        /// 改此配置即可切换访问网址，无需改动页面代码。
        /// </summary>
        public static string PathBase = "";

        /// <summary>
        /// 子目录访问模式（配置了 PathBase）下，允许从根目录（不带子目录前缀）访问的
        /// WebAPI 路由前缀列表，如 "/api"。
        /// 典型场景：前台是部署在 wwwroot 的 Vue3 静态站点，直接调用根级 /api/* 接口；
        /// 管理端则通过 /root/* 访问。
        /// 前缀按路径段匹配（"/api" 匹配 /api 与 /api/xxx，不匹配 /apixxx），不区分大小写。
        /// 仅在配置了 PathBase 时生效；未配置 PathBase 时所有接口本来就在根目录下。
        /// </summary>
        public static List<string> RootApiPrefixes = new List<string>();

        /// <summary>
        /// 把根路径转换为带子目录前缀的完整路径，例如 PathBase="root" 时 "/login" → "/root/login"。
        /// 未配置 PathBase 时原样返回。
        /// </summary>
        /// <param name="path">以 / 开始的根路径</param>
        public static string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(PathBase) || string.IsNullOrWhiteSpace(path)) return path;
            return "/" + PathBase.Trim('/') + (path.StartsWith("/") ? path : "/" + path);
        }

        /// <summary>
        /// 把带子目录前缀的路径还原为根路径，例如 PathBase="root" 时 "/root/login" → "/login"。
        /// 未配置 PathBase 或路径不带前缀时原样返回。
        /// </summary>
        /// <param name="path">浏览器中的绝对路径</param>
        public static string ToRootPath(string path)
        {
            if (string.IsNullOrWhiteSpace(PathBase) || string.IsNullOrWhiteSpace(path)) return path;

            var prefix = "/" + PathBase.Trim('/');
            if (path.Equals(prefix, StringComparison.OrdinalIgnoreCase)) return "/";

            if (path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(prefix.Length);
            }

            return path;
        }
    }

    /// <summary>
    /// 用户编辑页扩展字段的控件类型
    /// </summary>
    public enum BsUserEditorFieldControlType
    {
        String,
        Int,
        Bool,
        ListString
    }

    /// <summary>
    /// 用户编辑页扩展字段定义
    /// </summary>
    public class BsUserEditorField
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 绑定到 BsUser 的字段名，例如 ExtField1
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 控件类型
        /// </summary>
        public BsUserEditorFieldControlType ControlType { get; set; }

        /// <summary>
        /// 列表类控件的可选值提供器，仅在 ListString 时使用
        /// </summary>
        public Func<List<string>> GetListValues { get; set; }
    }
}
