using BpmInfrastructure.Models;

namespace BpmInfrastructure.Common
{
    public static class Utility
    {
        /// <summary>
        /// Is Oracle
        /// </summary>
        /// <param name="appSettings"></param>
        /// <returns></returns>
        public static bool IsOracle(AppSettings appSettings)
        {
            bool result;
            if (appSettings?.UseDatabaseType?.ToLower() == "oracle") { result = true; }
            else { result = false; }
            return result;
        }

        public static string ToStringFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return string.Empty;
            return value?.ToString();
        }

        public static string? ToStringNullableFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return String.Empty;
            return value?.ToString();
        }

        public static DateTime ToDateTimeFromObject(this object? value)
        {
            return DateTime.Parse(value?.ToString()); 
        }

        public static DateTime? ToDateTimeNullableFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return null;
            return DateTime.Parse(value?.ToString());
        }

        public static long ToLongFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return (long)0;
            return long.Parse(value?.ToString());
        }

        public static long? ToLongNullableFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return null;
            return long.Parse(value?.ToString());
        }

        public static bool ToBoolFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return false;
            return bool.Parse(value?.ToString());
        }

        public static bool? ToBoolNullableFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return null;
            return bool.Parse(value?.ToString());
        }

        public static decimal ToDecimalFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return (decimal)0;
            return decimal.Parse(value?.ToString()?.Replace('.', ','));
        }

        public static decimal? ToDecimalNullableFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return null;
            return decimal.Parse(value?.ToString()?.Replace('.', ','));
        }

        public static double? ToDoubleNullableFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return null;
            return double.Parse(value?.ToString()?.Replace('.', ','));
        }

        public static double ToDoubleFromObject(this object? value)
        {
            if (value == null || value?.ToString() == String.Empty) return (double)0;
            return double.Parse(value?.ToString()?.Replace('.', ','));
        }

        public static IEnumerable<T> ToListJsonObject<T>(this IEnumerable<T>? value)
        {
            var result = new List<T>();
            if (value != null) result = [.. value];
            return result;
        }

        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }
}