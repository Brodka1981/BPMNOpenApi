using BpmDomain.Common;
using BpmDomain.Handlers;
using BpmDomain.Handlers.Interfaces;
using BpmInfrastructure.Common;
using System.Text.Json.Nodes;

namespace BpmServiceFields.Handlers
{
    public class Combobox: ServiceFieldHandler, IServiceFieldHandler
    {
        public new string OperationType => nameof(Combobox);

        /// <summary>
        /// Execute override method
        /// </summary>
        /// <param name="field"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public override JsonObject? Execute(JsonObject field, Dictionary<string, object?>? variables)
        {
            var nameField = field["name"].ToStringFromObject();

            var value = field["value"].ToStringFromObject();

            if (value == null || value == String.Empty)
                value = field["codici"].ToStringFromObject();

            if ((value == null || value == String.Empty) && variables != null && variables.ContainsKey(nameField))
                value = variables?[nameField].ToStringFromObject();

            if (value != null)
                field["value"] = value?.GetValueFromObject();

            return field;
        }
    }
}