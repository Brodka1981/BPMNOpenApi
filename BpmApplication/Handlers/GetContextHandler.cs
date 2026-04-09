using BpmApplication.Commands;
using BpmApplication.Common;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Results;
using BpmDomain.Engine.Interfaces;

namespace BpmApplication.Handlers;

public class GetContextHandler
    : ICommandHandler<GetContextCommand, Result<GetContextResult>>
{
    private readonly IBpmEngine _engine;

    public GetContextHandler(IBpmEngine engine)
    {
        _engine = engine;
    }

    /// <summary>
    /// Handle Async
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result<GetContextResult>> HandleAsync(GetContextCommand cmd, CancellationToken ct = default)
    {
        try
        {
            var engineRequest = new BpmDomain.Commands.GetContextCommand(
                cmd.ProcessInstanceId,
                cmd.Company,
                cmd.User
            );

            var result = await _engine.GetContextAsync(engineRequest, ct);

            return Result<GetContextResult>.Ok(result.ToGetContextResult());
        }
        catch (UnauthorizedAccessException ex)
        {
            return ResultExtensions.Unauthorized<GetContextResult>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return ResultExtensions.NotFound<GetContextResult>(ex.Message);
        }
        catch (Exception ex)
        {
            return ResultExtensions.Internal<GetContextResult>(ex.Message);
        }
    }
}