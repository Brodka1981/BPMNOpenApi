namespace BpmDomain.Engine.Interfaces;

public interface IExpressionEvaluator
{
    bool Evaluate(string expression, Dictionary<string, object?> variables);
}
