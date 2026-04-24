namespace BpmDomain.Commands;

public record GetDefinitionsCommand(
    string User,
    string Company,
    string? Category
);