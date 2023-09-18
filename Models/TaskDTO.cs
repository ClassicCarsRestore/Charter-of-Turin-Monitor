using System;

namespace tasklist.Models
{
    public class TaskDTO
    {
        public string Id { get; set; }

        public string ProcessInstanceId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime CompletionTime { get; set; }
        public string CommentReport { get; set; }
        public string CommentExtra { get; set; }

        public string BoardSectionUrl { get; set; }

        public string BlockChainId { get; set; }

        public TaskDTO(Task task)
        {
            Id = task.ActivityId;
            ProcessInstanceId = task.ProcessInstanceId;
            StartTime = task.StartTime;
            CompletionTime = task.CompletionTime;
            CommentReport = task.CommentReport;
            CommentExtra = task.CommentExtra;
            BoardSectionUrl = task.BoardSectionUrl;
            BlockChainId = task.BlockChainId;
        }
    }
}
