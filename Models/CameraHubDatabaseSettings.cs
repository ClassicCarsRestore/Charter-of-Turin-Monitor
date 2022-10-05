namespace tasklist.Models
{
	public class CameraHubDatabaseSettings : ICameraHubDatabaseSettings
    {
        public string CameraHubCollectionName { get; set; }
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

    public interface ICameraHubDatabaseSettings
    {
        string CameraHubCollectionName { get; set; }
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
