namespace BpmWebApi.Contracts;

public class WorkflowContextResponse
{
    public Guid ProcessId { get; set; }
    public string State { get; set; } = "";
    public IEnumerable<Action> AvailableActions { get; set; } = [];
    public Dictionary<string, object?> Variables { get; set; } = new();
    public string? Form { get; set; }
    // opzionali
    public string CurrentNodeId { get; set; } = "";
    public string CurrentNodeName { get; set; } = "";
}
