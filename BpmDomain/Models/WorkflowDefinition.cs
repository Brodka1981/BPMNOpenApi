namespace BpmDomain.Models;

public class WorkflowDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    public string StartEventId { get; set; } = "";

    public GlobalUserTaskDefinition GlobalForm { get; set; } = new();

    public List<StateUserTaskDefinition> States { get; set; } = new();
    public List<ActionUserTaskDefinition> ActionTasks { get; set; } = new();
    public List<TaskDefinition> SystemTasks { get; set; } = new();
    public List<GatewayDefinition> Gateways { get; set; } = new();

    public WorkflowDefinition(
        string id,
        string name,
        string startEventId,
        GlobalUserTaskDefinition globalForm,
        List<StateUserTaskDefinition> states,
        List<ActionUserTaskDefinition> actionTasks,
        List<TaskDefinition> systemTasks,
        List<GatewayDefinition> gateways)
    {
        Id = id;
        Name = name;
        StartEventId = startEventId;
        GlobalForm = globalForm;
        States = states;
        ActionTasks = actionTasks;
        SystemTasks = systemTasks;
        Gateways = gateways;
    }

    public WorkflowDefinition()
    {
    }

    public object GetNode(string id)
    {
        return (object?)States.FirstOrDefault(s => s.Id == id)
            ?? (object?)ActionTasks.FirstOrDefault(u => u.Id == id)
            ?? (object?)SystemTasks.FirstOrDefault(t => t.Id == id)
            ?? (object?)Gateways.FirstOrDefault(g => g.Id == id)
            ?? throw new Exception($"Node {id} not found");
    }

    public StateUserTaskDefinition GetState(string id) =>
        States.First(s => s.Id == id);
}
