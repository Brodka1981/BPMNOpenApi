namespace BpmApplication.DTO;

public class SearchProcessDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
}
