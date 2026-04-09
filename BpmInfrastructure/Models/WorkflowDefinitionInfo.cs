namespace BpmInfrastructure.Models;

// DTO contenente le info da recuperare dal DB per le definizioni BPMN disponibili
public record WorkflowDefinitionInfo
{
    public string ProcessType { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
}
