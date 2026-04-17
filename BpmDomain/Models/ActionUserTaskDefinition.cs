namespace BpmDomain.Models;

public sealed class ActionUserTaskDefinition : UserTaskDefinition
{
    public ActionDefinition? Confirm { get; set; }
    public ActionDefinition? Cancel { get; set; }

    public override IReadOnlyList<UserTaskField> GetEffectiveFields(GlobalUserTaskDefinition global)
    {
        return Fields; // niente global
    }
}