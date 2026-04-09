namespace BpmDomain.Commands;

public record GetContextCommand(
    long ProcessInstanceId,
    string? Company,
    string? User
);