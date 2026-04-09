using System.Text.Json.Serialization;

namespace BpmDomain.Models
{
    public class Form
    {
        [JsonPropertyName("sections")]
        public List<Models.Section>? Sections { get; set; }
    }
}
