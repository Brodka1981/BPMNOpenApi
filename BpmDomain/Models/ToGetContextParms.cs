using BpmInfrastructure.Models;

namespace BpmDomain.Models
{
    public class ToGetContextParms
    {
        public List<GetVariableSqlValues>? Variables { get; set; }
        public string? CurrentState { get; set; }
        public long? ProcessInstanceId { get; set; }
        public string? Name { get; set; }
        public string? ProcessType { get; set; }
        public string? ContextMode { get; set; }
        public string? CurrentHttpContext { get; set; }
        public AppSettings AppSettings { get; set; } = new();
        public string? Abi { get; set; }
    }
}