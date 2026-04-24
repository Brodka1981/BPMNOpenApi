namespace BpmApplication.DTO;
public record AvailableAction
{
    public int ActionId { get; set; }   // ID_OPERAZIONE legacy
    public string Label { get; set; } = "";
}

public record FormError
{
    public string Field { get; set; } = "";
    public string Message { get; set; } = "";
}