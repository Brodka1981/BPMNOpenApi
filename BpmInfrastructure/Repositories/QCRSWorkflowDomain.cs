using BpmApplication.Command;

namespace BpmEngine.CQRS.DTO.Requests;

public class CreateWorkflowCommand : ICommand
{
    public int IdWorkflow { get; set; }  // Popolato dopo insert
    public string Nome { get; set; } = "";
    public string Versione { get; set; } = "";
    public string? BpmnXml { get; set; }
    public List<StatoDefinitionDto> Stati { get; set; } = new();
}

public class UpdateWorkflowCommand : ICommand
{
    public int IdWorkflow { get; set; }
    public string Nome { get; set; } = "";
    public string Versione { get; set; } = "";
    public bool Attivo { get; set; }
}

public class DeleteWorkflowCommand : ICommand
{
    public int IdWorkflow { get; set; }
    public bool CancellazioneFisica { get; set; }
}