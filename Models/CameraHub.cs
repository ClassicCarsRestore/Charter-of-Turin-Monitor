using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace tasklist.Models
{
	public class CameraHub
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("project_id")]
		public string ProjectId { get; set; }

		[BsonElement("projectName")]
		public string ProjectName { get; set; }

		[BsonElement("password")]
		public string Password { get; set; }

		[BsonElement("start_time")]
		public string StartTime { get; set; }

		[BsonElement("end_time")]
		public string EndTime { get; set; }

		[BsonElement("boardSectionId")]
		public string BoardSectionId { get; set; }
	}
}
