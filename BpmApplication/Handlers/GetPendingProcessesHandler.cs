using BpmApplication.DTO;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Queries;

namespace BpmApplication.Handlers;

public class GetPendingProcessesHandler : IQueryHandler<GetPendingProcessesQuery, IEnumerable<PendingProcessDto>>
{
    public Task<IEnumerable<PendingProcessDto>> HandleAsync(GetPendingProcessesQuery query, CancellationToken ct = default)
    {
        var list = new List<PendingProcessDto>();

        return Task.FromResult<IEnumerable<PendingProcessDto>>(list);
    }
}
