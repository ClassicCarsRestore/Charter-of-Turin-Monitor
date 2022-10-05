using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
	public class AmazonS3SensorInfo
	{
		public string DeviceId { get; set; }
		public string Car { get; set; }
		public int Location { get; set; }
		public List<string> Event { get; set; }
		public int StartTimestamp { get; set; }
		public int EndTimestamp { get; set; }
	}
}
