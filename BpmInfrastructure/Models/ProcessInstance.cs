namespace BpmInfrastructure.Models;

public class ProcessInstance
{
    public long Id { get; set; }

    public string DefinitionId { get; set; } = "";
    public string Company { get; set; } = "";
    public string User { get; set; } = "";

    public string CurrentNode { get; set; } = "";

    public Dictionary<string, object?> Variables { get; set; } = new();
}
