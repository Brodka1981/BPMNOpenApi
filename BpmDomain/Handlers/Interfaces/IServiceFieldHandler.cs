using System.Text.Json.Nodes;
namespace BpmDomain.Handlers.Interfaces;

public interface IServiceFieldHandler
{
    string OperationType { get; }
    JsonObject? Execute(JsonObject field, Dictionary<string, object?>? variables);
}