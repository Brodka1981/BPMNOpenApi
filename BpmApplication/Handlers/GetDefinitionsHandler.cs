using BpmApplication.DTO;
using BpmApplication.Handlers.Interfaces;
using BpmDomain.NLog;
using BpmApplication.Results;
using BpmDomain.Engine.Interfaces;
using NLog;
using System.Reflection;

namespace BpmApplication.Handlers;

public class GetDefinitionsHandler(IBpmEngine engine)
        : ICommandHandler<Commands.GetDefinitionsCommand, Result<IEnumerable<WorkflowDefinitionDto>>>
{
    private readonly IBpmEngine _engine = engine;

    /// <summary>
    /// Handle Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<IEnumerable<WorkflowDefinitionDto>>> HandleAsync(Commands.GetDefinitionsCommand query, CancellationToken ct = default)
    {
        using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));

        try
        {
            var engineRequest = new BpmDomain.Commands.GetDefinitionsCommand(
                query.User,
                query.Company,
                query.Category
            );

            var definitions = await _engine.GetDefinitionsAsync(engineRequest, ct);

            var mapped = definitions.Select(x => new WorkflowDefinitionDto
            {
                ProcessType = x.ProcessType,
                Name = x.Name,
                Category = x.Category
            });

            return Result<IEnumerable<WorkflowDefinitionDto>>.Ok(mapped);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Unauthorized<IEnumerable<WorkflowDefinitionDto>>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.NotFound<IEnumerable<WorkflowDefinitionDto>>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Internal<IEnumerable<WorkflowDefinitionDto>>(ex.Message);
        }
    }
}