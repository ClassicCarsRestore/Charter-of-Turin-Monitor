using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class ActivityMap
	{

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("activityId")]
        public string ActivityId { get; set; }

        [BsonElement("diagramId")]
        public string DiagramId { get; set; }
        
        [BsonElement("diagramOrder")]
        public int DiagramOrder { get; set; }

        [BsonElement("sensorsRelated")]
        public List<string> SensorsRelated { get; set; }
    }
}
