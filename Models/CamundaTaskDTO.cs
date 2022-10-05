using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class CamundaTaskDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Assignee { get; set; }
        public string Created { get; set; }
        public string ExecutionId { get; set; }
        public string ParentTaskId { get; set; }
        public string ProcessDefinitionId { get; set; }
        public string ProcessInstanceId { get; set; }
        public string TaskDefinitionKey { get; set; }
        public bool Suspended { get; set; }

        public CamundaTaskDTO(CamundaTask task)
        {
            Id = task.Id;
            Name = task.Name;
            Assignee = task.Assignee;
            Created = task.Created;
            ExecutionId = task.ExecutionId;
            ParentTaskId = task.ParentTaskId;
            ProcessDefinitionId = task.ProcessDefinitionId;
            ProcessInstanceId = task.ProcessInstanceId;
            TaskDefinitionKey = task.TaskDefinitionKey;
            Suspended = task.Suspended;
        }
    }
}
