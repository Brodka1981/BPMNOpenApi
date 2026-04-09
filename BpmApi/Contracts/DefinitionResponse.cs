namespace BpmWebApi.Contracts;
public class DefinitionListItem
{
    public int DefinitionId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
}

public class BpmnResponse
{
    public string FileName { get; set; } = "";
    public byte[] Content { get; set; } = [];
}
