namespace BpmInfrastructure.Services.Interfaces;

public interface IAuthorizationDataProvider
{
    Task<IReadOnlyCollection<string>> GetUserRsCodesAsync(string username, CancellationToken ct = default);
}
