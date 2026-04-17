namespace BpmDomain.Models;

public abstract class UserTaskDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";

    public List<UserTaskField> Fields { get; set; } = new();

    public virtual IReadOnlyList<UserTaskField> GetEffectiveFields(GlobalUserTaskDefinition global)
    {
        return Fields;
    }
}