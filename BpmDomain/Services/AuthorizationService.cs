using BpmDomain.Engine.Interfaces;
using BpmDomain.Models;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Services.Interfaces;

namespace BpmDomain.Services;

public class AuthorizationService(IAuthorizationDataProvider authorizationDataProvider,
    ICompetenceDefinitionRepository competenceDefinitionRepository) : IAuthorizationService
{
    /// <summary>
    /// Definisce se un utente possa o meno avviare un processo
    /// </summary>
    /// <param name="model">Contiene i dati del processo da avviare</param>
    /// <param name="user">Identificativo Utente</param>
    /// <param name="company">Identificativo Banca</param>
    /// <param name="ct">CancellationToken (preimpostato a default)</param>
    /// <returns>booleano che indica se il processo puo' essere avviato</returns>
    public async Task<bool> CanStartAsync(string processDefinitionId, string user, string company, CancellationToken ct = default)
    {
        // Ottiene i gruppi di cui un utente fa parte
        var rsCodes = await authorizationDataProvider.GetUserRsCodesAsync(user, ct);

        if (!int.TryParse(company, out var tenantId))
        {
            return false;
        }

        // Controlla che qualcuno dei gruppi associati all' utente abbia i permessi di Start ($UTI) del processo
        var hasCompetenceDefinitions = await competenceDefinitionRepository.GetCompetenceDefinitionsAsync(
            "$UTI",
            processDefinitionId,
            tenantId,
            rsCodes);

        return hasCompetenceDefinitions.Any();

    }
}
