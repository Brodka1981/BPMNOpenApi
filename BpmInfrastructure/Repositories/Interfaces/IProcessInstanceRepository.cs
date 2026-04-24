using BpmInfrastructure.Models;

namespace BpmInfrastructure.Repository.Interfaces;

public interface IProcessInstanceRepository
{
    Task<long> SaveAsync(ProcessInstance instance, CancellationToken ct);
}