using System.Text.Json;
using System.Text.Json.Nodes;

namespace BpmDomain.Common
{
    public static class Utility
    {
        public static dynamic? GetValueFromObject(this object? value)
        {
            dynamic? result;

            var isNumeric = decimal.TryParse(value?.ToString(), out decimal numeric);
            var isBoolean = bool.TryParse(value?.ToString(), out bool boolean);
            var isDate = DateTime.TryParse(value?.ToString(), out DateTime dateTime);

            if (isNumeric)
                result = numeric;
            else if (isBoolean)
                result = boolean;
            else if (isDate)
                result = dateTime;
            else
            {
                if (value?.GetType().Name == "JsonValueOfElement" || value?.GetType().Name == "JsonElement")
                    result = value?.ToString();
                else
                    result = value;
            }

            return result;
        }

        public static DateTime ToDateTime(this object? value)
        {
            DateTime result = DateTime.MinValue;

            if (value != null && value?.ToString() != String.Empty)
                _ = DateTime.TryParse(value?.ToString(), out result);

            return result;
        }
    }
}