namespace BpmInfrastructure.Models
{
    public class GetProcessDefinitionSqlParms 
    {
        public long ProcessDefinitionId { get; set; }
        public string? User { get; set; }
        public string? Company { get; set; }
    }
}