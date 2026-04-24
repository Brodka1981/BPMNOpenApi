namespace BpmDomain.Engine.Interfaces;

public interface IAuthorizationService
{
    Task<bool> CanStartAsync(string processDefinitionId, string user, string company, CancellationToken ct = default);
    Task<bool> CanGetDefinitionsAsync(string processDefinitionId, string user, string company, CancellationToken ct = default);
    Task<bool> CanGetContextAsync(string processDefinitionId, string user, string company, CancellationToken ct = default);
}