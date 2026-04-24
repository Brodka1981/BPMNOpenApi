namespace BpmApplication.Messages;

public class ApiMessage
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public Dictionary<string, object?> Details { get; set; } = new();
}