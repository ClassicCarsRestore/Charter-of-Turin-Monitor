using System;
using System.Text.Json.Serialization;

namespace tasklist.Models
{
    public class InventoryProjectDTO
    {
        [JsonPropertyName("charter_id")]
        public string CharterId { get; set; }
        [JsonPropertyName("make")]
        public string Make { get; set; }
        [JsonPropertyName("model")]
        public string Model  { get; set; }
        [JsonPropertyName("year")]
        public int Year { get; set; }
        [JsonPropertyName("licence_plate")]
        public string LicencePlate { get; set; }
        [JsonPropertyName("country")]
        public string Country { get; set; }
        [JsonPropertyName("chassis_num")]
        public string ChassisNo { get; set; }
        [JsonPropertyName("engine_num")]
        public string EngineNo { get; set; }
        [JsonPropertyName("owner")]
        public string OwnerEmail { get; set; }
        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }
        }
}