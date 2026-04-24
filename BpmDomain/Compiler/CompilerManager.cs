using BpmDomain.Compiler.Results;
using BpmDomain.NLog;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using NLog;
using System.Reflection;

namespace BpmDomain.Compiler;

public static class CompilerManager
{
    /// <summary>
    /// Execute Code
    /// </summary>
    /// <param name="_params"></param>
    /// <returns></returns>
    public static ExecuteCodeResult ExecuteCode(ExecuteCodeClass _params)
    {
        var result = new ExecuteCodeResult() { Success = false, Exception = null };

        try
        {
            var opt = ScriptOptions.Default
                .AddReferences(typeof(Enumerable).Assembly) // Riferimento all'assembly LINQ
                .AddReferences(typeof(System.Text.Json.JsonSerializer).Assembly) // Riferimento all'assembly Json
                .AddReferences(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly)
                .AddImports("System.Linq")
                .AddImports("System")
                .AddImports("System.Text")
                .AddImports("System.Text.Json")
                .AddImports("System.Text.Json.Nodes")
                .AddImports("System.Collections.Generic")
                .AddImports("System.Reflection");

            var runAsyncResponse = CSharpScript.RunAsync(_params.Code(), opt, _params, _params.GetType()).Result;

            result.ReturnValue = runAsyncResponse.ReturnValue;
            result.Variables = runAsyncResponse.Variables;
            result.Exception = runAsyncResponse.Exception;

            if (result.Exception != null)
                throw result.Exception;

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Exception = ex;
        }
        finally
        {
            result.JsonObject = _params.GetField();
            result.JsonObjectList = _params.GetFields();
        }

        return result;
    }
}