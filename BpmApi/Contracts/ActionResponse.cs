namespace BpmWebApi.Contracts;

public record ActionResponse
{
    public Guid ProcessId { get; init; }
    public string NewNodeId { get; set; } = "";
    public string NewNodeName { get; set; } = "";
    public bool Success { get; set; }  
    public string Message { get; set; } = "";  
}
