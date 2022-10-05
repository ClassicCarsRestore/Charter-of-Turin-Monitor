using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
	public class ActivityMiningRow
	{
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicencePlate { get; set; }
        public string ActivityName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompletionTime { get; set; }

        public ActivityMiningRow(Project project, CamundaHistoryTask camundaTask, Task task)
        {
            Make = project.Make;
            Model = project.Model;
            Year = project.Year;
            LicencePlate = project.LicencePlate;
            ActivityName = camundaTask.ActivityName;

            if (task != null)
            {
                StartTime = task.StartTime;
                CompletionTime = task.CompletionTime;
            }
            else
            {
                StartTime = DateTime.Parse(camundaTask.StartTime);
                CompletionTime = DateTime.Parse(camundaTask.EndTime);
            }
        }
    }
}
