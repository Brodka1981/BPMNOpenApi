using BpmDomain.Commands;
using BpmDomain.Results;

namespace BpmDomain.Engine.Interfaces;

public interface IBpmEngine
{
    Task<EngineStartResult> StartProcessAsync(EngineStartCommand cmd, CancellationToken ct = default);
    Task<IEnumerable<EngineDefinitionResult>> GetDefinitionsAsync(EngineGetDefinitionsCommand query, CancellationToken ct = default);
    Task<GetContextResult> GetContextAsync(GetContextCommand cmd, CancellationToken ct = default);
}