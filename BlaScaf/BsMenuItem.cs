namespace BlaScaf
{
    /// <summary>
    /// 菜单选项
    /// </summary>
    public class BsMenuItem
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// antdesign图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 路由链接
        /// </summary>
        public string RouterLink { get; set; }

        /// <summary>
        /// 角色，属于的才显示
        /// </summary>
        public List<string> Roles { get; set; }
    }
}
