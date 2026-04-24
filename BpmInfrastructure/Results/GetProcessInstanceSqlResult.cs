namespace BpmInfrastructure.Results
{
    public class GetProcessInstanceSqlResult
    {
        public long ProcessInstanceId { get; set; }
        public long ProcessDefinitionId { get; set; }
        public string? Status { get; set; }
        public DateTime StartedAt { get; set; }
        public string? CurrentNodeId { get; set; }
        public string? CurrentUserTaskId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public long TenantId { get; set; }
    }
}