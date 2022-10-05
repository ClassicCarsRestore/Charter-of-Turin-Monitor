namespace tasklist.Models
{
	public class TaskToApprove
	{
		public string Id { get; set; }
		public string StartTime { get; set; }
		public string CompletionTime { get; set; }
		public string Message { get; set; }
		public string CommentReport { get; set; }
		public string CommentExtra { get; set; }
		public string[] Media { get; set; }
		public string[] ExtraMedia { get; set; }
	}
}
