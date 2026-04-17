using BpmDomain.Engine.Interfaces;
using BpmDomain.Exceptions;
using BpmDomain.Models;
using System.Xml.Linq;

namespace BpmDomain.Engine;

public class BpmnParserService : IBpmnParserService
{
    private readonly XNamespace bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";
    private readonly XNamespace zv = "http://zv-engine.com/schema/user-task";

    public WorkflowDefinition? Parse(string? xml)
    {
        xml ??= String.Empty;
        var doc = XDocument.Parse(xml);
        var definitions = doc.Root ?? throw new GenericException("Invalid BPMN XML");

        var process = definitions.Element(bpmn + "process")
                     ?? throw new GenericException("No <process> found");

        string id = process.Attribute("id")?.Value ?? Guid.NewGuid().ToString();
        string name = process.Attribute("name")?.Value ?? "Unnamed";

        var states = new List<StateUserTaskDefinition>();
        var actionTasks = new List<ActionUserTaskDefinition>();
        var systemTasks = new List<TaskDefinition>();
        var gateways = new List<GatewayDefinition>();

        var globalFields = new List<UserTaskField>();
        var globalActions = new List<ActionDefinition>();

        string startEventId = "";

        // ------------------------------------------------------------
        // 1) Parse nodi
        // ------------------------------------------------------------
        foreach (var node in process.Elements())
        {
            switch (node.Name.LocalName)
            {
                case "startEvent":
                    startEventId = node.Attribute("id")?.Value ?? "";
                    break;

                case "userTask":
                    ParseUserTask(node, states, actionTasks, globalFields);
                    break;

                case "serviceTask":
                    ParseServiceTask(node, systemTasks);
                    break;

                case "exclusiveGateway":
                    ParseGateway(node, gateways);
                    break;
            }
        }

        // ------------------------------------------------------------
        // 2) Parse sequence flows
        // ------------------------------------------------------------
        foreach (var flow in process.Elements(bpmn + "sequenceFlow"))
        {
            var source = flow.Attribute("sourceRef")?.Value ?? "";
            var target = flow.Attribute("targetRef")?.Value ?? "";

            var condition = flow.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "conditionExpression")
                ?.Value;

            LinkFlow(states, actionTasks, systemTasks, gateways, source, target, condition);
        }

        // ------------------------------------------------------------
        // 3) Costruzione GlobalUserTaskDefinition
        // ------------------------------------------------------------
        var global = new GlobalUserTaskDefinition(
            fields: globalFields,
            actions: globalActions
        );

        // ------------------------------------------------------------
        // 4) Costruzione WorkflowDefinition
        // ------------------------------------------------------------
        return new WorkflowDefinition(
            id,
            name,
            startEventId,
            global,
            states,
            actionTasks,
            systemTasks,
            gateways
        );
    }

    // ------------------------------------------------------------
    // USER TASK
    // ------------------------------------------------------------
    private void ParseUserTask(
        XElement element,
        List<StateUserTaskDefinition> states,
        List<ActionUserTaskDefinition> actionTasks,
        List<UserTaskField> globalFields)
    {
        var id = element.Attribute("id")?.Value ?? Guid.NewGuid().ToString();
        var name = element.Attribute("name")?.Value ?? "";

        var fields = ParseForms(element);

        // 1) Global form
        if (name == "Apertura standard")
        {
            globalFields.AddRange(fields);
            return;
        }

        // 2) State (stato_*)
        if (id.StartsWith("stato_"))
        {
            states.Add(new StateUserTaskDefinition
            {
                Id = id,
                Name = name,
                Fields = fields,
                Actions = []
            });
            return;
        }

        // 3) ActionUserTaskDefinition (conferma/annulla)
        actionTasks.Add(new ActionUserTaskDefinition
        {
            Id = id,
            Name = name,
            Fields = fields,
            Confirm = new ActionDefinition(
                id: "confirm",
                label: "Conferma",
                targetNodeId: "",
                requirements: []),
            Cancel = new ActionDefinition(
                id: "cancel",
                label: "Annulla",
                targetNodeId: "",
                requirements: [])
        });
    }

    // ------------------------------------------------------------
    // SERVICE TASK
    // ------------------------------------------------------------
    private static void ParseServiceTask(XElement element, List<TaskDefinition> systemTasks)
    {
        var id = element.Attribute("id")?.Value ?? Guid.NewGuid().ToString();
        var name = element.Attribute("name")?.Value ?? "";

        var type = name.Contains("CambioStato") ? "CambioStato" : name;

        var sys = new TaskDefinition(
            id: id,
            label: name,
            type: type,
            nextNodeId: null,
            parameters: new Dictionary<string, object?>()
        );

        systemTasks.Add(sys);
    }

    // ------------------------------------------------------------
    // GATEWAY
    // ------------------------------------------------------------
    private static void ParseGateway(XElement element, List<GatewayDefinition> gateways)
    {
        var id = element.Attribute("id")?.Value ?? Guid.NewGuid().ToString();
        var name = element.Attribute("name")?.Value ?? "";

        gateways.Add(new GatewayDefinition(
            id: id,
            label: name,
            outgoing: []
        ));
    }

    // ------------------------------------------------------------
    // FORMS
    // ------------------------------------------------------------
    private List<UserTaskField> ParseForms(XElement element)
    {
        var result = new List<UserTaskField>();

        var ext = element.Element(bpmn + "extensionElements");
        if (ext == null)
            return result;

        var elements = ext.Elements(zv + "ui");


        if (elements != null && elements.ToList()?.Count > 0)
        {
            foreach (var ui in elements)
            {
                var json = ui.Value.Trim();
                if (!string.IsNullOrWhiteSpace(json))
                    result.Add(new UserTaskField(json));
            }
        }
        else
        {
            var json = ext.Value.Trim();
            if (!string.IsNullOrWhiteSpace(json))
                result.Add(new UserTaskField(json));
        }

        return result;
    }

    // ------------------------------------------------------------
    // LINK FLOWS
    // ------------------------------------------------------------
    private static void LinkFlow(
        List<StateUserTaskDefinition> states,
        List<ActionUserTaskDefinition> actionTasks,
        List<TaskDefinition> systemTasks,
        List<GatewayDefinition> gateways,
        string source,
        string target,
        string? condition)
    {
        // 1) State → Action dinamica
        var state = states.FirstOrDefault(s => s.Id == source);
        if (state != null)
        {
            state.Actions.Add(new ActionDefinition(
                id: Guid.NewGuid().ToString(),
                label: condition ?? "",
                targetNodeId: target,
                requirements: []
            ));
            return;
        }

        // 2) ActionUserTaskDefinition → Confirm/Cancel
        var ui = actionTasks.FirstOrDefault(u => u.Id == source);
        if (ui != null)
        {
            bool isCancel = condition?.Contains("annulla", StringComparison.OrdinalIgnoreCase) == true;

            var action = new ActionDefinition(
                id: Guid.NewGuid().ToString(),
                label: isCancel ? "Annulla" : "Conferma",
                targetNodeId: target,
                requirements: []
            );

            if (isCancel)
                ui.Cancel = action;
            else
                ui.Confirm = action;

            return;
        }

        // 3) Gateway → outgoing
        var gw = gateways.FirstOrDefault(g => g.Id == source);
        if (gw != null)
        {
            gw.Outgoing.Add(new GatewayOutgoing(
                condition ?? "",
                target
            ));
            return;
        }

        // 4) ServiceTask → ricostruzione con NextNodeId
        var sysIndex = systemTasks.FindIndex(s => s.Id == source);
        if (sysIndex >= 0)
        {
            var old = systemTasks[sysIndex];

            var updated = new TaskDefinition(
                id: old.Id,
                label: old.Label,
                type: old.Type,
                nextNodeId: target,
                parameters: old.Parameters
            );

            systemTasks[sysIndex] = updated;
        }
    }
}