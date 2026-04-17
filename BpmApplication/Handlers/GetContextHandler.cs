using BpmApplication.Commands;
using BpmApplication.Common;
using BpmApplication.Handlers.Interfaces;
using BpmDomain.NLog;
using BpmApplication.Results;
using BpmDomain.Engine.Interfaces;
using NLog;
using System.Reflection;

namespace BpmApplication.Handlers;

public class GetContextHandler(IBpmEngine engine)
        : ICommandHandler<GetContextCommand, Result<GetContextResult>>
{
    private readonly IBpmEngine _engine = engine;

    /// <summary>
    /// Handle Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<GetContextResult>> HandleAsync(GetContextCommand cmd, CancellationToken ct = default)
    {
        using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));

        try
        {
            var engineRequest = new BpmDomain.Commands.GetContextCommand(
                cmd.ProcessInstanceId,
                cmd.Company,
                cmd.User,
                cmd.CurrentHttpContext,
                cmd.Abi
            );

            var result = await _engine.GetContextAsync(engineRequest, ct);

            return Result<GetContextResult>.Ok(result.ToGetContextResult());
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Unauthorized<GetContextResult>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.NotFound<GetContextResult>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Internal<GetContextResult>(ex.Message);
        }
    }
}