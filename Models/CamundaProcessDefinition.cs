namespace tasklist.Models
{
    public class CamundaProcessDefinition
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int? Version { get; set; }
        public string Resource { get; set; }
        public string Deployment { get; set; }
        public string Diagram { get; set; }
        public bool Suspended { get; set; }
        public string TenantId { get; set; }
        public string VersionTag { get; set; }
    }
}
