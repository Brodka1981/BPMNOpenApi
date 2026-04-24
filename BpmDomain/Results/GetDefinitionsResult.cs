using System.Text.Json.Serialization;

namespace BpmDomain.Results;

public class GetDefinitionsResult
{
    [JsonPropertyName("processType")]
    public string? ProcessType { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("category")]
    public string? Category { get; set; }
}
