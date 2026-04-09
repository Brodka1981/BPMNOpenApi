namespace BpmDomain.Commands;

public record EngineStartCommand(
    string DefinitionId,
    Dictionary<string, object?> Variables,
    string Company,
    string User
);