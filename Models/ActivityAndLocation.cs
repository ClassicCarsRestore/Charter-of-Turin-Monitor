using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace tasklist.Models
{
    public class ActivityAndLocation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("activityName")]
        public string ActivityName { get; set; }

        [BsonElement("startDate")]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        public DateTime? EndDate { get; set; }

        [BsonElement("locationId")]
        public string LocationId { get; set; }

        public ActivityAndLocation(string activityName, DateTime startDate, DateTime endDate, string locationId)
        {
            ActivityName = activityName;
            StartDate = startDate;
            EndDate = endDate;
            LocationId = locationId;
        }
    }
}
