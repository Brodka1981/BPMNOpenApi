namespace BpmDomain.Models;
public class FlowElement
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ElementType { get; set; } = ""; // userTask, startEvent, gateway, etc.

    public string? TaskType { get; set; } // per serviceTask
    public Dictionary<string, string> Extensions { get; set; } = new();
}
