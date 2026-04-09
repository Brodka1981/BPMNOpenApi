using BpmDomain.Models;

namespace BpmDomain.Engine.Interfaces;

public interface IAuthorizationService
{
    Task<bool> CanStartAsync(string processDefinitionId, string user, string company, CancellationToken ct = default);
}
