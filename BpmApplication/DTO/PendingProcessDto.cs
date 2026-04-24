namespace BpmApplication.DTO;

public record PendingProcessDto
{
    public Guid ProcessId { get; init; }
    public string State { get; init; } = "";
    public string User { get; init; } = "";
}
