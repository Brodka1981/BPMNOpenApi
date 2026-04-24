namespace BpmWebApi.Contracts;

public record ActionRequest
{
    public Guid ProcessId { get; init; }
    public string ActionName { get; init; } = "";
    public Dictionary<string, object?>? Variables { get; init; }
}

