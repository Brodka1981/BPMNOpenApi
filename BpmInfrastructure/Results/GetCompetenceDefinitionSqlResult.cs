namespace BpmInfrastructure.Results;

public class GetCompetenceDefinitionSqlResult
{
    public long CompetenceDefinitionId { get; set; }
    public string? CompetenceType { get; set; }
    public string? ProcessDefinitionId { get; set; }
    public long TenantId { get; set; }
    public string? ParticipantId { get; set; }
}