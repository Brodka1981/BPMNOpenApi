using System.Text.Json.Serialization;

namespace BpmDomain.Results;

public class StartProcessResult()
{
    [JsonPropertyName("processId")]
    public long ProcessId { get; set; }
}