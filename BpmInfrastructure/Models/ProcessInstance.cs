namespace BpmInfrastructure.Models;

public class ProcessInstance
{
    public string? User { get; set; }
    public Dictionary<string, object?>? Variables { get; set; }
    public long ProcessDefinitionId { get; set; }
    public string? Status { get; set; } 
    public DateTime StartedAt { get; set; }
    public string? CurrentNodeId { get; set; }
    public long? CurrentUserTaskId { get; set; }
    public DateTime CompletedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public long TenantId { get; set; }
}