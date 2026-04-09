namespace BpmApplication.DTO;

public record WorkflowDefinitionDto
{
    //public string DefinitionId { get; init; } = "";
    public string ProcessType { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
}
