using Newtonsoft.Json;

namespace tasklist.Models
{
	public class UploadParameters
	{
		[JsonProperty(PropertyName = "x-amz-date")]
		public string x_amz_date { get; set; }
		[JsonProperty(PropertyName = "x-amz-signature")]
		public string x_amz_signature { get; set; }
		public string key { get; set; }
		public string policy { get; set; }
		[JsonProperty(PropertyName = "x-amz-credential")]
		public string x_amz_credential { get; set; }
		[JsonProperty(PropertyName = "x-amz-security-token")]
		public string x_amz_security_token { get; set; }
		[JsonProperty(PropertyName = "x-amz-algorithm")]
		public string x_amz_algorithm { get; set; }
		[JsonProperty(PropertyName = "Content-Type")]
		public string Content_Type { get; set; }
		public string file { get; set; }
	}
}
