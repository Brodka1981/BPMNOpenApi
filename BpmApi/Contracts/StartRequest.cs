namespace BpmWebApi.Contracts;

public record StartProcessRequest
{
    public string? DefinitionId { get; init; }
    public Dictionary<string, object?>? Variables { get; init; }
}