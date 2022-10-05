using System;

namespace tasklist.Models
{
	public class Media
	{
		public string media_id { get; set; }
		public string media_type { get; set; }
		public string upload_url { get; set; }
		public UploadParameters upload_parameters { get; set; }
	}
}
