using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace tasklist.Models
{
    public class ActivityAndLocationHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("caseInstanceId")]
        public string CaseInstanceId { get; set; }

        [BsonElement("history")]
        public List<ActivityAndLocation> History { get; set; }

        public ActivityAndLocationHistory(string caseInstanceId, List<ActivityAndLocation> history)
        {
            CaseInstanceId = caseInstanceId;
            History = history;
        }
    }
}
