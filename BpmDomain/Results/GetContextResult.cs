using System.Text.Json.Nodes;

namespace BpmDomain.Results;

public class GetContextResult
{
    public long? ProcessId { get; set; }
    public string? ProcessType { get; set; }
    public string? Name { get; set; }
    public string? ContextMode { get; set; }
    public Models.State? State { get; set; }
    public List<Models.Action>? Actions { get; set; }
    public List<JsonObject>? Variables { get; set; }
    public Models.Form? Form { get; set; }
}