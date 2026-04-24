using BpmApplication.Queries.Interfaces;

namespace BpmApplication.Queries;

public record GetVariablesQuery : IQuery<Dictionary<string, object?>>
{
    public Guid ProcessId { get; init; }
}
