namespace tasklist.Models
{
    public class CamundaCalledProcessDefinition
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int? Version { get; set; }
        public string Resource { get; set; }
        public string DeploymentId { get; set; }
        public string Diagram { get; set; }
        public bool Suspended { get; set; }
        public string TenantId { get; set; }
        public string VersionTag { get; set; }
        public int? HistoryTimeToLive { get; set; }
        public bool StartableInTasklist { get; set; }
        public string[] CalledFromActivityIds { get; set; }
        public string CallingProcessDefinitionId { get; set; }
    }
}
