namespace tasklist.Models
{
    public class CamundaProcessInstance
    {
        public string Id { get; set; }
        public string DefinitionId { get; set; }
        public string BusinessKey { get; set; }
        public string CaseInstanceId { get; set; }
        public bool Ended { get; set; }
        public bool Suspended { get; set; }
        public string TenantId { get; set; }
    }
}
