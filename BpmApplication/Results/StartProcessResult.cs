using System.Text.Json.Serialization;

namespace BpmApplication.Results;

public class StartProcessResult
{
    [JsonPropertyName("processId")]
    public long ProcessId { get; set; }
}