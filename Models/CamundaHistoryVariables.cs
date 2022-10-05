using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
	public class CamundaHistoryVariables
	{
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public dynamic Value { get; set; }
        public object ValueInfo { get; set; }
        public string ProcessDefinitionKey { get; set; }
        public string ProcessDefinitionId { get; set; }
        public string ProcessInstanceId { get; set; }
        public string ExecutionId { get; set; }
        public string ActivityInstanceId { get; set; }
        public string CaseDefinitionKey { get; set; }
        public string CaseInstanceId { get; set; }
        public string CaseExecutionId { get; set; }
        public string TaskId { get; set; }
        public string TenantId { get; set; }
        public string ErrorMessage { get; set; }
        public string State { get; set; }
        public string CreateTime { get; set; }
        public string RemovalTime { get; set; }
        public string RootProcessInstanceId { get; set; }
    }
}
