namespace NullMarketManager.Models
{
    public class AuthInfo
    {
        public AuthInfo()
        {
            access_token = "";
            expires_in = -1;
            refresh_token = "";
            expiry_time = -1;
        }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public long expiry_time { get; set; }
    }
}
