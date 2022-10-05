using System;

namespace tasklist.Models
{
	public class FullHistoryTaskDTO
	{
        public string ActivityName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompletionTime { get; set; }
        public DateTime SubmitionTime { get; set; }
        public string BoardSectionUrl { get; set; }

        public FullHistoryTaskDTO(CamundaHistoryTask camundaTask, Task task)
		{
            ActivityName = camundaTask.ActivityName;

            if (task != null)
			{
                StartTime = task.StartTime;
                CompletionTime = task.CompletionTime;
                BoardSectionUrl = task.BoardSectionUrl;
            } else
			{
                StartTime = DateTime.Parse(camundaTask.StartTime);
                CompletionTime = DateTime.Parse(camundaTask.EndTime);
            }
            SubmitionTime = DateTime.Parse(camundaTask.StartTime);
        }
    }
}
