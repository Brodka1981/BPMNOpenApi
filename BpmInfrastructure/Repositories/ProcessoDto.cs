namespace BpmCQRS.Domain;

public class ProcessoDto
{
    public int IdProcesso { get; set; }
    public string Oggetto { get; set; } = "";
    public int IdWorkflow { get; set; }
    public int IdStatoCorrente { get; set; }
    public int Priorita { get; set; }
    public DateTime DataInizio { get; set; }
    public DateTime DataFine { get; set; }
    public bool Chiuso { get; set; }
    public DateTime DataScadenza { get; set; }
    public bool AzioneInCorso { get; set; }   
    public int IdAzioneAperturaStandard { get; set; }
}
