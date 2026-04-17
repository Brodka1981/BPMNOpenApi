namespace BpmDomain.Commands;

public record GetContextCommand(
    long ProcessInstanceId,
    string? Company,
    string? User,
    string? CurrentHttpContext,
    string? Abi
);