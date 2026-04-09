

using BpmApplication.DTO;

public record WorkflowContextDto
{
    public long ProcessId { get; init; }
    public string State { get; init; } = "";
    public IEnumerable<AvailableAction> AvailableActions { get; init; } = [];
}
