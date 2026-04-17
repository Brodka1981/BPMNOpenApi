using BpmDomain.Models;

namespace BpmDomain.Engine.Interfaces;

public interface IBpmnParserService
{
    WorkflowDefinition? Parse(string? xml);
}