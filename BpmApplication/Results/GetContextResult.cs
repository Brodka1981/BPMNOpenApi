using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BpmApplication.Results
{
    public class GetContextResult
    {
        [JsonPropertyName("processId")]
        public long? ProcessId { get; set; }
        [JsonPropertyName("processType")]
        public string? ProcessType { get; set; }
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("contextMode")]
        public string? ContextMode { get; set; }
        [JsonPropertyName("state")]
        public Models.State? State { get; set; }
        [JsonPropertyName("actions")]
        public List<Models.Action>? Actions { get; set; }
        [JsonPropertyName("variables")]
        public List<JsonObject>? Variables { get; set; }
        [JsonPropertyName("form")]
        public Models.Form? Form { get; set; }
    }
}
