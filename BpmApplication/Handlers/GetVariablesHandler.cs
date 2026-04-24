using BpmApplication.Handlers.Interfaces;
using BpmApplication.Queries;

namespace BpmApplication.Handlers;

public class GetVariablesHandler : IQueryHandler<GetVariablesQuery, Dictionary<string, object?>>
{
    public Task<Dictionary<string, object?>> HandleAsync(GetVariablesQuery query, CancellationToken ct = default)
    {
        var vars = new Dictionary<string, object?>
        {
            ["example"] = "value"
        };

        return Task.FromResult(vars);
    }
}