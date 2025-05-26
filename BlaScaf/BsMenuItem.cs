namespace BlaScaf
{
    /// <summary>
    /// 菜单选项
    /// </summary>
    public class BsMenuItem
    {
        public string Title { get; set; }

        public string Key { get; set; }

        public string Icon { get; set; }

        public string RouterLink { get; set; }

        /// <summary>
        /// 角色，属于的才显示
        /// </summary>
        public List<string> Roles { get; set; }
    }
}
