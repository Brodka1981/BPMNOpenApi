using BpmDomain.Handlers;
using System.Text.Json.Nodes;

namespace BpmServiceFields.Handlers
{
    public class Mail: ServiceFieldHandler
    {
        public new string OperationType => nameof(Mail);

        /// <summary>
        /// Execute
        /// </summary>
        /// <param name="field"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        public override JsonObject? Execute(JsonObject? field, Dictionary<string, object>? variables)
        {


            return field;
        }
    }
}