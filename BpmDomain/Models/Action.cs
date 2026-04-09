using System.Text.Json.Serialization;

namespace BpmDomain.Models
{
    public class Action
    {
        [JsonPropertyName("idAction")]
        public string? IdAction { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}