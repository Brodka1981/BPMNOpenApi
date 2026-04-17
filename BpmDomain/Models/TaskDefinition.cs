namespace BpmDomain.Models;
public sealed class TaskDefinition(
    string id,
    string label,
    string type,
    string? nextNodeId,
    IReadOnlyDictionary<string, object?> parameters)
{
    public string Id { get; } = id;
    public string Label { get; } = label;
    public string Type { get; } = type;
    public string? NextNodeId { get; } = nextNodeId;
    public IReadOnlyDictionary<string, object?> Parameters { get; } = parameters;
}