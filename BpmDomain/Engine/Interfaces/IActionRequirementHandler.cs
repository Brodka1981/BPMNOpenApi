using BpmDomain.Models;

namespace BpmDomain.Engine.Interfaces;


public interface IActionRequirementHandler
{
    string RequirementType { get; }

    Task<bool> EvaluateAsync(
        ActionRequirementDefinition requirement,
        Dictionary<string, object?> variables);
}
