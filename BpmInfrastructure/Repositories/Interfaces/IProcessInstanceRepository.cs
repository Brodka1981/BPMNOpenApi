using BpmInfrastructure.Models;

namespace BpmInfrastructure.Repository.Interfaces;

public interface IProcessInstanceRepository
{
    Task<long> GenerateProcessIdAsync(CancellationToken ct);
    Task SaveAsync(ProcessInstance instance, CancellationToken ct);
}