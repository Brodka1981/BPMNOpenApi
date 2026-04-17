namespace BpmInfrastructure.Models
{
    public class GetVariableSqlValues
    {
        public long? VariableId { get; set; }
        public long? TenantId { get; set; }
        public long? ProcessInstanceId { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? ValueType { get; set; }
        public string? ValueString { get; set; }
        public decimal? ValueNumber { get; set; }
        public DateTime? ValueDate { get; set; }
        public bool? ValueBoolean { get; set; }
        public string? ValueJson { get; set; }
    }
}
