namespace BpmDomain.Models;
public sealed class GatewayDefinition
{
    public string Id { get; }
    public string Label { get; }
    public List<GatewayOutgoing> Outgoing { get; }

    public GatewayDefinition(string id, string label, List<GatewayOutgoing> outgoing)
    {
        Id = id;
        Label = label;
        Outgoing = outgoing;
    }
}

public sealed class GatewayOutgoing
{
    public string Condition { get; }
    public string TargetNodeId { get; }

    public GatewayOutgoing(string condition, string targetNodeId)
    {
        Condition = condition;
        TargetNodeId = targetNodeId;
    }
}
