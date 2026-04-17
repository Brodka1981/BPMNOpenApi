namespace BpmEngine.Api.DTO.Requests;

public class ImportBpmnRequest
{
    public string BpmnXml { get; set; } = "";
    public string? Nome { get; set; }
    public string? Versione { get; set; }
    public bool SovrascriviSeEsiste { get; set; }
}

public class ImportBpmnFileRequest
{
    public IFormFile File { get; set; } = null!;
    public string? Nome { get; set; }
    public string? Versione { get; set; }
}