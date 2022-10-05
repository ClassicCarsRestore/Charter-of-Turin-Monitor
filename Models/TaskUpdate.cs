using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class TaskUpdate
	{
        public string StartDate { get; set; }
        public string CompletionDate { get; set; }
        public string CommentReport { get; set; }
        public string CommentExtra { get; set; }
        public string[] Media { get; set; }
        public string[] ExtraMedia { get; set; }
    }
}
