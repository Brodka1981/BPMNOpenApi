using BpmDomain.Handlers;
using BpmDomain.Handlers.Interfaces;
using System.Text.Json.Nodes;

namespace BpmServiceFields.Handlers
{
    public class Number: ServiceFieldHandler, IServiceFieldHandler
    {
        public new string OperationType => nameof(Number);

        /// <summary>
        /// Execute override method
        /// </summary>
        /// <param name="field"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        //public override JsonObject? Execute(JsonObject? field, Dictionary<string, object?>? variables)
        //{


        //    return field;
        //}
    }
}