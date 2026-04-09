using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Results;
using BpmDomain.Commands;
using BpmDomain.Engine.Interfaces;

namespace BpmApplication.Handlers;

public class GetDefinitionsHandler
    : ICommandHandler<GetDefinitionsCommand, Result<IEnumerable<WorkflowDefinitionDto>>>
{
    private readonly IBpmEngine _engine;

    public GetDefinitionsHandler(IBpmEngine engine)
    {
        _engine = engine;
    }

    public async Task<Result<IEnumerable<WorkflowDefinitionDto>>> HandleAsync(GetDefinitionsCommand query, CancellationToken ct = default)
    {
        try
        {
            var engineRequest = new EngineGetDefinitionsCommand(
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
            return ResultExtensions.Unauthorized<IEnumerable<WorkflowDefinitionDto>>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.NotFound<IEnumerable<WorkflowDefinitionDto>>(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.Internal<IEnumerable<WorkflowDefinitionDto>>(ex.Message);
        }
    }
}
