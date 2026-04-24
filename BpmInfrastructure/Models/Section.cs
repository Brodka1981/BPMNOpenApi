using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace BpmInfrastructure.Models
{
    public class Section
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("fields")]
        public List<JsonObject>? Fields { get; set; }
    }
}
