using System.Text.Json.Serialization;

namespace BpmWebApi.Contracts;

public class ListDefinitionsValueResponse
{
    [JsonPropertyName("items")]
    public List<ListDefinitionsItemResponse> Items { get; set; } = [];

    [JsonPropertyName("page")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Page { get; set; }

    [JsonPropertyName("size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Size { get; set; }

    [JsonPropertyName("totalElements")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TotalElements { get; set; }

    [JsonPropertyName("totalPages")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TotalPages { get; set; }
}

public class ListDefinitionsItemResponse
{
    [JsonPropertyName("definitionType")]
    public string? DefinitionType { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }
}
