using BpmDomain.Commands;
using BpmDomain.Results;

namespace BpmDomain.Engine.Interfaces;

public interface IBpmEngine
{
    Task<StartProcessResult> StartProcessAsync(StartProcessCommand cmd, CancellationToken ct = default);
    Task<IEnumerable<GetDefinitionsResult>> GetDefinitionsAsync(GetDefinitionsCommand query, CancellationToken ct = default);
    Task<SearchProcessResult> SearchProcessAsync(SearchProcessCommand query, CancellationToken ct = default);
    Task<GetContextResult> GetContextAsync(GetContextCommand cmd, CancellationToken ct = default);
}