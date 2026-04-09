using BpmApplication.Commands;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Results;
using BpmDomain.Engine.Interfaces;

namespace BpmApplication.Handlers;

public class StartProcessHandler
    : ICommandHandler<StartProcessCommand, Result<StartProcessResult>>
{
    private readonly IBpmEngine _engine;

    public StartProcessHandler(IBpmEngine engine)
    {
        _engine = engine;
    }

    public async Task<Result<StartProcessResult>> HandleAsync(StartProcessCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var engineRequest = new BpmDomain.Commands.EngineStartCommand(
                cmd.DefinitionId,
                cmd.Variables,
                cmd.Company,
                cmd.User
            );

            var result = await _engine.StartProcessAsync(engineRequest, ct);

            return Result<StartProcessResult>.Ok(new StartProcessResult(result.ProcessId));
        }
        catch (UnauthorizedAccessException ex)
        {
            return ResultExtensions.Unauthorized<StartProcessResult>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.NotFound<StartProcessResult>(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.Internal<StartProcessResult>(ex.Message);
        }
    }
}
