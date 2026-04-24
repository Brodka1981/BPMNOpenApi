using BpmApplication.Queries.Interfaces;

namespace BpmApplication.Queries;

public record GetBpmnQuery : IQuery<string>
{
    public string DefinitionId { get; init; } = "";
}
