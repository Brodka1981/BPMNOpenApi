using BpmApplication.DTO;
using BpmApplication.Queries.Interfaces;

namespace BpmApplication.Queries;

public record GetPendingProcessesQuery : IQuery<IEnumerable<PendingProcessDto>>
{
    public string User { get; init; } = "";
}

