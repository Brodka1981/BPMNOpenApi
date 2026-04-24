using BpmApplication.Commands.Interfaces;

namespace BpmApplication.Commands;

public class StartProcessCommand : ICommand
{
    public string? ProcessType { get; set; }
    public Dictionary<string, object?>? Variables { get; set; }
    public string? User { get; set; }
    public string? Company { get; set; }
}
