
using System.Data.Common;

namespace BpmCQRS.Domain;
public static class ProcessoMapper
{
    public static ProcessoDto Map(DbDataReader r)
    {
        return new ProcessoDto
        {
            IdProcesso = r.GetInt32(0),
            Oggetto = r.IsDBNull(1) ? "" : r.GetString(1),
            IdWorkflow = r.GetInt32(2),
            IdStatoCorrente = r.GetInt32(3),
            Priorita = r.GetInt32(4),
            DataInizio = r.GetDateTime(5),
            DataFine = r.GetDateTime(6),
            Chiuso = r.GetBoolean(7),
            DataScadenza = r.GetDateTime(8),
            AzioneInCorso = r.GetBoolean(9),
            IdAzioneAperturaStandard = r.GetInt32(10)
        };
    }
}
