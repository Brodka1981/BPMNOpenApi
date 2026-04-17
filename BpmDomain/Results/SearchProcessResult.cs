using System.Text.Json.Serialization;

namespace BpmDomain.Results;

public  class SearchProcessResult
{
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; }
    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
