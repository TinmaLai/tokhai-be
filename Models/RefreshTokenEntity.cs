namespace z76_backend.Models
{
    [TableName("RefreshToken")]
    public class RefreshTokenEntity
    {
        public Guid user_id { get; set; }
        public string token { get; set; }
        public DateTime expires { get; set; }
        public DateTime created { get; set; }
        public bool? is_revoked { get; set; }
    }
    public class RegisterParam
    {
        public string username { get; set; }
        public string password { get; set; }
        public string full_name { get; set; }
        public string stock_manage { get; set; }
    }

    public class LoginParam
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class RefreshParam
    {
        public string RefreshToken { get; set; }
    }

    public class LogoutParam
    {
        public string RefreshToken { get; set; }
    }
}
