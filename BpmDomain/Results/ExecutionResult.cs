namespace BpmDomain.Results;
public sealed class ExecutionResult
{
    public string CurrentNodeId { get; set; } = "";
    public string CurrentNodeName { get; set; } = "";
    public Dictionary<string, object?> Variables { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
