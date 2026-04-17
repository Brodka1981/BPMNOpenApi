using System.Text.Json.Nodes;

namespace BpmDomain.Common
{
    public static class Utility
    {
        public static dynamic? GetValueFromObject(this object? jsonValue)
        {
            dynamic? result;

            var isNumeric = decimal.TryParse(jsonValue?.ToString(), out decimal numeric);
            var isBoolean = bool.TryParse(jsonValue?.ToString(), out bool boolean);

            if (isNumeric)
                result = numeric;
            else if (isBoolean)
                result = boolean;
            else
                result = jsonValue?.ToString();

            return result;
        }
    }
}