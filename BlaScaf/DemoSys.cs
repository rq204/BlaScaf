namespace BlaScaf
{

    public class DemoSys
    {
        public static UserService Login(BsUser bsUser)
        {
            if (bsUser.Username != "admin" && bsUser.Password != "admin")
            {
                throw new Exception("用户名或密码错误");
            }
            UserService userService = new UserService();
            userService.Username= bsUser.Username;
            userService.UserId= bsUser.UserId;
            userService.Roles = new List<string>() { "admin" };

            return userService;
        }
    }
}
