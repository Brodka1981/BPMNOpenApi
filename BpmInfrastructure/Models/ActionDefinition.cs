namespace BpmInfrastructure.Models;

public sealed class ActionDefinition
{
    public string Id { get; }
    public string Label { get; }
    public string TargetNodeId { get; }
    public IReadOnlyList<ActionRequirementDefinition> Requirements { get; }

    public ActionDefinition(
        string id,
        string label,
        string targetNodeId,
        IReadOnlyList<ActionRequirementDefinition> requirements)
    {
        Id = id;
        Label = label;
        TargetNodeId = targetNodeId;
        Requirements = requirements;
    }
}