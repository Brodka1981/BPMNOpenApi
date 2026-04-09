namespace BpmDomain.Models;

public sealed class ActionRequirementDefinition
{
    public string Type { get; }
    public Dictionary<string, object?> Parameters { get; }

    public ActionRequirementDefinition(
        string type,
        Dictionary<string, object?> parameters)
    {
        Type = type;
        Parameters = parameters;
    }
}

