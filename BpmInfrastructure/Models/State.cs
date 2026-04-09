using System.Text.Json.Serialization;

namespace BpmInfrastructure.Models
{
    public class State
    {
        [JsonPropertyName("idState")]
        public string? IdState { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
