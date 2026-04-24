namespace BpmDomain.Models;
public class SequenceFlow
{
    public string Id { get; set; } = "";
    public string SourceRef { get; set; } = "";
    public string TargetRef { get; set; } = "";
    public string? ConditionExpression { get; set; }
}
