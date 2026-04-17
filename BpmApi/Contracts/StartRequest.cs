namespace BpmWebApi.Contracts;

public record StartProcessRequest
{
    public string? ProcessType { get; init; }
    public Dictionary<string, object?>? Variables { get; init; }
}