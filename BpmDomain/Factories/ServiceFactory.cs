using BpmDomain.Factories.Interfaces;
using BpmDomain.Handlers.Interfaces;

namespace BpmDomain.Factories
{
    public class ServiceFactory(IEnumerable<IServiceFieldHandler> fieldHandlers, IEnumerable<IServiceTaskHandler> taskHandlers) : IServiceFactory
    {
        private readonly IEnumerable<IServiceFieldHandler> _fieldHandlers = fieldHandlers;
        private readonly IEnumerable<IServiceTaskHandler> _taskHandlers = taskHandlers;

        /// <summary>
        /// Field Resolve
        /// </summary>
        /// <param name="operationType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IServiceFieldHandler FieldResolve(string operationType)
        {
            return _fieldHandlers.FirstOrDefault(h => h.OperationType == operationType)
            ?? throw new InvalidOperationException(
                $"No handlers found for '{operationType}'");
        }

        /// <summary>
        /// Task Resolve
        /// </summary>
        /// <param name="operationType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IServiceTaskHandler TaskResolve(string operationType)
        {
            return _taskHandlers.FirstOrDefault(h => h.OperationType == operationType)
            ?? throw new InvalidOperationException(
                $"No handlers found for '{operationType}'");
        }
    }
}