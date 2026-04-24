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
    : ICommandHandler<SearchProcessCommand, Result<SearchProcessDto>>
{
    private readonly IBpmEngine _engine = engine;

    public async Task<Result<SearchProcessDto>> HandleAsync(SearchProcessCommand cmd, CancellationToken ct = default)
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
                Grouped = cmd.Grouped,
                ExportType = cmd.ExportType,
                Page = cmd.Page,
                Size = cmd.Size
            };

            var result = await _engine.SearchProcessAsync(engineRequest, ct);

            var mapped = new SearchProcessDto
            {
                Items = result.Items
                    .Select(row => row
                        .Select(item => new SearchProcessItemDto
                        {
                            Key = item.Key,
                            Value = item.Value
                        })
                        .ToList())
                    .ToList(),
                Page = result.Page,
                Size = result.Size,
                TotalElements = result.TotalElements,
                TotalPages = result.TotalPages
            };


            return Result<SearchProcessDto>.Ok(mapped);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Unauthorized<SearchProcessDto>(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.NotFound<SearchProcessDto>(ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex.ToString());
            return ResultExtensions.Internal<SearchProcessDto>(ex.Message);
        }
    }
}
