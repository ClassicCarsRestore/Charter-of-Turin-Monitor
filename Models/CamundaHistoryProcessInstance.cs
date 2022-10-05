using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class CamundaHistoryProcessInstance
    {
        public String Id { get; set; }
        public String SuperProcessInstanceId { get; set; }
        public String SuperCaseInstanceId { get; set; }
        public String CaseInstanceId { get; set; }
        public String ProcessDefinitionKey { get; set; }
        public String ProcessDefinitionId { get; set; }
        public String BusinessKey { get; set; }
        public String StartTime { get; set; }
        public String EndTime { get; set; }
        public long? DurationInMillis { get; set; }
        public String StartUserId { get; set; }
        public String StartActivityId { get; set; }
        public String DeleteReason { get; set; }
        public String TenantId { get; set; }
    }
}
