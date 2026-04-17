using System.Text.Json.Serialization;

namespace BpmApplication.DTO;

public record WorkflowDefinitionDto
{
    [JsonPropertyName("processType")]
    public string? ProcessType { get; init; }
    [JsonPropertyName("name")]
    public string? Name { get; init; }
    [JsonPropertyName("category")]
    public string? Category { get; init; }
}