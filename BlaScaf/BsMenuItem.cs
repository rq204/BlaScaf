namespace BlaScaf
{
    public class BsMenuItem
    {
        public string Title { get; set; }

        public string Key { get; set; }

        public string Icon { get; set; }

        public string RouterLink { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public List<string> Roles { get; set; }
    }
}
