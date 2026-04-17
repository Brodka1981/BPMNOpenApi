namespace BpmInfrastructure.Models
{
    public class IsUserActivityCompleteSqlParms
    {
        public long ProcessInstanceId { get; set; }
        public string? StateName { get; set; }
        public string? User { get; set; }
        public string? Company { get; set; }
    }
}