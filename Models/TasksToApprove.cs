namespace tasklist.Models
{
	public class TasksToApprove
	{
		public TaskToApprove[] Tasks { get; set; }
		public string[][] Variables { get; set; }
		public string[] StartEventTriggers { get; set; }
		public string ProcessInstanceId { get; set; }
	}
}
