namespace BpmWebApi.Contracts;

public class PendingProcessFilter
{
    public string? Category { get; set; }
    public string? State { get; set; }
    public string? AssignedTo { get; set; }
}

public class PendingProcessItem
{
    public Guid ProcessId { get; set; }
    public string WorkflowName { get; set; } = "";
    public string CurrentNodeId { get; set; } = "";
    public string CurrentNodeName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
