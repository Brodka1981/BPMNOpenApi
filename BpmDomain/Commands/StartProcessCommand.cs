namespace BpmDomain.Commands;

public record StartProcessCommand(
    string? ProcessType,
    Dictionary<string, object?>? Variables,
    string? Company,
    string? User
);