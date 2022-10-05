using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class CamundaHistoryTask
    {
        public string Id { get; set; }
        public string ParentActivityInstanceId { get; set; }
        public string ActivityId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string ProcessDefinitionKey { get; set; }
        public string ProcessDefinitionId { get; set; }
        public string ProcessInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public string TaskId { get; set; }
        public string CalledProcessInstanceId { get; set; }
        public string CalledCaseInstanceId { get; set; }
        public string Assignee { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public long? DurationInMillis { get; set; }
        public bool Canceled { get; set; }
        public bool CompleteScope { get; set; }
        public string TenantId { get; set; }
        public string RemovalTime { get; set; }
        public string RootProcessInstanceId { get; set; }
    }
}
