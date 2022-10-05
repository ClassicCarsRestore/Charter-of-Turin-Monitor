namespace tasklist.Models
{
	public class LoginCredentialsDatabaseSettings : ILoginCredentialsDatabaseSettings
    {
        public string LoginCredentialsCollectionName { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string ConnectionString
        {
            get
            {
                return $@"mongodb://{Host}:{Port}";
            }
        }
        public string DatabaseName { get; set; }
    }

    public interface ILoginCredentialsDatabaseSettings
    {
        string LoginCredentialsCollectionName { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string ConnectionString
        {
            get
            {
                return $@"mongodb://{Host}:{Port}";
            }
        }
        public string DatabaseName { get; set; }
    }
}
