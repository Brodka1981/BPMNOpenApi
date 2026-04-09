namespace BpmDomain.Models;

public sealed class GlobalUserTaskDefinition
{
    public IReadOnlyList<UserTaskField> Fields { get; }
    public IReadOnlyList<ActionDefinition> Actions { get; }

    public GlobalUserTaskDefinition(
        IReadOnlyList<UserTaskField> fields,
        IReadOnlyList<ActionDefinition> actions)
    {
        Fields = fields;
        Actions = actions;
    }

    public GlobalUserTaskDefinition() : this(
        Array.Empty<UserTaskField>(),
        Array.Empty<ActionDefinition>())
    {
    }
}
