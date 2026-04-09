using BpmDomain.Models;
using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BpmDomain.Common
{
    public static class GetContextUtility
    {
        /// <summary>
        /// To Get Context Result
        /// </summary>
        /// <param name="_workflowDefinition"></param>
        /// <param name="toGetContextParms"></param>
        /// <returns></returns>
        public static Results.GetContextResult ToGetContextResult(this WorkflowDefinition? _workflowDefinition, ToGetContextParms toGetContextParms)
        {
            var actions = new List<Models.Action>();
            List<JsonObject>? variables = null;
            var sections = new List<Models.Section>();
            Models.State? state = null;
            var fields = new List<JsonObject>() { };
            List<JsonObject>? warnings = null;

            if (_workflowDefinition == null) 
                return new Results.GetContextResult() { };

            //add fields from GlobalForm
            fields.AddRange(AddFieldsFromGlobalForm(_workflowDefinition.GlobalForm.Fields));

            //take variables and fields from current state
            var _state = _workflowDefinition.States.Where(_ => _.Id?.Trim() == toGetContextParms.CurrentState?.Trim())?.FirstOrDefault();

            //add current state details
            if (_state != null)
                state = new Models.State() { IdState = _state.Id, Description = _state.Name };

            if (_state == null) _state = new StateUserTaskDefinition() { };

            //add fields from current state
            fields.AddRange(AddFieldsFromCurrentState(_state.Fields));

            //add actions from GlobalForm
            foreach (var item in _workflowDefinition.GlobalForm.Actions)
                actions.Add( new Models.Action() { IdAction = item.Id, Description = item.Label });

            //take actions from current state
            foreach (var item in _state.Actions)
                actions.Add(new Models.Action() { IdAction = item.Id, Description = item.Label });

            //take and add field Values from variables
            fields = AddFieldValuesFromVariables(toGetContextParms.Variables, fields);

            //populate stection for fields
            if (fields?.Count > 0)
                sections.Add(new Models.Section() { Title = "Fields", Type = "Fields", Fields = fields });

            warnings = GetVariables<JsonObject>([.. toGetContextParms.Variables.ToListJsonObject().Where(_ => _.Type?.Trim().ToLower() == "warning")]);

            //populate stection for warnings
            if (warnings?.Count > 0)
                sections.Add(new Models.Section() { Title = "Warnings", Type = "Warnings", Fields = warnings });

            //get variables
            variables = GetVariables<JsonObject>([.. toGetContextParms.Variables.ToListJsonObject().Where(_ => _.Type?.Trim().ToLower() == "variable")]);

            if (actions?.Count == 0) actions = null;
            if (sections?.Count == 0) sections = null;
            if (variables?.Count == 0) variables = null;

            return new BpmDomain.Results.GetContextResult()
            {
                ProcessId = toGetContextParms.ProcessInstanceId,
                Name = toGetContextParms.Name,
                ProcessType = toGetContextParms.ProcessType,
                ContextMode = toGetContextParms.ContextMode,
                Actions = actions,
                State = state,
                Variables = variables,
                Form = new Models.Form() { Sections = sections }
            };
        }

        private static List<JsonObject> AddFieldsFromGlobalForm(IReadOnlyList<Models.UserTaskField> userTaskFields)
        {
            var result = new List<JsonObject>();

            foreach (var item in userTaskFields)
            {
                var _fields = JsonSerializer.Deserialize<List<JsonObject>>(item.Json);

                foreach (var _field in _fields.ToListJsonObject())
                    result.Add(_field);
            }

            return result;
        }

        private static List<JsonObject> AddFieldsFromCurrentState(List<BpmDomain.Models.UserTaskField> userTaskFields)
        {
            var result = new List<JsonObject>();

            foreach (var item in userTaskFields)
            {
                var _fields = JsonSerializer.Deserialize<List<JsonObject>>(item.Json);

                foreach (var _field in _fields.ToListJsonObject())
                    result.Add(_field);
            }

            return result;
        }

        private static List<JsonObject>? AddFieldValuesFromVariables(List<GetVariableSqlValues>? variables, List<JsonObject>? fields)
        {
            var fieldValues = GetVariables<JsonObject>([.. variables.ToListJsonObject().Where(_ => _.Type?.Trim().ToLower() == "field")]);

            //Add Value To fields from variables
            foreach (var fieldValue in fieldValues.ToListJsonObject())
            {
                var fieldValueKey = fieldValue.GetKeyNameFromJsonObject();
                fields = fields?.AddValueToJsonObject(fieldValueKey, fieldValue[fieldValueKey]?.GetValue<string>());
            }

            return fields;
        }

        private static string GetKeyNameFromJsonObject(this JsonObject jsonObject)
        {
            var result = "";

            foreach (KeyValuePair<string, JsonNode?> subObj in jsonObject)
                result = subObj.Key;

            return result;
        }

        private static List<JsonObject> AddValueToJsonObject(this List<JsonObject> jsonObjectList, string? key, string? value)
        {
            var result = new List<JsonObject>() { };

            foreach (var jsonObject in jsonObjectList)
            {
                result.Add(jsonObject);

                var currentJsonObject = result.LastOrDefault();

                foreach (KeyValuePair<string, JsonNode?> subObj in jsonObject)
                {
                    if (subObj.Value?.ToString() == key)
                    {
                        currentJsonObject?.Add("value", value);
                        return result;
                    }
                }               
            }

            return result;
        }

        private static List<T>? GetVariables<T>(IEnumerable<GetVariableSqlValues> getVariableSqlValues)
        {
            List<T>? result = null;

            var warnigDictionaryList = new List<Dictionary<string, object>>();

            if (getVariableSqlValues?.ToList().Count > 0)
            {
                foreach (var item in getVariableSqlValues)
                {
                    var getValueResult = GetValue(item);
                    var warnigDictionary = new Dictionary<string, object>();
                    warnigDictionary.Add(getValueResult.Name, getValueResult.Value);
                    warnigDictionaryList.Add(warnigDictionary);
                }

                string warnigsInJson = JsonSerializer.Serialize(warnigDictionaryList);

                result = JsonSerializer.Deserialize<List<T>>(warnigsInJson);
            }

            return result;
        }

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
    }
}