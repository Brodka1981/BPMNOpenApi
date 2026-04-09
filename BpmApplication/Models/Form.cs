using System.Text.Json.Serialization;

namespace BpmApplication.Models
{
    public class Form
    {
        [JsonPropertyName("sections")]
        public List<Models.Section>? Sections { get; set; }
    }
}
