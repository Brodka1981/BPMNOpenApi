using BpmApplication.DTO;
using BpmApplication.Queries.Interfaces;
using BpmApplication.Results;

namespace BpmApplication.Queries;

public record GetDefinitionsQuery(
    string User,
    string Company,
    string? Category
) : IQuery<Result<IEnumerable<WorkflowDefinitionDto>>>;


