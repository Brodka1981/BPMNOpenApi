using BpmDomain.Compiler.Methods;
using System.Text.Json.Nodes;

namespace BpmDomain.Compiler;

public class ExecuteCodeClass(List<JsonObject>? fields, JsonObject? filed, string? code, List<JsonObject>? warnings)
{
    private List<JsonObject>? _fields = fields;
    private JsonObject? _filed = filed;
    private readonly string? _code = code;
    private List<JsonObject>? _warnings = warnings;

    public List<JsonObject>? GetFields()
    { return _fields; }

    public JsonObject? GetField()
    { return _filed; }

    public string? Code()
    { return _code; }

    public List<JsonObject>? GetWarnings()
    { return _warnings; }

    public BaseMethods BaseMethods()
    {
        _fields ??= [];
        _filed ??= [];
        _warnings ??= [];
        return new BaseMethods(_fields, _filed, _warnings);
    }

    public CustomMethods CustomMethods()
    {
        _fields ??= [];
        _filed ??= [];
        _warnings ??= [];
        return new CustomMethods(_fields, _filed, _warnings);
    }
}