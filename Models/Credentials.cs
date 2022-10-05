using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace tasklist.Models
{
	public class Credentials
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("username")]
		public string Username { get; set; }

		[BsonElement("access_key_id")]
		public string Access_key_id { get; set; }

		[BsonElement("secret_access_key")]
		public string Secret_access_key { get; set; }
	}
}
