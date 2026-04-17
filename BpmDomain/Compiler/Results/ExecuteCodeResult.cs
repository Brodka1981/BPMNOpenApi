using Microsoft.CodeAnalysis.Scripting;
using System.Text.Json.Nodes;

namespace BpmDomain.Compiler.Results;

public class ExecuteCodeResult
{
    public List<JsonObject>? JsonObjectList { get; set; }
    public JsonObject? JsonObject { get; set; }
    public bool Success { get; set; }
    public Exception? Exception { get; set; }
    public dynamic? ReturnValue { get; set; }
    public IEnumerable<ScriptVariable>? Variables { get; set; }
}