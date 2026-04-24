namespace BpmEngine.Api.DTO.Requests;

public class CleanupRequest
{
    public int Giorni { get; set; } = 30;
    public bool Completi { get; set; } = true;
    public bool Falliti { get; set; } = true;
    public bool Sospesi { get; set; }
}

public class RetryFailedRequest
{
    public int IdProcesso { get; set; }
    public bool ForzaEsecuzione { get; set; }
}

public class MigrateProcessRequest
{
    public int IdProcesso { get; set; }
    public int IdWorkflowNuovaVersione { get; set; }
    public Dictionary<string, string>? MappaStati { get; set; }
}
