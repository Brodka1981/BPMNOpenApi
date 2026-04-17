using BpmApplication.Commands.Interfaces;

namespace BpmApplication.Commands;

public class GetDefinitionsCommand : ICommand
{
    public string User { get; set; }
    public string Company { get; set; }
    public string? Category { get; set; }
}