using System.Net.Http.Json;
using BpmInfrastructure.Services.Interfaces;

namespace BpmInfrastructure.Services;

public class AuthorizationDataProvider(HttpClient httpClient) : IAuthorizationDataProvider
{
    /// <summary>
    /// Restituisce la lista dei Gruppi associati ad un determinato utente
    /// </summary>
    /// <param name="username">Identificativo utente</param>
    /// <param name="ct">Cancellation Token (preimpostato a default)</param>
    /// <returns>Lista gruppi di cui fa parte l'utente</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<IReadOnlyCollection<string>> GetUserRsCodesAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        var encodedUsername = Uri.EscapeDataString(username.Trim());
        List<UserMembershipDto> memberships;

        try
        {
            memberships = await httpClient.GetFromJsonAsync<List<UserMembershipDto>>(
                              $"v1/core/users/{encodedUsername}/memberships",
                              ct)
                          ?? [];
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            memberships = [];
        }

        return memberships
            .Where(m => !string.IsNullOrWhiteSpace(m.RsCode))
            .Select(m => m.RsCode!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private sealed class UserMembershipDto
    {
        public string? RsCode { get; init; }
    }
}
