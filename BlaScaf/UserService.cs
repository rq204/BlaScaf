namespace BlaScaf
{
    /// <summary>
    /// 当前登录用户信息
    /// </summary>
    public class UserService
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public List<string> Roles { get; set; } = new();

        public string Token { get; set; } = Guid.NewGuid().ToString();
    }
}
