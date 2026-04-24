namespace BpmDomain.Commands;
public record SearchProcessCommand
{
    public string[]? DefinitionType { get; init; }
    public string[]? IdState { get; init; }
    public bool? IsClosed { get; init; }
    public string[]? Category { get; init; }
    public object[]? Filters { get; init; }
    public required string[] Columns { get; init; }
    public object[]? Sort { get; init; }
    public bool? Grouped { get; init; }
    public string? ExportType { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
}

