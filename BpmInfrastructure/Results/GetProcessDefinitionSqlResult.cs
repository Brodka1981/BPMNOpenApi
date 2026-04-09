namespace BpmInfrastructure.Results
{
    public class GetProcessDefinitionSqlResult
    {
        public long ProcessDefinitionId { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Name { get; set; }
        public string? BpmnXml { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public long TenantId { get; set; } 
    }
}