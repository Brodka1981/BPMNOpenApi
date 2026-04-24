namespace BpmApi.Contracts;
public class PendingProcessRequest
{
    public string? Category { get; set; }
    public string? State { get; set; }
    public string? AssignedTo { get; set; }
}

