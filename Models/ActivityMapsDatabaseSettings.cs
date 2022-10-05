namespace tasklist.Models
{
	public class ActivityMapsDatabaseSettings : IActivityMapsDatabaseSettings
    {
        public string ActivityMapsCollectionName { get; set; }
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

    public interface IActivityMapsDatabaseSettings
    {
        string ActivityMapsCollectionName { get; set; }
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
