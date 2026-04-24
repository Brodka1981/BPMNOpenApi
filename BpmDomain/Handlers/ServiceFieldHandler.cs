using BpmDomain.Common;
using BpmDomain.Handlers.Interfaces;
using BpmInfrastructure.Common;
using System.Text.Json.Nodes;

namespace BpmDomain.Handlers;

public class ServiceFieldHandler : IServiceFieldHandler
{
    public string OperationType => nameof(ServiceFieldHandler);

    /// <summary>
    /// Execute base method
    /// </summary>
    /// <param name="field"></param>
    /// <param name="variables"></param>
    /// <returns></returns>
    public virtual JsonObject? Execute(JsonObject field, Dictionary<string, object?>? variables)
    {
        var nameField = field["name"].ToStringFromObject();

        if (variables != null && variables.TryGetValue(nameField, out object? value))
        {
            if (value != null)
                field["value"] = value?.GetValueFromObject();
        }

        return field;
    }
}