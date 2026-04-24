using BpmDomain.Engine.Interfaces;
using System.Globalization;

namespace BpmDomain.Engine;

public class SimpleExpressionEvaluator : IExpressionEvaluator
{
    public bool Evaluate(string expression, Dictionary<string, object?> variables)
    {
        // Per semplicità: split su && e ||
        // Esempi supportati:
        //  - amount > 1000
        //  - status == "OK"
        //  - amount > 1000 && status == "OK"

        var orParts = expression.Split("||", StringSplitOptions.TrimEntries);

        foreach (var orPart in orParts)
        {
            var andParts = orPart.Split("&&", StringSplitOptions.TrimEntries);
            var andResult = true;

            foreach (var part in andParts)
            {
                if (!EvaluateAtomic(part, variables))
                {
                    andResult = false;
                    break;
                }
            }

            if (andResult)
                return true;
        }

        return false;
    }

    private bool EvaluateAtomic(string expr, Dictionary<string, object?> variables)
    {
        // Supportiamo operatori base
        var ops = new[] { "==", "!=", ">=", "<=", ">", "<" };

        foreach (var op in ops)
        {
            var idx = expr.IndexOf(op, StringComparison.Ordinal);
            if (idx <= 0) continue;

            var left = expr[..idx].Trim();
            var right = expr[(idx + op.Length)..].Trim();

            var leftVal = ResolveValue(left, variables);
            var rightVal = ResolveValue(right, variables);

            return Compare(leftVal, rightVal, op);
        }

        // Se non c'è operatore, proviamo a interpretare come bool variabile
        var val = ResolveValue(expr.Trim(), variables);
        return ToBool(val);
    }

    private object? ResolveValue(string token, Dictionary<string, object?> variables)
    {
        // string literal
        if (token.StartsWith('"') && token.EndsWith('"'))
            return token.Trim('"');

        // number literal
        if (decimal.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
            return dec;

        // bool literal
        if (bool.TryParse(token, out var b))
            return b;

        // variable
        if (variables.TryGetValue(token, out var value))
            return value;

        return null;
    }

    private bool Compare(object? left, object? right, string op)
    {
        // numerico
        if (left is IConvertible && right is IConvertible &&
            decimal.TryParse(left.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ld) &&
            decimal.TryParse(right.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var rd))
        {
            return op switch
            {
                ">" => ld > rd,
                "<" => ld < rd,
                ">=" => ld >= rd,
                "<=" => ld <= rd,
                "==" => ld == rd,
                "!=" => ld != rd,
                _ => false
            };
        }

        // string/altro
        var ls = left?.ToString();
        var rs = right?.ToString();

        return op switch
        {
            "==" => string.Equals(ls, rs, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(ls, rs, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private bool ToBool(object? value)
    {
        if (value is bool b) return b;
        if (value is string s && bool.TryParse(s, out var parsed)) return parsed;
        if (value is IConvertible c && decimal.TryParse(c.ToString(), out var d)) return d != 0;
        return false;
    }
}
