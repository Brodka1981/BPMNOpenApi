namespace BpmInfrastructure.Models
{
    public class GetProcessInstanceSqlParms
    {
        public long ProcessInstanceId { get; set; }
        public string? User { get; set; }
        public string? Company { get; set; }
    }
}