using BpmApplication.Commands;

namespace BpmEngine.Domain;

public class SetVariabileCommand : ICommand
{
    public int IdProcesso { get; set; }
    public string Nome { get; set; } = "";
    public object Valore { get; set; } = null!;
    public string? Tipo { get; set; }
}

public class SetMultipleVariabiliCommand : ICommand
{
    public int IdProcesso { get; set; }
    public Dictionary<string, object> Variabili { get; set; } = new();
}

public class UpdateVariabileCommand : ICommand
{
    public int IdProcesso { get; set; }
    public string Nome { get; set; } = "";
    public object Valore { get; set; } = null!;
}

public class DeleteVariabileCommand : ICommand
{
    public int IdProcesso { get; set; }
    public string Nome { get; set; } = "";
}

public class SaveDatiFormCommand : ICommand
{
    public int IdProcesso { get; set; }
    public Dictionary<string, object> DatiForm { get; set; } = new();
    public DateTime DataSalvataggio { get; set; }
}
