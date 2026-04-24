namespace BpmWebApi.Contracts;

public class SearchProcessRequest
{
    public string[]? DefinitionType { get; set; }
    public string[]? IdState { get; set; }
    public bool? IsClosed { get; set; }
    public string[]? Category { get; set; }
    public object[]? Filters { get; set; }
    public required string[] Columns { get; set; }
    public object[]? Sort { get; set; }
    public bool? Grouped { get; set; }
    public string? ExportType { get; set; }
    public int? Page { get; set; }
    public int? Size { get; set; }

}
