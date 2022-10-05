using System;

namespace tasklist.Models
{
	public class ProjectFormDTO
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicencePlate { get; set; }
        public string Country { get; set; }
        public string ChassisNo { get; set; }
        public string EngineNo { get; set; }
        public string OwnerEmail { get; set; }
        public string ClientExpectation { get; set; }
        public string Photo { get; set; }
        public bool OriginalMaterials { get; set; }
        public bool CarDocuments { get; set; }
        public DateTime StartDate { get; set; }
    }
}
