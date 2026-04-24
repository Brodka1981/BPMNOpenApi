using BpmDomain.Compiler.Methods;
using BpmDomain.Compiler.Models;
using System.Text.Json.Nodes;

namespace BpmDomain.Compiler;

public class ExecuteCodeClass(MethodsParameters methodsParameters)
{
    private readonly MethodsParameters _methodsParameters = methodsParameters;

    public List<JsonObject>? GetFields()
    { return _methodsParameters.Fields; }

    public JsonObject? GetField()
    { return _methodsParameters.Field; }

    public string? Code()
    { return _methodsParameters.Code; }

    public List<JsonObject>? GetWarnings()
    { return _methodsParameters.Warnings; }

    public string? CurrentState()
    { return _methodsParameters.CurrentState; }

    public BaseMethods BaseMethods()
    {
        _methodsParameters.Fields ??= [];
        _methodsParameters.Field ??= [];
        _methodsParameters.Warnings ??= [];
        return new BaseMethods(_methodsParameters);
    }

    public CustomMethods CustomMethods()
    {
        _methodsParameters.Fields ??= [];
        _methodsParameters.Field ??= [];
        _methodsParameters.Warnings ??= [];
        return new CustomMethods(_methodsParameters);
    }

    public DateTime GetDateTime(object? value)
    {
        DateTime result = DateTime.MinValue;

        if (value != null && value?.ToString() != String.Empty)
            _ = DateTime.TryParse(value?.ToString(), out result);

        return result;
    }
}