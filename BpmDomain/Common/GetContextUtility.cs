using BpmDomain.Compiler;
using BpmDomain.Compiler.Models;
using BpmDomain.Exceptions;
using BpmDomain.Factories.Interfaces;
using BpmDomain.Models;
using BpmDomain.NLog;
using BpmInfrastructure.Common;
using BpmInfrastructure.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using System.Xml;

namespace BpmDomain.Common
{
    public static class GetContextUtility
    {       
        /// <summary>
        /// To Get Context Result
        /// </summary>
        /// <param name="_workflowDefinition"></param>
        /// <param name="toGetContextParms"></param>
        /// <param name="_serviceFactory"></param>
        /// <returns></returns>
        public static Results.GetContextResult ToGetContextResult(this WorkflowDefinition? _workflowDefinition, ToGetContextParms toGetContextParms, IServiceFactory _serviceFactory)
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

            _state ??= new StateUserTaskDefinition() { };

            //add fields from current state
            fields.AddRange(AddFieldsFromCurrentState(_state.Fields));

            //add actions from GlobalForm
            foreach (var item in _workflowDefinition.GlobalForm.Actions)
                actions.Add(new Models.Action() { IdAction = item.Id, Description = item.Label });

            //take actions from current state
            foreach (var item in _state.Actions)
                actions.Add(new Models.Action() { IdAction = item.Id, Description = item.Label });

            //take and add field Values from variables
            fields = AddFieldValuesFromVariables(toGetContextParms.Variables, fields, _serviceFactory);

            warnings = BpmInfrastructure.Common.GetContextUtility.GetVariables<List<JsonObject>>([.. toGetContextParms.Variables.ToListJsonObject().Where(_ => _.Type?.Trim().ToLower() == "warning")]);

            //take the code in the field and execute it
            fields = ExecuteCodeFromField(new MethodsParameters() { Fields = fields, Warnings = warnings, CurrentState = toGetContextParms.CurrentState });

            //populate stection for fields
            if (fields?.Count > 0)
                sections.Add(new Models.Section() { Title = "Fields", Type = "Fields", Fields = fields });

            //populate stection for warnings
            if (warnings?.Count > 0)
                sections.Add(new Models.Section() { Title = "Warnings", Type = "Warnings", Fields = warnings });

            //get variables
            variables = BpmInfrastructure.Common.GetContextUtility.GetVariables<List<JsonObject>>([.. toGetContextParms.Variables.ToListJsonObject().Where(_ => _.Type?.Trim().ToLower() == "variable")]);

            if (actions?.Count == 0) actions = null;
            if (sections?.Count == 0) sections = null;
            if (variables?.Count == 0) variables = null;

            return new Results.GetContextResult()
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

        private static List<JsonObject> AddFieldsFromCurrentState(List<Models.UserTaskField> userTaskFields)
        {
            var result = new List<JsonObject>();

            foreach (var item in userTaskFields)
            {
                var json = item.Json;

                json = json.Replace("&#9;", @"\").Replace("&quot;", @""""); //for manage quotation marks into string 
                json = json.Replace("&#9;", @"\").Replace("&lt;", @"<").Replace("&gt;", @">"); //for manage major and minor symbols into string 

                var _fields = JsonSerializer.Deserialize<List<JsonObject>>(json);

                foreach (var _field in _fields.ToListJsonObject())
                    result.Add(_field);
            }

            return result;
        }

        private static List<JsonObject>? AddFieldValuesFromVariables(List<GetVariableSqlValues>? variables, List<JsonObject>? fields, IServiceFactory serviceFactory)
        {
            using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));

            var result = new List<JsonObject>() { };
            var getVariablesResult = BpmInfrastructure.Common.GetContextUtility.GetVariables<Dictionary<string, object>>([.. variables.ToListJsonObject()]);

            if (fields == null)
                return null;

            foreach (var field in fields)
            {
                result?.Add(field);

                var currentField = result?.LastOrDefault();
                var typeField = field["type"].ToStringFromObject();

                try
                {
                    if (currentField != null && typeField != String.Empty)
                        _ = serviceFactory.FieldResolve(typeField.FirstCharToUpper()).Execute(currentField, getVariablesResult);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }

            return result;
        }

        private static List<JsonObject>? ExecuteCodeFromField(MethodsParameters methodsParameters)
        {
            using var logger = new NLogScope(LogManager.GetCurrentClassLogger(), NLogUtility.GetMethodToNLog(MethodInfo.GetCurrentMethod()));

            if (methodsParameters.Fields == null)
                return null;

            foreach (var field in methodsParameters.Fields)
            {
                var code = field["code"].ToStringFromObject();

                if (code != null && code != String.Empty)
                {
                    methodsParameters.Code = code;
                    methodsParameters.Field = field;
                    var _params = new ExecuteCodeClass(methodsParameters);

                    try
                    {
                        var executeCodeResult = CompilerManager.ExecuteCode(_params);

                        if (executeCodeResult.Exception != null)
                            throw new GenericException(executeCodeResult.Exception.Message);

                        methodsParameters.Fields = executeCodeResult.JsonObjectList;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                    }
                }
            }

            return methodsParameters.Fields;
        }
    }
}