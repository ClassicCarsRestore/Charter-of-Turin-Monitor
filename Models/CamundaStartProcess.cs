using System.Collections.Generic;

namespace tasklist.Models
{
    public class CamundaStartProcess
    {
        public Dictionary<string, PairKeyValueType> Variables { get; set; }

        public string CaseInstanceId { get; set; }

    }
}
