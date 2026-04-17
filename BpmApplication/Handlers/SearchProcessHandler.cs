using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Results;
using BpmDomain.Engine.Interfaces;
using BpmDomain.NLog;
using NLog;
using System.Reflection;

namespace BpmApplication.Handlers;

public class SearchProcessHandler(IBpmEngine engine)
    : ICommandHandler<SearchProcessCommand, Result<IEnumerable<SearchProcessDto>>>
{
    private readonly IBpmEngine _engine = engine;

    public async Task<Result<IEnumerable<SearchProcessDto>>> HandleAsync(SearchProcessCommand cmd, CancellationToken ct = default)
    {
        using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));

        try
        {
            var engineRequest = new BpmDomain.Commands.SearchProcessCommand
            {
                DefinitionType = cmd.DefinitionType,
                IdState = cmd.IdState,
                IsClosed = cmd.IsClosed,
                Category = cmd.Category,
                Filters = cmd.Filters,
                Columns = cmd.Columns,
                Sort = cmd.Sort,
                ExportType = cmd.ExportType,
                Page = cmd.Page,
                Size = cmd.Size
            };

            var result = await _engine.SearchProcessAsync(engineRequest, ct);

            var mapped = result.Select(x => new SearchProcessDto
            {
                Page = x.Page,
                Size = x.Size,
                TotalElements = x.TotalElements,
                TotalPages = x.TotalPages
            });

            return Result<IEnumerable<SearchProcessDto>>.Ok(mapped);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Unauthorized<IEnumerable<SearchProcessDto>>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.NotFound<IEnumerable<SearchProcessDto>>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Internal<IEnumerable<SearchProcessDto>>(ex.Message);
        }
    }
}
