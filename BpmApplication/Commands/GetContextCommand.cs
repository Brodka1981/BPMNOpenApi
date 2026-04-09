using BpmApplication.Commands.Interfaces;

namespace BpmApplication.Commands;

public class GetContextCommand : ICommand
{
    public long ProcessInstanceId { get; set; }
    public string? User { get; set; }
    public string? Company { get; set; }
}
