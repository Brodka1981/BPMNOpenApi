using BpmInfrastructure.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BpmInfrastructure.Common
{
    public static class GetContextUtility
    {
        /// <summary>
        /// Get Variables
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getVariableSqlValues"></param>
        /// <returns></returns>
        public static T? GetVariables<T>(IEnumerable<GetVariableSqlValues> getVariableSqlValues)
        {
            T? result = default;

            var warnigDictionaryList = new Dictionary<string, object>();

            if (getVariableSqlValues?.ToList().Count > 0)
            {
                foreach (var item in getVariableSqlValues)
                {
                    var getValueResult = GetValue(item);
                    warnigDictionaryList[getValueResult.Name] = getValueResult.Value;
                }

                string warnigsInJson = JsonSerializer.Serialize(warnigDictionaryList);

                if(typeof(T).Name.Contains("List`"))
                {
                    result = JsonSerializer.Deserialize<T>("[" + warnigsInJson + "]");
                }
                else if (typeof(T).Name.Contains("Dictionary`"))
                {
                    result = JsonSerializer.Deserialize<T>(warnigsInJson);
                }
                else
                    result = JsonSerializer.Deserialize<T>(warnigsInJson);
            }

            return result;
        }

        /// <summary>
        /// Get Value
        /// </summary>
        /// <param name="getVariableSqlValues"></param>
        /// <returns></returns>
        public static (string Name, object Value) GetValue(GetVariableSqlValues getVariableSqlValues)
        {
            (string Name, object Value) result;
            result.Name = String.Empty;
            result.Value = String.Empty;

            switch (getVariableSqlValues?.ValueType?.Trim().ToLower())
            {
                case "boolean":
                    result.Name = getVariableSqlValues.Name.ToStringFromObject();
                    result.Value = getVariableSqlValues.ValueBoolean.ToBoolFromObject();
                    break;
                case "date":
                    result.Name = getVariableSqlValues.Name.ToStringFromObject();
                    result.Value = getVariableSqlValues.ValueDate.ToDateTimeFromObject();
                    break;
                case "json":
                    result.Name = getVariableSqlValues.Name.ToStringFromObject();
                    result.Value = getVariableSqlValues.ValueJson.ToStringFromObject();
                    break;
                case "number":
                    result.Name = getVariableSqlValues.Name.ToStringFromObject();
                    result.Value = getVariableSqlValues.ValueNumber.ToDecimalFromObject();
                    break;
                case "string":
                    result.Name = getVariableSqlValues.Name.ToStringFromObject();
                    result.Value = getVariableSqlValues.ValueString.ToStringFromObject();
                    break;
            }

            return result;
        }

        /// <summary>
        /// Get Key Name From JsonObject
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static string GetKeyNameFromJsonObject(this JsonObject jsonObject)
        {
            var result = "";

            foreach (KeyValuePair<string, JsonNode?> subObj in jsonObject)
                result = subObj.Key;

            return result;
        }
    }
}