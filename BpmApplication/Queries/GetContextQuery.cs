using BpmApplication.DTO;
using BpmApplication.Queries.Interfaces;

namespace BpmApplication.Queries;

public record GetContextQuery : IQuery<WorkflowContextDto>
{
    public long ProcessId { get; init; }
}
