using BpmApplication.Queries.Interfaces;

namespace BpmApplication.Queries;

public record ValidateActionQuery : IQuery<bool>
{
    public Guid ProcessId { get; init; }
    public string ActionName { get; init; } = "";
}
