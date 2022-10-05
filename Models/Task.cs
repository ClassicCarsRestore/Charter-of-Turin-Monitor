using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace tasklist.Models
{
    public class Task
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("activityId")]
        public string ActivityId { get; set; }

        [BsonElement("processInstanceId")]
        public string ProcessInstanceId { get; set; }

        [BsonElement("startTime")]
        public DateTime StartTime { get; set; }

        [BsonElement("completionTime")]
        public DateTime CompletionTime { get; set; }

        [BsonElement("commentReport")]
        public string CommentReport { get; set; }

        [BsonElement("comment")]
        public string CommentExtra { get; set; }

        [BsonElement("boardSectionId")]
        public string BoardSectionId { get; set; }

        [BsonElement("boardSectionUrl")]
        public string BoardSectionUrl { get; set; }

        [BsonElement("pins")]
        public List<string> Pins { get; set; }

        public Task(string activityId, string processInstanceId, string startTime, string completionTime, string commentReport, string commentExtra, string boardSectionId, string boardSectionUrl, List<string> pins)
		{
            ActivityId = activityId;
            ProcessInstanceId = processInstanceId;
            StartTime = DateTime.Parse(startTime);
            CompletionTime = DateTime.Parse(completionTime);
            CommentReport = commentReport;
            CommentExtra = commentExtra;
            BoardSectionId = boardSectionId;
            BoardSectionUrl = boardSectionUrl;
            Pins = pins;

        }
    }
}
