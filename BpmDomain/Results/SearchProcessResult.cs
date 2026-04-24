using System.Text.Json.Serialization;
using BpmInfrastructure.Results;

namespace BpmDomain.Results;

public  class SearchProcessResult
{
    [JsonPropertyName("items")]
    public List<List<GetProcessInstanceSearchSqlResult>> Items { get; set; } = [];
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; }
    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
