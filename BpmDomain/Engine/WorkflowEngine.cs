using BpmDomain.Engine.Interfaces;
using BpmDomain.Factories.Interfaces;
using BpmDomain.Models;
using BpmDomain.Results;

namespace BpmDomain.Engine;

public class WorkflowEngine(
    IServiceFactory serviceFactory,
    IActionRequirementRegistry requirementRegistry,
    IExpressionEvaluator expressionEvaluator) : IWorkflowEngine
{
    private readonly IServiceFactory _serviceFactory = serviceFactory;
    private readonly IActionRequirementRegistry _requirementRegistry = requirementRegistry;
    private readonly IExpressionEvaluator _expressionEvaluator = expressionEvaluator;

    // ------------------------------------------------------------
    // START
    // ------------------------------------------------------------
    public ExecutionResult Start(
        WorkflowDefinition definition,
        Dictionary<string, object?> variables)
    {
        if (string.IsNullOrWhiteSpace(definition.StartEventId))
            return Error("StartEventId not defined", variables);

        return MoveFrom(definition, definition.StartEventId, variables);
    }

    // ------------------------------------------------------------
    // EXECUTE (da stato corrente o da azione)
    // ------------------------------------------------------------
    public ExecutionResult Execute(
        WorkflowDefinition definition,
        string currentNodeId,
        Dictionary<string, object?> variables,
        string? actionId = null)
    {
        // Se è stata scelta un'azione, la valutiamo
        if (!string.IsNullOrWhiteSpace(actionId))
        {
            var state = definition.States.FirstOrDefault(s => s.Id == currentNodeId);
            if (state == null)
                return Error($"State '{currentNodeId}' not found", variables);

            var action = state.Actions.FirstOrDefault(a => a.Id == actionId);
            if (action == null)
                return Error($"Action '{actionId}' not found on state '{currentNodeId}'", variables);

            return ExecuteAction(definition, state, action, variables).Result;
        }

        // Altrimenti riprendiamo dal nodo corrente
        return MoveFrom(definition, currentNodeId, variables);
    }

    // ------------------------------------------------------------
    // NAVIGAZIONE NODI
    // ------------------------------------------------------------
    private ExecutionResult MoveFrom(
        WorkflowDefinition definition,
        string nodeId,
        Dictionary<string, object?> variables)
    {
        var node = definition.GetNode(nodeId);

        switch (node)
        {
            case StateUserTaskDefinition state:
                return HandleState(state, variables);

            case ActionUserTaskDefinition ui:
                return HandleUiTask(ui, variables);

            case TaskDefinition sys:
                return HandleSystemTask(definition, sys, variables).Result;

            case GatewayDefinition gw:
                return HandleGateway(definition, gw, variables);

            default:
                return Error($"Unknown node '{nodeId}'", variables);
        }
    }

    // ------------------------------------------------------------
    // STATE
    // ------------------------------------------------------------
    private static ExecutionResult HandleState(
        StateUserTaskDefinition state,
        Dictionary<string, object?> variables) {
        return new()
        {
            CurrentNodeId = state.Id,
            CurrentNodeName = state.Name,
            Variables = variables
        };
    }

    // ------------------------------------------------------------
    // UI TASK
    // ------------------------------------------------------------
    private static ExecutionResult HandleUiTask(
        ActionUserTaskDefinition ui,
        Dictionary<string, object?> variables)
    {
        return new()
        {
            CurrentNodeId = ui.Id,
            CurrentNodeName = ui.Name,
            Variables = variables
        };
    }

    // ------------------------------------------------------------
    // ACTION + REQUIREMENTS
    // ------------------------------------------------------------
    private async Task<ExecutionResult> ExecuteAction(
        WorkflowDefinition definition,
        StateUserTaskDefinition state,
        ActionDefinition action,
        Dictionary<string, object?> variables)
    {
        // 1. Valutazione requisiti
        foreach (var req in action.Requirements)
        {
            var handler = _requirementRegistry.Resolve(req.Type);
            var ok = await handler.EvaluateAsync(req, variables);
            if (!ok)
            {
                return Error(
                    $"Requirement '{req.Type}' failed for action '{action.Id}' on state '{state.Id}'",
                    variables);
            }
        }

        // 2. Navigazione al nodo successivo
        if (string.IsNullOrWhiteSpace(action.TargetNodeId))
            return Error($"Action '{action.Id}' has no TargetNodeId", variables);

        return MoveFrom(definition, action.TargetNodeId, variables);
    }

    // ------------------------------------------------------------
    // SYSTEM TASK
    // ------------------------------------------------------------
    private async Task<ExecutionResult> HandleSystemTask(
        WorkflowDefinition definition,
        TaskDefinition sys,
        Dictionary<string, object?> variables)
    {
        var handler = _serviceFactory.TaskResolve(sys.Type);
        await handler.ExecuteAsync(sys, variables);

        if (string.IsNullOrWhiteSpace(sys.NextNodeId))
            return Error($"SystemTask '{sys.Id}' has no NextNodeId", variables);

        return MoveFrom(definition, sys.NextNodeId, variables);
    }

    // ------------------------------------------------------------
    // GATEWAY + CONDIZIONI
    // ------------------------------------------------------------
    private ExecutionResult HandleGateway(
        WorkflowDefinition definition,
        GatewayDefinition gw,
        Dictionary<string, object?> variables)
    {
        // 1. Proviamo tutte le condizioni
        foreach (var outgoing in gw.Outgoing)
        {
            if (string.IsNullOrWhiteSpace(outgoing.Condition))
                continue;

            var ok = _expressionEvaluator.Evaluate(outgoing.Condition, variables);
            if (ok)
                return MoveFrom(definition, outgoing.TargetNodeId, variables);
        }

        // 2. Default (prima senza condition)
        var defaultFlow = gw.Outgoing.FirstOrDefault(o => string.IsNullOrWhiteSpace(o.Condition));
        if (defaultFlow != null)
            return MoveFrom(definition, defaultFlow.TargetNodeId, variables);

        return Error($"No matching condition on gateway '{gw.Id}'", variables);
    }

    // ------------------------------------------------------------
    // ERROR
    // ------------------------------------------------------------
    private static ExecutionResult Error(string message, Dictionary<string, object?> variables)
    {
        return new()
        {
            CurrentNodeId = "",
            CurrentNodeName = "",
            Variables = variables,
            Errors = { message }
        };
    }
}