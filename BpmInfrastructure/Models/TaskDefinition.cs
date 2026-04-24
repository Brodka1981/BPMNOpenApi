namespace BpmInfrastructure.Models;

public sealed class TaskDefinition
{
    public string Id { get; }
    public string Label { get; }
    public string Type { get; }
    public string? NextNodeId { get; }
    public IReadOnlyDictionary<string, object?> Parameters { get; }

    public TaskDefinition(
        string id,
        string label,
        string type,
        string? nextNodeId,
        IReadOnlyDictionary<string, object?> parameters)
    {
        Id = id;
        Label = label;
        Type = type;
        NextNodeId = nextNodeId;
        Parameters = parameters;
    }
}