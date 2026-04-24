namespace BpmDomain.DTO;

public record EngineStartDto(
    string DefinitionId,
    Dictionary<string, object?> Variables,
    string Company,
    string User
);
