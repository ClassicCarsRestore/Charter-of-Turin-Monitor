using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
	public class ProjectERPNextDTO
	{
		public string ERPNextProjectId { get; set; }
		public string ProjectName { get; set; }
		public string LicencePlate { get; set; }
		public string OwnerEmail { get; set; }
		public string ClientExpectation { get; set; }
		public bool OriginalMaterials { get; set; }
		public bool CarDocuments { get; set; }
	}
}
