using BpmDomain.Handlers;
using BpmDomain.Handlers.Interfaces;
using BpmDomain.Models;

namespace BpmServiceTasks.Handlers
{
    public class Task1 : ServiceTaskHandler, IServiceTaskHandler
    {
        public new string OperationType => nameof(Task1);

        /// <summary>
        /// ExecuteAsync
        /// </summary>
        /// <param name="taskDefinition"></param>
        /// <param name="variables"></param>
        /// <returns></returns>
        //public override async Task ExecuteAsync(TaskDefinition taskDefinition, Dictionary<string, object?> variables)
        //{
        //    //TODO
        //}
    }
}