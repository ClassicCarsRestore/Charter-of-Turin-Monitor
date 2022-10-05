using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Class used to define the request bodies of tasks/events related to task approvals. This includes 
/// a regular 'UserTask' approval, the signal that has to be sent to the 'ReceiveMessageTask' to complete
/// that task, and the trigger of the 'Signal Start Event'.
/// </summary>
namespace tasklist.Models
{
	public class CamundaTaskApprove
	{
		public Dictionary<string, PairKeyValue> Variables { get; set; }

		public bool WithVariablesInReturn { get; set; }

	}

	public class CamundaSignalReceiveTask
	{
		public string MessageName { get; set; }
		public string ProcessInstanceId { get; set; }
		public Dictionary<string, PairKeyValueType> ProcessVariables { get; set; }
	}

	public class CamundaTriggerSignalStartEvent
	{
		public string Name { get; set; }
		public string ExecutionId { get; set; }
	}

	public class PairKeyValue
	{
		public string Value { get; set; }

	}

	public class PairKeyValueType
	{
		public dynamic Value { get; set; }

		public string Type { get; set; }
	}
}
