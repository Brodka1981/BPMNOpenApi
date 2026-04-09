using BpmApplication.Commands.Interfaces;

namespace BpmApplication.Services;

public record ExecuteActionCommand : ICommand
{
    public Guid ProcessId { get; init; }
    public string ActionName { get; init; } = "";
    public string User { get; init; } = "";
}

