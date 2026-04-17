namespace BpmEngine.Api.DTO.Requests;

public class SetVariabileRequest
{
    public string Nome { get; set; } = "";
    public object Valore { get; set; } = null!;
    public string? Tipo { get; set; } // "string", "int", "bool", "date", "json"
}

public class UpdateVariabileRequest
{
    public object Valore { get; set; } = null!;
}

public class SetMultipleVariabiliRequest
{
    public Dictionary<string, object> Variabili { get; set; } = new();
}