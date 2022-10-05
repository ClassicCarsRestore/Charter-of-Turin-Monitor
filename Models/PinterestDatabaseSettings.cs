using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class PinterestDatabaseSettings : IPinterestDatabaseSettings
    {
        public string PinterestCollectionName { get; set; }
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

    public interface IPinterestDatabaseSettings
    {
        string PinterestCollectionName { get; set; }
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
