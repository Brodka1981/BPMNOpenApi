using System.Text.Json.Nodes;

namespace BpmDomain.Compiler.Models;

public class MethodsParameters
{
    public List<JsonObject>? Fields { get; set; }
    public JsonObject? Field { get; set; }
    public List<JsonObject>? Warnings { get; set; }
    public string? Code { get; set; }
    public string? CurrentState { get; set; }
}