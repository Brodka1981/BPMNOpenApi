namespace BpmDomain.Commands;

public record EngineGetDefinitionsCommand(
    string User,
    string Company,
    string? Category
);
