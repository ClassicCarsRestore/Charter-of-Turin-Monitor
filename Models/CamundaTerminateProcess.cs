using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
	public class CamundaTerminateProcess
	{
		public string DeleteReason { get; set; }
		public string[] ProcessInstanceIds { get; set; }
		public bool SkipCustomListeners { get; set; }
		public bool SkipSubprocesses { get; set; }

	}
}
