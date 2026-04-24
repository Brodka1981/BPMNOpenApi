using System.Text.Json.Serialization;

namespace BpmInfrastructure.Models
{
    public class Form
    {
        [JsonPropertyName("sections")]
        public List<Models.Section>? Sections { get; set; }
    }
}
