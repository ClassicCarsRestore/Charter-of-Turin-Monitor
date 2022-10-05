using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class PathProcess
    {
        public string Name { get; set; }
        public string DefinitionId { get; set; }
        public string InstanceId { get; set; }
        public PathProcess(CamundaProcessInstance instance, string name)
        {
            Name = name;
            DefinitionId = instance.DefinitionId;
            InstanceId = instance.Id;
        }
        public PathProcess(CamundaHistoryProcessInstance instance, string name)
        {
            Name = name;
            DefinitionId = instance.ProcessDefinitionId;
            InstanceId = instance.Id;
        }
        public PathProcess(CamundaProcessDefinition definition)
        {
            Name = definition.Name;
            DefinitionId = definition.Id;
            InstanceId = null;
        }
        public PathProcess(CamundaCalledProcessDefinition definition)
        {
            Name = definition.Name;
            DefinitionId = definition.Id;
            InstanceId = null;
        }
    }
}
