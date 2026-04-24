namespace BpmApplication.DTO;

public class SearchProcessDto
{
    public List<List<SearchProcessItemDto>> Items { get; set; } = [];
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
}

public class SearchProcessItemDto
{
    public string Key { get; set; } = string.Empty;
    public object? Value { get; set; }
}

