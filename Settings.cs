namespace tasklist
{
    public static class Settings
    {
        public const string Secret = "YourSecret";
        public const string Camera_Hub_Secret = "CameraHubSecret";
        public const string Email_Address = "EmailAddress";
        public const string Email_Password = "EmailPassword";
        public const string Pinterest_ID = "PinterestAppID";
        public const string Pinterest_Secret = "PinterestAppSecret";
        public const string Pinterest_Account = "PinterestAccountUsername";
		
        public const string Pinterest_Redirect_URI = "pinterest";
        public const string Pinterest_Scope = "user_accounts:read,boards:read,boards:read_secret,pins:read,pins:read_secret,ads:read,boards:write,boards:write_secret,pins:write,pins:write_secret";
        public const string Pinterest_API_URL = "https://api.pinterest.com/v5/";
        public const string Pinterest_Photos_URL = "https://www.pinterest.com/";
    }
}
