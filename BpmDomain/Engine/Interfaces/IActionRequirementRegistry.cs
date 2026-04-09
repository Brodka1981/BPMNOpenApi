namespace BpmDomain.Engine.Interfaces;

public interface IActionRequirementRegistry
{
    IActionRequirementHandler Resolve(string requirementType);
}
