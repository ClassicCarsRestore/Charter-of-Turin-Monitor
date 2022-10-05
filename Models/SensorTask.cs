using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
	public class SensorTask
	{
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("activityId")]
        public string ActivityId { get; set; }

        [BsonElement("projectId")]
        public string ProjectId { get; set; }

        [BsonElement("events")]
        public List<string> Events { get; set; }

        [BsonElement("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("endTime")]
        public DateTime EndTime { get; set; }

        public SensorTask(AmazonS3SensorInfo sensorInfo, string projectId)
		{
            ActivityId = null;
            ProjectId = projectId;
            Events = sensorInfo.Event;

            // auxiliary DateTimeOffset variables to convert Unix Epoch into DateTime
            DateTimeOffset startTimeOffset = DateTimeOffset.FromUnixTimeSeconds(sensorInfo.StartTimestamp);
            DateTimeOffset endTimeOffset = DateTimeOffset.FromUnixTimeSeconds(sensorInfo.EndTimestamp);

            StartTime = startTimeOffset.DateTime;
            EndTime = endTimeOffset.DateTime;
        }
    }
}
