using BpmApplication.Commands;
using BpmApplication.Handlers.Interfaces;
using BpmDomain.NLog;
using BpmApplication.Results;
using BpmDomain.Engine.Interfaces;
using NLog;
using System.Reflection;

namespace BpmApplication.Handlers;

public class StartProcessHandler(IBpmEngine engine)
        : ICommandHandler<StartProcessCommand, Result<StartProcessResult>>
{
    private readonly IBpmEngine _engine = engine;

    public async Task<Result<StartProcessResult>> HandleAsync(StartProcessCommand cmd, CancellationToken ct = default)
    {
        using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));

        try
        {
            var engineRequest = new BpmDomain.Commands.StartProcessCommand(
                cmd.ProcessType,
                cmd.Variables,
                cmd.Company,
                cmd.User
            );

            var result = await _engine.StartProcessAsync(engineRequest, ct);

            return Result<StartProcessResult>.Ok(new StartProcessResult() { ProcessId = result.ProcessId });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Unauthorized<StartProcessResult>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.NotFound<StartProcessResult>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Internal<StartProcessResult>(ex.Message);
        }
    }
}