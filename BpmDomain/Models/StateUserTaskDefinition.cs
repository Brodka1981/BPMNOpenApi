namespace BpmDomain.Models;

public sealed class StateUserTaskDefinition : UserTaskDefinition
{
    public List<ActionDefinition> Actions { get; set; } = new();

    public override IReadOnlyList<UserTaskField> GetEffectiveFields(GlobalUserTaskDefinition global)
    {
        return global.Fields.Concat(Fields).ToList();
    }
}
