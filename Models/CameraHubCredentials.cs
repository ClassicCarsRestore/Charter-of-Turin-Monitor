using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace tasklist.Models
{
	public class CameraHubCredentials
    {
        public string ProjectName { get; set; }

        public string Password { get; set; }
    }
}
