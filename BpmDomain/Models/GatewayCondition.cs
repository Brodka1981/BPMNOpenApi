namespace BpmDomain.Models;

public class GatewayCondition
{
    public string Expression { get; set; } = "";

    public bool Evaluate(Dictionary<string, object?> variables)
    {
        if (string.IsNullOrWhiteSpace(Expression))
            return true;

        var parts = Expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        var left = parts[0];
        var op = parts[1];
        var right = parts[2];

        if (!variables.TryGetValue(left, out var value))
            return false;

        var leftValue = value?.ToString();
        var rightValue = right;

        return op switch
        {
            "==" => string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase),
            "!=" => !string.Equals(leftValue, rightValue, StringComparison.OrdinalIgnoreCase),
            ">" => Compare(leftValue, rightValue) > 0,
            "<" => Compare(leftValue, rightValue) < 0,
            ">=" => Compare(leftValue, rightValue) >= 0,
            "<=" => Compare(leftValue, rightValue) <= 0,
            _ => false
        };
    }

    private static int Compare(string? left, string? right)
    {
        if (decimal.TryParse(left, out var l) && decimal.TryParse(right, out var r))
            return l.CompareTo(r);

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }
}

