namespace BpmApplication.Errors;

public class ApiError
{
    public string Code { get; set; } = "";
    public string Message { get; set; } = "";
    public Dictionary<string, object?> Details { get; set; } = new();
}
