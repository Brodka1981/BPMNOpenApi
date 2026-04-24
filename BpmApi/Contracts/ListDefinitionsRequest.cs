namespace BpmWebApi.Contracts;

public class ListDefinitionsRequest
{
    public List<Dictionary<string, string>>? Filters { get; set; }
}
