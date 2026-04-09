using System.Data;
using System.Reflection;
using System.Text;

namespace ZV_Engine
{
    [Serializable]
    public class ModelloStampa
    {
        public byte[] Documento;
        public string Name;
        public string Descrizione;
    }

    [Serializable]
    public class AggiornaDbEsterno
    {
        public string DBEsterno;
        public string DBConnectionString;
        public string SPName;
        public string Name;
        public List<SPParameter> lSPParameter;


    }

    public class SPParameter
    {
        public string DatiWF;
        public string SPParameterName;
        public string TipoParameter;
    }
    /// <summary>
    /// Oggetto classe Workflow
    /// </summary>
    [Serializable]
    public class ZVWorkflow : ZV_Engine.IZVEngineObject
    {
        /// <summary>
        /// Evento su chiusura di Workflow
        /// </summary>
        public event EventHandler OnClose;

        /// <summary>
        /// Definizioen della stampa di DEFAULT
        /// </summary>
        public const string STAMPA_DEFAULT = "DEFAULT";

        /// <summary>
        /// Parametro di base della [ZV_ParametriEsterni]
        /// </summary>
        public const string BASE_PARAM = "#BASEPARAM#";

        /// <summary>
        /// Definizione della fonte dei parametri esterni
        /// </summary>
        public struct FonteParEsterni
        {
            public const int Nessuna = 0;
            public const int TabellaAuto = 1;
            public const int QueryString = 2;
        };

        /// <summary>
        /// informazioni dell'utente
        /// </summary>
        public struct InfoUtenteDef
        {
            public const string TelefonoUfficio = "TelefonoUfficio";
            public const string Cellulare = "Cellulare";
            public const string Indirizzo = "Indirizzo";
            public const string CAP = "CAP";
            public const string Località = "Località";
            public const string Sesso = "Sesso";
            public const string DataNascita = "DataNascita";
            public const string LuogoNascita = "LuogoNascita";

        }
        /// <summary>
        /// Lancio evento OnClose su chiusura di oggetto
        /// </summary>
        public void Close()
        {
            OnClose?.Invoke(this, new EventArgs());
        }

        #region Metodi statici per creazione Processo (Run time)

        /// <summary>
        ///  Popola la proprietà OperazioniIsSavedRequired
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        public static List<string> GetOperazioniIsSavedRequired(ZVWorkflow workflow)
        {
            try
            {
                //eseguo solo la prima volta
                if (workflow != null)
                {
                    List<string> _operazioniIsSavedRequired = new List<string>();

                    //identifico le operazioni che necessitano del flag "IsSaved" = true
                    foreach (ZVOperazione _OP in workflow.Operazioni)
                    {
                        //se non attiva, la escludo
                        if (!_OP.Attivo)
                            continue;

                        string _myType = _OP.GetType().ToString();

                        //se è di tipo ComboBox o CheckList (se propaga o usa dati da DB)
                        if (_myType.Equals("ZV_Operazioni.ZVCampoComboBox", StringComparison.InvariantCultureIgnoreCase) ||
                             _myType.Equals("ZV_Operazioni.ZVCampoCheckList", StringComparison.InvariantCultureIgnoreCase))
                        {
                            PropertyInfo _opsUsaQueryPerCombo = _OP.GetType().GetProperty("UsaQueryPerCombo");
                            PropertyInfo _opsInfoAggiuntive = _OP.GetType().GetProperty("ListInfoAggiuntive");
                            if ((_opsInfoAggiuntive != null && ((List<string>)(_opsInfoAggiuntive.GetValue(_OP, null))).Count > 0) ||
                                (_opsUsaQueryPerCombo != null && (bool)(_opsUsaQueryPerCombo.GetValue(_OP, null))))
                                if (!_operazioniIsSavedRequired.Contains(_OP.Name))
                                    _operazioniIsSavedRequired.Add(_OP.Name);

                        }

                        //se è di tipo PopolaCampiDaQuery
                        if (_myType.Equals("ZV_Operazioni.ZVPopolaCampi", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (!_operazioniIsSavedRequired.Contains(_OP.Name))
                                _operazioniIsSavedRequired.Add(_OP.Name);

                            //ne ricavo anche le operazioni che dipendono da PopolaCampiDaQuery
                            Dictionary<string, List<string>> ListaParametriAssociati = (Dictionary<string, List<string>>)(_OP.GetType().GetProperty("ListaParametriAssociati").GetValue(_OP, null));
                            foreach (KeyValuePair<string, List<string>> _kvp in ListaParametriAssociati)
                                foreach (string _valPiped in _kvp.Value)
                                {
                                    string _val = _valPiped.Split('|')[0];
                                    if (!_operazioniIsSavedRequired.Contains(_val))
                                        _operazioniIsSavedRequired.Add(_val);
                                }
                        }

                        //se hanno una dipendenza (a prescindere...)
                        PropertyInfo _opsInizializzazione = _OP.GetType().GetProperty("OperazioniPerInizializzazione");
                        if (_opsInizializzazione != null && ((List<string>)(_opsInizializzazione.GetValue(_OP, null))).Count > 0)
                            if (!_operazioniIsSavedRequired.Contains(_OP.Name))
                                _operazioniIsSavedRequired.Add(_OP.Name);


                    }//- end for 

                    return _operazioniIsSavedRequired;
                }
                return null;
            }
            catch (Exception) { return null; }
        }

        /// <summary>
        /// recupero i template di stampa del workflow
        /// </summary>
        /// <param name="abi">Abi della banca da trattare.</param>
        /// <param name="procedura">Procedura da trattare.</param>
        /// <param name="titolo">Titolo del workflow da trattare.</param>
        /// <returns>boolean</returns>
        public static List<ModelloStampa> GetTemplateWord(string abi, string procedura, string titolo)
        {
            int id = ZVDataLayer.DefWorkflowIdDaTitolo(abi, procedura, titolo, false);
            ZVWorkflow wrk = ApriWorkflowTemplate(abi, procedura, id, true);

            if (wrk == null)
                return null;
            else
                return wrk.ListaModelloStampe;
        }

        /// <summary>
        /// Ritorna le informazioni utili per schedulare un workflow automatico
        /// </summary>
        /// <param name="titolo">Titolo del workflow da trattare.</param>
        /// <returns>Informazioni utili per schedulare un workflow automatico.</returns>
        public static ZVInfoWorkflowAutomatici GetInfoPerWfAutomatici(string titolo)
        {
            return GetInfoPerWfAutomatici(Abi, Procedura, titolo);
        }

        /// <summary>
        /// Ritorna le informazioni utili per schedulare un workflow automatico
        /// </summary>
        /// <param name="abi">Abi della banca da trattare.</param>
        /// <param name="procedura">Procedura da trattare.</param>
        /// <param name="titolo">Titolo del workflow da trattare.</param>
        /// <returns>Informazioni utili per schedulare un workflow automatico.</returns>
        public static ZVInfoWorkflowAutomatici GetInfoPerWfAutomatici(string abi, string procedura, string titolo)
        {
            ZVInfoWorkflowAutomatici infoWorkflowAutomatici = new ZVInfoWorkflowAutomatici();
            //int id = ZVDataLayer.DefWorkflowIdDaTitolo(titolo, false);
            int id = ZVDataLayer.DefWorkflowIdDaTitolo(abi, procedura, titolo, false);
            //ZVWorkflow wrk = ApriWorkflowTemplate(id, true);
            ZVWorkflow wrk = ApriWorkflowTemplate(abi, procedura, id, true);

            if (wrk == null)
            {
                infoWorkflowAutomatici.Disponibile = false;
            }
            else
            {
                infoWorkflowAutomatici.Disponibile = true;
                infoWorkflowAutomatici.AzioniEsegubili = GetListaAzioniEseguibiliPerWfAutomatici(wrk);
                infoWorkflowAutomatici.OperazioniInizializzabili = GetListaOperazioniInizializzabiliPerWf(wrk);
            }
            return infoWorkflowAutomatici;
        }

        /// <summary>
        /// Ritorna la lista delle operazioni inizializzabili dall'esterno per il workflow specificato.
        /// </summary>
        /// <param name="titolo">Titolo del workflow da trattare.</param>
        /// <returns>Lista delle operazioni inizializzabili dall'esterno per il workflow specificato.</returns>
        public static List<ZVOperazioneInizializzabile> GetListaOperazioniInizializzabiliPerWf(string titolo)
        {
            return GetListaOperazioniInizializzabiliPerWf(Abi, Procedura, titolo);
        }

        /// <summary>
        /// Ritorna la lista delle operazioni inizializzabili dall'esterno per il workflow specificato.
        /// </summary>
        /// <param name="abi">Abi della banca da trattare.</param>
        /// <param name="procedura">Procedura da trattare.</param>
        /// <param name="titolo">Titolo del workflow da trattare.</param>
        /// <returns>Lista delle operazioni inizializzabili dall'esterno per il workflow specificato.</returns>
        public static List<ZVOperazioneInizializzabile> GetListaOperazioniInizializzabiliPerWf(string abi, string procedura, string titolo)
        {
            //int id = ZVDataLayer.DefWorkflowIdDaTitolo(titolo, false);
            int id = ZVDataLayer.DefWorkflowIdDaTitolo(abi, procedura, titolo, false);
            //ZVWorkflow wrk = ApriWorkflowTemplate(id, true);
            ZVWorkflow wrk = ApriWorkflowTemplate(abi, procedura, id, true);
            return GetListaOperazioniInizializzabiliPerWf(wrk);
        }

        /// <summary>
        ///  Ritorna la lista delle operazioni inizializzabili dall'esterno per il contesto dell'operazione specificata
        /// </summary>
        /// <param name="operazione"></param>
        /// <returns></returns>
        public static List<ZVOperazioneInizializzabile> GetListaOperazioniInizializzabiliContestoOp(ZVOperazione operazione)
        {
            if (operazione.Workflow == null)
                return null;

            List<ZVOperazioneInizializzabile> listaOperazioni = new List<ZVOperazioneInizializzabile>();

            foreach (ZVOperazione op in operazione.Workflow.Operazioni)
            {
                if (op.Stato.Equals(operazione.Stato) && op.Azione.Equals(operazione.Azione) && op.Attivo)
                {
                    ZVOperazioneInizializzabile opInizializzabile = new ZV_Engine.ZVWorkflow.ZVOperazioneInizializzabile(op);
                    if (opInizializzabile.Parametri.Count > 0)
                    {
                        listaOperazioni.Add(opInizializzabile);
                    }
                }
            }
            return listaOperazioni;
        }

        /// <summary>
        /// Ritorna la lista delle operazioni inizializzabili dall'esterno per il workflow specificato.
        /// </summary>
        /// <param name="wrk">Workflow da trattare.</param>
        /// <returns>Lista delle operazioni inizializzabili dall'esterno per il workflow specificato.</returns>
        public static List<ZVOperazioneInizializzabile> GetListaOperazioniInizializzabiliPerWf(ZVWorkflow wrk)
        {
            if (wrk == null)
                return null;

            List<ZVOperazioneInizializzabile> listaOperazioni = new List<ZVOperazioneInizializzabile>();

            foreach (ZVOperazione op in wrk.AzioneAperturaStandard.OperazioniAttive)
            {
                // AB - 20190321: Su richiesta di Sudano, rimossa operazione ZVIntestazioneRichiesta da quelle inizializzabili
                if (op.GetType().Name.Equals("ZVIntestazioneRichiesta"))
                    continue;
                ZVOperazioneInizializzabile opInizializzabile = new ZV_Engine.ZVWorkflow.ZVOperazioneInizializzabile(op);
                if (opInizializzabile.Parametri.Count > 0)
                {
                    listaOperazioni.Add(opInizializzabile);
                }
            }
            return listaOperazioni;
        }

        /// <summary>
        /// Ritorna l'elenco delle azioni eseguibili automaticamente per il workflow specificato.
        /// </summary>
        /// <param name="titolo">Titolo del workflow da trattare.</param>
        /// <returns>Elenco delle azioni eseguibili automaticamente per il workflow specificato.</returns>
        public static List<ZVAzioniEseguibili> GetListaAzioniEseguibiliPerWfAutomatici(string titolo)
        {
            return GetListaAzioniEseguibiliPerWfAutomatici(Abi, Procedura, titolo);
        }

        /// <summary>
        /// Ritorna l'elenco delle azioni eseguibili automaticamente per il workflow specificato.
        /// </summary>
        /// <param name="abi">Abi della banca da trattare.</param>
        /// <param name="procedura">Procedura da trattare.</param>
        /// <param name="titolo">Titolo del workflow da trattare.</param>
        /// <returns>Elenco delle azioni eseguibili automaticamente per il workflow specificato.</returns>
        public static List<ZVAzioniEseguibili> GetListaAzioniEseguibiliPerWfAutomatici(string abi, string procedura, string titolo)
        {
            //int id = ZVDataLayer.DefWorkflowIdDaTitolo(titolo, false);
            int id = ZVDataLayer.DefWorkflowIdDaTitolo(abi, procedura, titolo, false);
            //ZVWorkflow wrk = ApriWorkflowTemplate(id, true);
            ZVWorkflow wrk = ApriWorkflowTemplate(abi, procedura, id, true);
            return GetListaAzioniEseguibiliPerWfAutomatici(wrk);
        }

        /// <summary>
        /// Ritorna l'elenco delle azioni eseguibili automaticamente per il workflow specificato.
        /// </summary>
        /// <param name="wrk">Workflow da trattare.</param>
        /// <returns>Elenco delle azioni eseguibili automaticamente per il workflow specificato.</returns>
        public static List<ZVAzioniEseguibili> GetListaAzioniEseguibiliPerWfAutomatici(ZVWorkflow wrk)
        {
            List<ZVAzioniEseguibili> listaAzioni = new List<ZVAzioniEseguibili>();
            foreach (ZVStato stato in wrk.stati)
            {
                if (stato.Iniziale)
                {
                    // Aggiungo l'azione "Salva"
                    listaAzioni.Add(new ZV_Engine.ZVWorkflow.ZVAzioniEseguibili(ZVAzione.Salva, ZVAzione.Salva));

                    foreach (ZVAzione azione in stato.Azioni)
                    {
                        if (azione.Attivo && !azione.IsAzioneStandard && !azione.Nascosta)
                        {
                            listaAzioni.Add(new ZV_Engine.ZVWorkflow.ZVAzioniEseguibili(azione));
                        }
                    }

                    return listaAzioni;
                }
            }
            return listaAzioni;
        }

        public static ZVWorkflow NuovoProcesso(string titolo, string s_CodiceSocieta, string s_Societa, object o_Contesto)
        {
            int id = ZVDataLayer.DefWorkflowIdDaTitolo(titolo, false);
            return NuovoProcesso(id, s_CodiceSocieta, s_Societa, o_Contesto);
        }

        // Usato per creare un nuovo processo: attivazione dal menu passare il riferimento del workflow di controllo
        public static ZVWorkflow NuovoProcesso(int i_idWorkflow, string s_CodiceSocieta, string s_Societa, object o_Contesto)
        {
            ZVWorkflow wrk = ApriWorkflowTemplate(i_idWorkflow);
            wrk.Contesto = o_Contesto;
            wrk.CodiceSocieta = s_CodiceSocieta;
            wrk.Societa = s_Societa;
            wrk.Nuovo = true;
            wrk.statoCorrente = wrk.StatoIniziale.GetCopia();
            wrk.statoCorrente.DataInizio = DateTime.Now;
            wrk.DataInizio = wrk.statoCorrente.DataInizio;
            wrk.idProcesso = ZVDataLayer.ProcessiAggiorna(wrk.GetDataSet(false, true));
            //foreach (ZVOperazione ope in wrk.Operazioni)  // non sembra serva
            //    ope.NuovoProcesso();
            wrk.CambiaUsaOperazione();
            wrk.infoOperazioniWorkflow = null;
            return wrk;
        }

        // AB - 20161220: Crea un nuovo processo selezionando l'ultima versione pubblicabile relativa al titolo specificato
        public static ZVWorkflow NuovoProcessoPubblicabile(string titolo, string s_CodiceSocieta, string s_Societa, object o_Contesto)
        {
            int id = ZVDataLayer.DefWorkflowUltimoIdPubblicabileDaTitolo(titolo);
            if (id == -1)
                return null;

            return NuovoProcesso(id, s_CodiceSocieta, s_Societa, o_Contesto);
        }


        // AB - 20151016: Crea un nuovo processo selezionando l'ultima versione pubblicabile relativa all'id specificato
        public static ZVWorkflow NuovoProcessoPubblicabile(int p_idProcesso, string s_CodiceSocieta, string s_Societa, object o_Contesto, string sessionID = "")
        {
            ZVDefWorkflowDS prcds = ZVDataLayer.DefWorkflowLeggi(p_idProcesso);
            if (prcds.ZV_DefWorkflow.Rows.Count != 1 ||
                (prcds.ZV_DefWorkflow.Rows[0] as ZVDefWorkflowDS.ZV_DefWorkflowRow).Template)
                return null;

            // AB - 20161220: completo l'operazione con il medoto già esistente
            /*
            int id = ZVDataLayer.DefWorkflowUltimoIdPubblicabileDaTitolo((prcds.ZV_DefWorkflow.Rows[0] as ZVDefWorkflowDS.ZV_DefWorkflowRow).Titolo, sessionID);
            if (id == -1)
                return null;
            return NuovoProcesso(id, s_CodiceSocieta, s_Societa, o_Contesto);
            */
            return NuovoProcessoPubblicabile((prcds.ZV_DefWorkflow.Rows[0] as ZVDefWorkflowDS.ZV_DefWorkflowRow).Titolo, s_CodiceSocieta, s_Societa, o_Contesto);


        }

        /// <summary>
        /// Metodo per allineare la tabella ZV_RichiesteLink con l'effettivo id del processo
        /// </summary>
        /// <param name="idProcesso">id Processo del workflow</param>
        /// <param name="idTemp">id temporaneo da sostituire</param>
        /// <param name="abi"></param>
        /// <returns></returns>


        /// <summary>
        /// Apre un processo già esistente.
        /// </summary>
        /// <param name="p_idProcesso">ID del processo da trattare.</param>
        /// <param name="s_CodiceSocieta">Codice dell'organizzazione da trattare.</param>
        /// <param name="s_Societa">Descrizione dell'organizzazione da trattare.</param>
        /// <param name="o_Contesto">Contesto di utilizzo.</param>
        /// <returns>Processo aperto.</returns>
        public static ZVWorkflow ApriProcesso(int p_idProcesso, string s_CodiceSocieta, string s_Societa, object o_Contesto)
        {
            ZVProcessiDS prcds = ZVDataLayer.ProcessiLeggi(p_idProcesso);
            if (prcds.ZV_Processi.Rows.Count != 1)
                return null;
            ZVProcessiDS.ZV_ProcessiRow row = prcds.ZV_Processi.Rows[0] as ZVProcessiDS.ZV_ProcessiRow;
            ZVWorkflow wrk = ApriWorkflowTemplate(row.IdWorkflow);
            wrk.CodiceSocieta = s_CodiceSocieta;
            wrk.Societa = s_Societa;
            wrk.Contesto = o_Contesto;
            wrk.IdWorkflow = row.IdWorkflow;
            wrk.IdProcesso = row.IdProcesso;
            if (!row.IsOggettoNull())
                wrk.Oggetto = row.Oggetto;
            wrk.Priorita = row.Priorita;
            wrk.Chiuso = row.Chiuso;
            if (!row.IsOggettoNull())
                wrk.Oggetto = row.Oggetto;
            if (!row.IsDataInizioNull())
                wrk.DataInizio = row.DataInizio;
            if (!row.IsDataFineNull())                     // date non sempre presenti sul DB
                wrk.DataFine = row.DataFine;
            if (!row.IsDataScadenzaNull())                    // date non sempre presenti sul DB 
                wrk.DataScadenza = row.DataScadenza;
            wrk.CambiaUsaOperazione();

            // AB - 20191018: Spostata prima per evitare che la query di popolamento di infoOperazioniWorkflow venga eseguita 2 volte
            wrk.infoOperazioniWorkflow = null;

            // Leggi stato corrente con idstato corrente 
            ZVStatiDS stads = ZVDataLayer.StatiLeggi(row.IdStatoCorrente);
            if (stads.ZV_Stati.Rows.Count == 1)
            {
                ZVStatiDS.ZV_StatiRow rowst = stads.ZV_Stati.Rows[0] as ZVStatiDS.ZV_StatiRow;
                wrk.NomeStatoCorrente = rowst.NomeStato; // imposta oggetto nel Workflow
                wrk.StatoCorrente.IdStato = rowst.IdStato;
                wrk.StatoCorrente.NomeStatoPrec = rowst.NomeStatoPrec;
                wrk.StatoCorrente.DataInizio = rowst.DataInizio;
                if (!rowst.IsDataScadenzaNull())                    // date non sempre presenti sul DB
                    wrk.StatoCorrente.DataScadenza = rowst.DataScadenza;
                if (!rowst.IsDataFineNull())                     // date non sempre presenti sul DB
                    wrk.StatoCorrente.DataFine = rowst.DataFine;
            }

            //LeggeIdTemporaneo
            ZVRichiesteLinkDS richds = ZVDataLayer.RichiesteLinkLeggi(wrk.IdProcesso);
            if (richds.ZV_RichiesteLink.Rows.Count > 0)
            {
                ZVRichiesteLinkDS.ZV_RichiesteLinkRow rowRich = richds.ZV_RichiesteLink.Rows[0] as ZVRichiesteLinkDS.ZV_RichiesteLinkRow;
                wrk.IdTemporaneo = (rowRich.IdTemp == null ? string.Empty : rowRich.IdTemp.ToString());
            }

            // AB - 20191018: Spostata prima per evitare che la query di popolamento di infoOperazioniWorkflow venga eseguita 2 volte
            //wrk.infoOperazioniWorkflow = null;

            return wrk;
        }


        /// <summary>
        /// Ritorna il valore della proprietà specificata dell'azione indicata.
        /// </summary>
        /// <param name="nomeAzione">Azione da trattare.</param>
        /// <param name="nomeProperty">Proprietà da trattare.</param>
        /// <returns>Valore della proprietà specificata dell'azione indicata.</returns>
        public object GetAzioneProperty(string nomeAzione, string nomeProperty)
        {
            ZVAzione azione = StatoCorrente.AzioneDaNome(nomeAzione);
            if (azione != null)
                foreach (ZVOperazione oper in azione.Operazioni)
                {
                    PropertyInfo prop = oper.GetType().GetProperty(nomeProperty, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                        return prop.GetValue(oper, null);
                }
            return null;
        }

        /// <summary>
        /// Imposta il valore della proprietà specificata per l'azione indicate ed eventualmente esegue l'azione stessa.
        /// </summary>
        /// <param name="nomeAzione">Azione da trattare.</param>
        /// <param name="nomeProperty">Proprietà da trattare.</param>
        /// <param name="value">Valore da impostare.</param>
        /// <param name="eseguiAzione">Impostare a true se deve essere eseguita anche l'azione, false altrimenti.</param>
        /// <returns>True se l'operazione è andata a buon fine, false altrimenti.</returns>
        public bool SetAzioneProperty(string nomeAzione, string nomeProperty, object value, bool eseguiAzione)
        {
            ZVAzione azione = StatoCorrente.AzioneDaNome(nomeAzione);
            if (azione != null)
                foreach (ZVOperazione oper in azione.Operazioni)
                {
                    PropertyInfo prop = oper.GetType().GetProperty(nomeProperty, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        prop.SetValue(oper, value, null);
                        if (eseguiAzione)
                            azione.AttivaEsegui();  //esegue azione e collegata.
                        return true;
                    }
                }
            return false;
        }



        #endregion

        #region Proprieta e metodi d'uso a Run Time (processo)

        /// <summary>
        /// Nome della stato corrente.
        /// </summary>
        public string NomeStatoCorrente
        {
            get { return statoCorrente.Name; }
            // usato per passare ad un nuovo stato il set provoca il cambio 
            // dello stato corrente
            set
            {
                foreach (ZVStato stato in Stati)
                    if (stato.Name.TrimEnd().Equals(value.TrimEnd(), StringComparison.InvariantCultureIgnoreCase))  // Ricerca stato con il nome impostato
                    {
                        ZVStato copia = stato.GetCopia();
                        if (StatoCorrente != null && NomeStatoCorrente == stato.Name)
                        {
                            copia.Workflow = this;
                            copia.IdStato = statoCorrente.IdStato;
                            copia.Nuovo = StatoCorrente.Nuovo;
                        }
                        statoCorrente = copia;
                        statoCorrente.InitStato();
                        break;
                    }
            }
        }

        /// <summary>
        /// Ritorna true se l'utente corrente è un creatore del workflow, false altrimenti.
        /// </summary>
        public bool Creatore
        {
            get
            {
                if (OperazioneCompetenze == null)
                    return true;
                return OperazioneCompetenze.Eseguibile(ContestoUtilizzo.Esegui);
            }
        }

        bool nuovo;  // Impostato a nuovo se inserimento

        /// <summary>
        /// Assume true se l processo è nuovo, false altrimenti.
        /// </summary>
        public bool Nuovo
        {
            get { return nuovo || idProcesso < 0; }
            set { nuovo = value; }
        }

        // AB - 20170911: ripristino dello stato precedende all'azione in caso di errore
        /// <summary>
        /// Elimina il processo se nuovo, altrimenti lo riporta allo stato precedente specificato.
        /// </summary>
        /// <param name="idStatoPrecedente">Id dello stato precedente a cui eventualmente riportare il processo.</param>
        public void EliminaSeNuovo(int idStatoPrecedente)
        {
            //ZVDataLayer.ProcessiAggiorna(GetDataSet(nuovo, false));
            ZVDataLayer.ProcessiAggiorna(GetDataSet(nuovo, false, idStatoPrecedente));
        }

        /// <summary>
        /// Elimina il processo se nuovo, altrimenti lo riporta allo stato precedente specificato. 
        /// Questo overload considera 
        /// </summary>
        /// <param name="_idStatoPrecedente"></param>
        /// <param name="_chiuso"></param>
        /// <param name="_dataFine"></param>
        public void EliminaSeNuovo(int _idStatoPrecedente, bool? _chiuso, DateTime? _dataFine)
        {
            using (ZVProcessiDS ZVProcessiDS = new ZVProcessiDS())
            {
                ZVProcessiDS.ZV_ProcessiRow row = ZVProcessiDS.ZV_Processi.NewZV_ProcessiRow();
                row.IdProcesso = IdProcesso;
                row.IdWorkflow = IdWorkflow;

                if (nuovo || _idStatoPrecedente == -1)
                    row.IdStatoCorrente = StatoCorrente.IdStato;
                else
                    row.IdStatoCorrente = _idStatoPrecedente;

                row.Oggetto = Oggetto;
                row.Priorita = Priorita;
                row.DataInizio = DataInizio;
                row.DataScadenza = DataScadenza;
                row.AzioneInCorso = false;

                if (_chiuso.HasValue)
                    row.Chiuso = _chiuso.Value;

                if (_dataFine.HasValue)
                    row.DataFine = _dataFine.Value;

                row.IdAzioneAperturaStandard = AzioneAperturaStandard.VirtualKey;
                ZVProcessiDS.ZV_Processi.AddZV_ProcessiRow(row);
                if (nuovo)
                {
                    row.AcceptChanges();
                    row.Delete();
                }
                else if (IdProcesso >= 0)
                {
                    row.AcceptChanges();
                    row.SetModified();
                }
                ZVDataLayer.ProcessiAggiorna(ZVProcessiDS);
            }
        }

        // AB - 20170911: ripristino dello stato precedende all'azione in caso di errore
        /// <summary>
        /// Aggiorna il DataSet dei processi.
        /// </summary>
        /// <param name="elimina">Impostare a true se si vuole eliminare lo stato processo corrente.</param>
        /// <param name="azioneInCorso">Impostare a true se c'è un'azione in corso, false altrimenti.</param>
        /// <param name="idStatoPrecedente">
        /// ID dell'eventuale stato precedente da estrarre. 
        /// Impostare a -1 se non si vuole estrarre alcun stato precedente.
        /// </param>
        /// <returns>DataSet dei processi aggiornato.</returns>
        public ZVProcessiDS GetDataSet(bool elimina, bool azioneInCorso, int idStatoPrecedente = -1)
        {
            // Inserisci in una row le propriet� del processo, e tutti gli altri DataSet associati.
            ZVProcessiDS ZVProcessiDS = new ZVProcessiDS();
            ZVProcessiDS.ZV_ProcessiRow row = ZVProcessiDS.ZV_Processi.NewZV_ProcessiRow();
            row.IdProcesso = IdProcesso;
            row.IdWorkflow = IdWorkflow;

            // AB - 20170911: ripristino dello stato precedende all'azione in caso di errore
            if (elimina || idStatoPrecedente == -1)
                row.IdStatoCorrente = StatoCorrente.IdStato;
            else
                row.IdStatoCorrente = idStatoPrecedente;

            row.Oggetto = Oggetto;
            row.Priorita = Priorita;
            row.DataInizio = DataInizio;
            row.DataFine = DataFine;
            row.DataScadenza = DataScadenza;
            row.Chiuso = Chiuso;
            row.AzioneInCorso = azioneInCorso;
            row.IdAzioneAperturaStandard = AzioneAperturaStandard.VirtualKey;
            ZVProcessiDS.ZV_Processi.AddZV_ProcessiRow(row);
            if (elimina)
            {
                row.AcceptChanges();
                row.Delete();
            }
            else if (IdProcesso >= 0)
            {
                row.AcceptChanges();
                row.SetModified();
            }
            return ZVProcessiDS;
        }

        /// <summary>
        /// Salva in DB l'errore specificato.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <param name="errore">Descrizione breve dell'errore.</param>
        /// <param name="message">Dettaglio dell'errore.</param>
        public void Errore(string contesto, string errore, string message)
        {
            ZVErroriDS erroriDs = new ZVErroriDS();
            ZVErroriDS.ZV_ErroriRow row = erroriDs.ZV_Errori.NewZV_ErroriRow();
            row.IdProcesso = IdProcesso;
            row.Contesto = contesto;
            row.Errore = errore;
            row.ExceptionMessage = message;
            row.Utente = ((IZVCompetenzeWorkflow)OperazioneCompetenze).CodiceUtente;
            erroriDs.ZV_Errori.AddZV_ErroriRow(row);
            ZVDataLayer.ErroriAggiorna(erroriDs);
        }

        /// <summary>
        /// Ritorna il DataSet delle azioni di storico.
        /// </summary>
        /// <returns>DataSet delle azioni di storico.</returns>
        public ZVAzioniAllDS AzioniStorico()
        {
            return ZVDataLayer.ElencoAzioniStorico(IdProcesso);
        }

        /// <summary>
        /// Ritorna il DataSet delle azioni di riepilogo.
        /// </summary>
        /// <returns>DataSet delle azioni di riepilogo.</returns>
        public ZVAzioniAllDS AzioniRiepilogo()
        {
            ZVAzioniAllDS azr = ZVDataLayer.ElencoAzioniRiepilogo(this.IdProcesso);
            if (azr != null)
            {
                ZVAzioniAllDS.ZV_AzioniAllRow rowDaEliminare = null;
                foreach (ZVAzioniAllDS.ZV_AzioniAllRow row in azr.ZV_AzioniAll.Rows)
                {

                    if (this.StatoCorrente != null && this.StatoCorrente.AzioneApertura != null &&  // toglie azione già visualizzata dallo stato
                      row.NomeStato == this.StatoCorrente.Name &&
                      row.NomeAzione == this.StatoCorrente.AzioneApertura.Name)
                        rowDaEliminare = row;
                }
                if (rowDaEliminare != null)
                    azr.ZV_AzioniAll.Rows.Remove(rowDaEliminare);
                return azr;
            }
            return null;
        }

        ZVAzione azioneIncorso = null;

        // AB - 20180606: aggiunta possibilità di specificare il tipo di eseguibilità in caso di azione di riepilogo.
        /// <summary>
        /// Ritorna l'azione del wf in base al suo id.
        /// </summary>
        /// <param name="idAzione">Id dell'azione da cercare.</param>
        /// <param name="azioneRiepilogo">Eventuale tipo di eseguibilità in caso di azione di riepilogo</param>
        /// <returns></returns>
        //public ZVAzione AzioneDaId(int idAzione)
        public ZVAzione AzioneDaId(int idAzione, ZVAzione.enumAzioneRiepilogo azioneRiepilogo = ZVAzione.enumAzioneRiepilogo.Nessuno)
        {
            if (azioneIncorso != null && azioneIncorso.IdAzione == idAzione)
                return azioneIncorso;
            azioneIncorso = null;
            ZVAzioniAllDS az = ZVDataLayer.AzioniAllLeggi(idAzione);
            foreach (ZVAzioniAllDS.ZV_AzioniAllRow row in az.ZV_AzioniAll)
            {
                foreach (ZVStato stato in this.Stati)
                {
                    if (row.NomeStato == stato.Name)
                    {
                        foreach (ZVAzione azione in stato.Azioni)
                        {
                            if (azione.Name == row.NomeAzione)
                            {
                                azioneIncorso = azione.GetCopia();

                                // AB - 20180606: imposto il tipo di eseguibilità in caso di azione di riepilogo.
                                azioneIncorso.AzioneRiepilogo = azioneRiepilogo;

                                azioneIncorso.ApriAzione(idAzione);

                                if (stato != statoCorrente)   // non ricarico i dati se � lo stato corrente
                                {
                                    stato.IdStato = row.IdStato;
                                    stato.InitStato();
                                }
                                // AB - 20171018: Inutile continuare il ciclo una volta trovata l'azione cercata
                                break;
                            }
                        }
                        // AB - 20171018: Inutile continuare il ciclo una volta trovato lo stato cercato
                        break;
                    }
                }
            }
            return azioneIncorso;
        }

        /// <summary>
        /// Aggiorna lo stato corrente con i valori da DB.
        /// </summary>
        public void RefreshStato()
        {
            azioneIncorso = null;   // annulla dati presenti per rilettura da DB
            evidenze = null;
            classificazione = null;
            scadenze = null;  // da vericare a seconda dell'uso
            StatoCorrente.Nuovo = false; // Stato
            Nuovo = false;  // Workflow
            infoOperazioniWorkflow = null;
            NomeStatoCorrente = NomeStatoCorrente;
        }

        /// <summary>
        /// Ritorna l'azione associata ai nomi dello stato de dell'azione specificati.
        /// </summary>
        /// <param name="nomeStato">Nome dello stato da trattare.</param>
        /// <param name="nomeAzione">Nome dell'azione da trattare.</param>
        /// <returns>Azione associata ai nomi dello stato de dell'azione specificati.</returns>
        public ZVAzione AzioneDaNomeStatoAzione(string nomeStato, string nomeAzione)
        {
            foreach (ZVStato stato in Stati)
                if (stato.Name == nomeStato)
                {
                    ZVStato wstato = null;
                    if (statoCorrente != null && statoCorrente.Name == nomeStato)
                        wstato = statoCorrente;
                    else
                        wstato = stato;
                    foreach (ZVAzione azione in wstato.Azioni)
                        if (azione.Name == nomeAzione)
                            return azione;
                }
            return null;
        }

        /// <summary>
        /// Ritorna lo stato associato al nome specificato.
        /// </summary>
        /// <param name="nomeStato">Nome dello stato da trattare.</param>
        /// <returns>Stato associato al nome specificato.</returns>
        public ZVStato StatoDaNome(string nomeStato)
        {
            foreach (ZVStato stato in Stati)
                if (stato.Name == nomeStato)
                {
                    return stato;

                }
            return null;
        }

        // AB - 20191106: Compilazione di tutto il c# di un workflow in un unico passaggio
        /// <summary>
        /// Compila tutto il codice c# inserito nella definizione del workflow.
        /// </summary>
        public void Compila()
        {
            // Chiave per salvataggio in Session del compilato in caso di esecuzione web
            string codiceCompilatoSessionKey = new StringBuilder("ZVCompiler").Append(IdProcesso).Append("ListOperazioni_").Append(SessionID).ToString();
            // Chiave per salvataggio in StaticCache del compilato in caso di esecuzione batch
            string codiceCompilatoStaticCache = new StringBuilder("ZVCompiler").Append("ListOperazioni_").Append(IdWorkflow).ToString();

            if (WorkflowIsWeb &&
                System.Web.HttpContext.Current != null &&
                System.Web.HttpContext.Current.Session != null &&
                !string.IsNullOrEmpty(SessionID) &&
                System.Web.HttpContext.Current.Session[codiceCompilatoSessionKey] != null)
            {
                // Esecuzione Web
                // Compilazione già presente in session!!!!
                return;
            }
            if (WorkflowIsWeb &&
                System.Web.HttpContext.Current == null &&
                Cache.ContainsKey(codiceCompilatoStaticCache))
            {
                // Esecuzione batch
                // Compilazione già presente in session!!!!
                return;
            }


            List<ZVOperazione> listOperazioni = new List<ZVOperazione>();
            List<string> listRequisiti = new List<string>();

            foreach (ZVStato stato in Stati)
            {
                // Per tutte gli stati attivi
                if (!stato.Attivo)
                    continue;

                foreach (ZVAzione azione in stato.Azioni)
                {
                    // Per tutte le azioni attive
                    if (!azione.Attivo)
                        continue;

                    foreach (ZVOperazione operazione in azione.OperazioniAttive)
                    {
                        // Per tutte le operazioni attive
                        PropertyInfo prop = operazione.GetType().GetProperty("Requisito", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        if (prop == null)
                        {
                            prop = operazione.GetType().GetProperty("CondizioneString", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                        }
                        if (prop != null)
                        {
                            string requisito = prop.GetValue(operazione, null).ToString();

                            if (!string.IsNullOrWhiteSpace(requisito))
                            {
                                listOperazioni.Add(operazione);
                                listRequisiti.Add(requisito);
                            }
                        }
                    }
                }
            }

            if (listOperazioni.Count > 0)
                ZVCompiler.Compila(IdProcesso, listRequisiti, listOperazioni, null, false, SessionID);
        }

        #endregion

        #region Proprieta di processo usate a run time

        [NonSerialized]
        private string codiceSocieta;
        /// <summary>
        /// Codice dell'organizzazione.
        /// </summary>
        public string CodiceSocieta
        {
            get
            {
                return codiceSocieta;
            }
            set { codiceSocieta = value; }
        }
        [NonSerialized]
        private string societa;
        /// <summary>
        /// Descrizione dell'organizzazione.
        /// </summary>
        public string Societa
        {
            get
            {
                return societa;
            }
            set { societa = value; }
        }

        [NonSerialized]
        private object contesto;    // Object custom esterno la workflow (Cache ecc.)
        /// <summary>
        /// Contesto di utilizzo.
        /// </summary>
        public object Contesto
        {
            get { return contesto; }
            set { contesto = value; }
        }

        /// <summary>
        /// Codice di riferimento.
        /// </summary>
        public String CodiceRiferimento
        {
            get { return (string)GetAttributo("CodiceRiferimento", typeof(string)); }
            set { SetAttributo("CodiceRiferimento", value); }
        }

        /// <summary>
        /// Competenza di riferimento.
        /// </summary>
        public String CompetenzaRiferimento
        {
            get { return (string)GetAttributo("CompetenzaRiferimento", typeof(string)); }
            set { SetAttributo("CompetenzaRiferimento", value); }
        }

        /// <summary>
        /// Info richiesta.
        /// </summary>
        public string InfoRichiesta
        {
            get { return (string)GetAttributo("InfoRichiesta", typeof(string)); }
            set { SetAttributo("InfoRichiesta", value); }
        }

        /// <summary>
        /// Descrizione riferimento.
        /// </summary>
        public String DescrizioneRiferimento
        {
            get { return (string)GetAttributo("DescrizioneRiferimento", typeof(string)); }
            set { SetAttributo("DescrizioneRiferimento", value); }
        }

        /// <summary>
        /// Email riferimento.
        /// </summary>
        public string EmailRiferimento
        {
            get { return (string)GetAttributo("EmailRiferimento", typeof(string)); }
            set { SetAttributo("EmailRiferimento", value); }
        }

        /// <summary>
        /// Email riferimento alternativa.
        /// </summary>
        public string EmailRiferimentoAlternativo
        {
            get { return (string)GetAttributo("EmailRiferimentoAlternativo", typeof(string)); }
            set { SetAttributo("EmailRiferimentoAlternativo", value); }
        }

        /// <summary>
        /// Classificazione di riferimento.
        /// </summary>
        public String ClassificazioneRiferimento
        {
            get { return (string)GetAttributo("ClassificazioneRiferimento", typeof(string)); }
            set { SetAttributo("ClassificazioneRiferimento", value); }
        }

        [NonSerialized]
        Dictionary<string, object> attributiWorkflow = null;
        [NonSerialized]
        Dictionary<string, bool> attributiWorkflowChanged = new Dictionary<string, bool>();

        /// <summary>
        /// Salva in DB gli attributi di workflow.
        /// </summary>
        public void SalvaAttributiWorkflow()
        {
            List<DataSet> list = new List<DataSet>();
            list.Add((ZVOperazioniDS)GetAttributiWorkflowDataSet.GetChanges());
            ZVProxy<ZVProcessiDS>.Proxy.Aggiorna(list);
        }

        Dictionary<string, object> AttributiWorkflow
        {
            get
            {
                if (attributiWorkflow == null)
                {
                    attributiWorkflow = new Dictionary<string, object>();
                }
                return attributiWorkflow;
            }
        }

        // AB - 20191021: Aggiunta variabile alla proprietà proprietà per evitare di ricalcolare ogni volta il dataset degli attributi.
        //                Se è necessario forzane il ricalcolo, impostare il valore della proprietà a null.
        //[NonSerialized]
        //ZVOperazioniDS attributiWorkflowDataSet = null;
        /// <summary>
        /// Ritorna il DatasSet gli attributi di workflow.
        /// </summary>
        public ZVOperazioniDS GetAttributiWorkflowDataSet
        {
            get
            {
                //if (attributiWorkflowDataSet == null)
                //{

                foreach (string nome in AttributiWorkflow.Keys)
                {
                    if (AttributiWorkflow[nome] == null)
                    {
                        continue;
                    }

                    //bool defaultValueOverwrite;
                    ZVOperazione.Serializza item = ZVOperazione.SerializzaAttributo(nome, AttributiWorkflow[nome], null/*, out defaultValueOverwrite*/);

                    DataRow[] rows = InfoOperazioniWorkflow.ZV_Operazioni.Select(new StringBuilder().AppendFormat("Proprieta='{0}'", nome).ToString());
                    ZVOperazioniDS.ZV_OperazioniRow row;
                    if (rows.Length > 0)
                    {
                        row = (ZVOperazioniDS.ZV_OperazioniRow)rows[0];
                        // AB - 20160607: se l'item non � valorizzato, lo rimuovo
                        if (item.Vuoto)
                        {
                            row.Delete();
                            continue;
                        }
                    }
                    else
                    {
                        // AB - 20160607: se l'item non � valorizzato, non lo aggiungo
                        if (item.Vuoto)
                        {
                            continue;
                        }

                        row = InfoOperazioniWorkflow.ZV_Operazioni.NewZV_OperazioniRow();
                        row.IdProcesso = idProcesso;
                        row.IdAzione = 0;
                        row.IdOperazione = 0;
                        row.IdStato = 0;
                        row.Attributo = item.Attributo;
                        row.Proprieta = nome;
                        InfoOperazioniWorkflow.ZV_Operazioni.AddZV_OperazioniRow(row);
                    }
                    row.ValoreA = item.ValoreA;
                    row.ValoreN = item.ValoreN;
                    row.ValoreB = item.ValoreB;
                }
                return InfoOperazioniWorkflow;
                // attributiWorkflowDataSet = InfoOperazioniWorkflow;
                //}

                //return attributiWorkflowDataSet;
            }
            //set
            //{
            //    attributiWorkflowDataSet = value;
            //}
        }

        [NonSerialized]
        ZVClientiDS clienteWorkflow;
        /// <summary>
        /// DataSet dei clienti.
        /// </summary>
        public ZVClientiDS ClienteWorkflow
        {
            get
            {
                if (clienteWorkflow == null)
                    clienteWorkflow = ZVDataLayer.ClientiLeggi(IdProcesso);
                return clienteWorkflow;
            }

        }

        //string dbEsterno;
        //public string DBEsterno
        //{
        //    get
        //    {
        //        if (dbEsterno == null)
        //            dbEsterno = string.Empty;
        //        return dbEsterno;
        //    }
        //    set
        //    {
        //        dbEsterno = value;
        //    }
        //}

        //string spNome;
        //public string SPNome
        //{
        //    get
        //    {
        //        if (spNome == null)
        //            spNome = string.Empty;
        //        return spNome;
        //    }
        //    set
        //    {
        //        spNome = value;
        //    }
        //}


        [NonSerialized]
        ZVApplicazioniExtDS applicazioniExtWorkflow;
        /// <summary>
        /// DataSet delle applicazioni esterne è valorizzato, false altrimenti.
        /// </summary>
        public ZVApplicazioniExtDS ApplicazioniExtWorkflow
        {
            get
            {
                if (applicazioniExtWorkflow == null)
                    applicazioniExtWorkflow = ZVDataLayer.ApplicazioniExtLeggi(this.Name);
                return applicazioniExtWorkflow;
            }
            set
            {
                applicazioniExtWorkflow = value;
            }
        }

        /// <summary>
        /// Assume true se il DataSet delle applicazioni esterne è valorizzato, false altrimenti.
        /// </summary>
        public bool ApplicazioniExtNotNull
        {
            get
            {
                return (applicazioniExtWorkflow != null);
            }
        }

        [NonSerialized]
        ZVOperazioniDS infoOperazioniWorkflow;
        /// <summary>
        /// DataSet dei valori delle operazioni del processo.
        /// </summary>
        public ZVOperazioniDS InfoOperazioniWorkflow
        {
            get
            {
                if (infoOperazioniWorkflow == null)
                    infoOperazioniWorkflow = ZVDataLayer.OperazioniLeggiByIdAzione(IdProcesso, 0);
                return infoOperazioniWorkflow;
            }
        }

        /// <summary>
        /// Assume true se il DataSet dei valori delle operazioni del processo è valorizzato, false altrimenti.
        /// </summary>
        public bool InfoOperazioniWorkflowNotNull
        {
            get
            {
                return infoOperazioniWorkflow != null;
            }
        }

        /// <summary>
        /// Ritorna la categoria del workflow specificato.
        /// </summary>
        /// <param name="idWorkflow">ID del workflow da trattare.</param>
        /// <returns>Categoria del workflow specificato.</returns>
        public string GetCategoria(int idWorkflow)
        {
            string _Categoria = string.Empty;

            ZVDefWorkflowDS wrkds = ZVDataLayer.DefWorkflowLeggi(idWorkflow);
            if (wrkds.ZV_DefWorkflow.Rows.Count == 1)
            {
                ZVDefWorkflowDS.ZV_DefWorkflowRow row = wrkds.ZV_DefWorkflow.Rows[0] as ZVDefWorkflowDS.ZV_DefWorkflowRow;

                _Categoria = row.Categoria;
            }
            return _Categoria;
        }

        /// <summary>
        /// Ritorna il valore dell'attributo specificato.
        /// </summary>
        /// <param name="name">Nome dell'attributo.</param>
        /// <param name="tipo">Tipo dell'attributo.</param>
        /// <returns>Valore dell'attributo specificato.</returns>
        public object GetAttributo(string name, Type tipo)
        {
            if (AttributiWorkflow.ContainsKey(name))
                return AttributiWorkflow[name];
            ZVOperazione.Serializza item = new ZVOperazione.Serializza();

            DataRow[] rows = InfoOperazioniWorkflow.ZV_Operazioni.Select(new StringBuilder().AppendFormat("Proprieta='{0}'", name).ToString());
            if (rows.Length > 0)
            {
                ZVOperazioniDS.ZV_OperazioniRow row = (ZVOperazioniDS.ZV_OperazioniRow)rows[0];
                item.ValoreA = row.ValoreA.Trim();
                //MR 20150804 ValoreB restituisce sempre eccezzione su valori nulli
                item.ValoreB = row.IsValoreBNull() ? null : row.ValoreB;
                item.ValoreN = row.ValoreN;
            }
            else
            {
                item.ValoreA = String.Empty;
                item.ValoreB = null;
                item.ValoreN = 0;
                // AB - 20160607: valorizzo flag vuoto
                item.Vuoto = true;
            }
            AttributiWorkflow[name] = ZVOperazione.DeSerializzaAttributo(item, tipo);
            return AttributiWorkflow[name];
        }

        // AB - 20180418: Aggiunto flag per evitare l'esecuzione di query inutili in fase di deserializzazione valori da Analisi WF
        /// <summary>
        /// Impostrare a true per evitare l'esecuzione di query inutili in fase di deserializzazione valori da Analisi WF, false altrimenti.
        /// </summary>
        public bool DeserializeOnly = false;

        /// <summary>
        /// Impoosta il valore dell'attributo specificato.
        /// </summary>
        /// <param name="name">Nome dell'attributo.</param>
        /// <param name="valore">Valore da impostare.</param>
        public void SetAttributo(string name, object valore)
        {
            // AB - 20180418: Aggiunto flag per evitare l'esecuzione di query inutili in fase di deserializzazione valori da Analisi WF
            if (DeserializeOnly)
                return;

            object dummy = InfoOperazioniWorkflow;
            if (!AttributiWorkflow.ContainsKey(name))
                attributiWorkflowChanged[name] = true;
            else
                if (AttributiWorkflow[name] != valore)
                attributiWorkflowChanged[name] = true;
            AttributiWorkflow[name] = valore;
        }

        // AB - 20180412: Metodi per la gestione del formato json del valore dell'operazione
        #region Metodi per la gestione del formato json del valore dell'operazione

        /// <summary>
        /// Ritorna il valore dell'attributo specificato in formato json.
        /// </summary>
        /// <param name="name">Nome dell'attributo da trattare.</param>
        /// <param name="value">Valore dell'attributo da convetire in formato json.</param>
        /// <returns>Valore dell'attributo specificato in formato json.</returns>
        public static string GetValoreJsonAttributo(string name, object value)
        {
            // Al momennto tutti gli attributi sono di tipo string
            // Se ci fossero tipi diversi, andrebbero gestiti qui con condizioni ad-hoc
            /*Switch(name)
            {
                case ...
            }*/

            //return JsonConvert.SerializeObject(value.ToString(), Newtonsoft.Json.Formatting.None);
            return value.ToString();
        }

        /// <summary>
        /// Ritorna il tipo del valore dell'attributo specificato in formato json.
        /// </summary>
        /// <param name="name">Nome dell'attributo da trattare.</param>
        /// <returns>Tipo del valore dell'attributo specificato in formato json.</returns>
        public static string GetTipoJsonAttributo(string name)
        {
            // Al momennto tutti gli attributi sono di tipo string
            // Se ci fossero tipi diversi, andrebbero gestiti qui con condizioni ad-hoc
            /*Switch(name)
            {
                case ...
            }*/
            string tipo = ZVOperazione.TipiJson.TipoString;

            return tipo;
        }

        #endregion

        // variabili gestite durante l'esecuzione di un'azione
        [NonSerialized]
        ZVOperazione operazioneInCorso = null;
        /// <summary>
        /// Operazione in corso di esecuzione.
        /// </summary>
        public ZVOperazione OperazioneInCorso
        {
            get { return operazioneInCorso; }
            set { operazioneInCorso = value; }
        }
        [NonSerialized]
        private bool cambioStatoInCorso = false;
        /// <summary>
        /// Assueme true se è in corso in cambio di stato, false altrimenti.
        /// </summary>
        public bool CambioStatoInCorso
        {
            get { return cambioStatoInCorso; }
            set { cambioStatoInCorso = value; }
        }
        [NonSerialized]
        List<ZVStato> statiInseriti;
        /// <summary>
        /// Lista degli stati inseriti.
        /// </summary>
        public List<ZVStato> StatiInseriti
        {
            get { return statiInseriti; }
            set { statiInseriti = value; }
        }
        [NonSerialized]
        List<ZVAzione> azioniInserite;
        /// <summary>
        /// Lista delle azioni inserite.
        /// </summary>
        public List<ZVAzione> AzioniInserite
        {
            get { return azioniInserite; }
            set { azioniInserite = value; }
        }
        [NonSerialized]
        List<ZVOperazione> operazioniEseguite;
        /// <summary>
        /// Lista delle operazioni eseguite.
        /// </summary>
        public List<ZVOperazione> OperazioniEseguite
        {
            get { return operazioniEseguite; }
            set { operazioniEseguite = value; }
        }

        private int idWorkflow = -1;
        /// <summary>
        /// Id del workflow.
        /// </summary>
        public int IdWorkflow
        {
            get { return idWorkflow; }
            set { idWorkflow = value; }
        }
        [NonSerialized]
        private int idProcesso = -1;
        /// <summary>
        /// Id del processo.
        /// </summary>
        public int IdProcesso
        {
            get { return idProcesso; }
            set { idProcesso = value; }
        }

        private int idTemplate = 0;
        /// <summary>
        /// Id del template.
        /// </summary>
        public int IdTemplate
        {
            get { return idTemplate; }
            set { idTemplate = value; }
        }

        [NonSerialized]
        private DateTime dataInizio = DateTime.MinValue;
        /// <summary>
        /// Data inizio processo.
        /// </summary>
        public DateTime DataInizio
        {
            get { return dataInizio; }
            set { dataInizio = value; }
        }
        [NonSerialized]
        private DateTime dataFine = DateTime.MinValue;
        /// <summary>
        /// Data fine processo.
        /// </summary>
        public DateTime DataFine
        {
            get { return dataFine; }
            set { dataFine = value; }
        }
        [NonSerialized]
        private DateTime dataScadenza = DateTime.MinValue;
        /// <summary>
        /// Data scadenza.
        /// </summary>
        public DateTime DataScadenza
        {
            get { return dataScadenza; }
            set { dataScadenza = value; }
        }
        [NonSerialized]
        private ZVStato statoCorrente;
        /// <summary>
        /// Stato corrente.
        /// </summary>
        public ZVStato StatoCorrente
        {
            get { return statoCorrente; }  // set attraverso nome stato o per load del processo
        }
        [NonSerialized]
        private bool chiuso = false;
        /// <summary>
        /// Assume true sei il processo è chiuso, false altrimenti.
        /// </summary>
        public bool Chiuso
        {
            get { return chiuso; }
            set { chiuso = value; }
        }
        [NonSerialized]
        private string idTemporaneo = string.Empty;
        /// <summary>
        /// Id temporaneo.
        /// </summary>
        public string IdTemporaneo
        {
            get { return idTemporaneo; }
            set { idTemporaneo = value; }
        }
        [NonSerialized]
        private ZVEvidenzeDS evidenze;
        /// <summary>
        /// Assume true se il dataset delle evidenze è valorizzato, false altrimenti.
        /// </summary>
        public bool EvidenzeNotNull
        {
            get
            {
                return (evidenze != null);
            }
        }
        /// <summary>
        /// DataSet delle evidenze.
        /// </summary>
        public ZVEvidenzeDS Evidenze
        {
            get
            {
                if (evidenze == null)   // leggi solo la prima volta
                    evidenze = ZVDataLayer.EvidenzeLeggi(idProcesso);
                return evidenze;
            }
        }

        [NonSerialized]
        private ZVClassificazioneDS classificazione;
        /// <summary>
        /// Assume true se il dataset delle classificazioni è valorizzato, false altrimenti.
        /// </summary>
        public bool ClassificazioneNotNull
        {
            get
            {
                return (classificazione != null);
            }
        }

        /// <summary>
        /// DataSet delle classificazioni.
        /// </summary>
        public ZVClassificazioneDS Classificazione
        {
            get
            {
                if (classificazione == null)   // leggi solo la prima volta
                    classificazione = ZVDataLayer.ClassificazioneLeggi(this.idWorkflow);
                return classificazione;
            }
        }

        /// <summary>
        /// DataSet delle competenze.
        /// </summary>
        public ZVCompetenzeDS Competenze
        {
            get
            {
                return (OperazioneCompetenze as IZVCompetenze).Competenze;
            }
        }

        [NonSerialized]
        private ZVScadenzeDS scadenze;
        /// <summary>
        /// Assume true se il dataset delle scadenze è valorizzato, false altrimenti.
        /// </summary>
        public bool ScadenzeNotNull
        {
            get
            {
                return (scadenze != null);
            }
        }

        /// <summary>
        /// DataSet delle scadenze.
        /// </summary>
        public ZVScadenzeDS Scadenze
        {
            get
            {
                if (scadenze == null)   // leggi solo la prima volta
                    scadenze = ZVDataLayer.ScadenzeLeggi(this.idProcesso);
                return scadenze;
            }
        }

        [NonSerialized]
        private int fonteParametriEsterni = FonteParEsterni.Nessuna;
        /// <summary>
        /// 0 -- Nessuna Fonte
        /// 1 -- Da tabella ZV_ParametriEsterni
        /// 2 -- Da link esterno
        /// </summary>
        public int FonteParametriEsterni
        {
            get { return fonteParametriEsterni; }
            set { fonteParametriEsterni = value; }
        }


        #endregion


        #region Proprieta salvate a design
        private List<ZVStato> stati = new List<ZVStato>();
        public List<ZVStato> Stati
        {
            get
            {
                return stati;
            }
        }

        //    private int height = 8000;

        private string titolo = String.Empty;
        /// <summary>
        /// Titolo del workflow.
        /// </summary>
        public string Titolo
        {
            get { return titolo; }
            set { titolo = value; }
        }

        private int priorita = 0;
        /// <summary>
        /// Priorità del workflow.
        /// </summary>
        public int Priorita
        {
            get { return priorita; }
            set { priorita = value; }
        }


        private string categoria = String.Empty;
        /// <summary>
        /// Categoria del workflow.
        /// </summary>
        public string Categoria
        {
            get { return categoria; }
            set { categoria = value; }
        }

        /// <summary>
        /// Stato iniziale del workflow.
        /// </summary>
        public ZVStato StatoIniziale
        {
            get
            {
                if (statoCorrente != null && statoCorrente.Iniziale)
                    return statoCorrente;  // A run time possibile copia 
                return Stati[0];
            }
        }



        [NonSerialized]
        ZVAzione azioneAperturaStandard;
        /// <summary>
        /// Azione Apertura standard.
        /// </summary>
        public ZVAzione AzioneAperturaStandard
        {
            get
            {
                if (azioneAperturaStandard.Stato == null)
                    azioneAperturaStandard.Stato = StatoIniziale;
                return azioneAperturaStandard;
            }
        }

        /// <summary>
        /// Carica l'azione apertura standard.
        /// </summary>
        public void AzioneAperturaStandardLoad()
        {
            azioneAperturaStandard = StatoIniziale.Azioni[0].GetCopia();
        }


        /// <summary>
        /// Inizializza l'azione apertura standard.
        /// </summary>
        public void AzioneAperturaStandardInit()
        {
            azioneAperturaStandard = null;
        }

        // AB - 20180606: aggiunta nuova proprietà
        [NonSerialized]
        private Dictionary<int, ZVAzione> azioniInRiepilogo = new Dictionary<int, ZVAzione>();
        /// <summary>
        /// Lista delle azioni visualizzabili nella griglia di riepilogo
        /// </summary>
        public Dictionary<int, ZVAzione> AzioniInRiepilogo
        {
            get { return azioniInRiepilogo; }
            set { azioniInRiepilogo = value; }
        }
        // AB - 20180606: aggiunta nuova proprietà
        [NonSerialized]
        private Dictionary<int, ZVAzione> azioniInStorico = new Dictionary<int, ZVAzione>();
        /// <summary>
        /// Lista delle azioni visualizzabili nella griglia di storico
        /// </summary>
        public Dictionary<int, ZVAzione> AzioniInStorico
        {
            get { return azioniInStorico; }
            set { azioniInStorico = value; }
        }

        //documento .doc compresso che contiene il modello usato per la stampa delle operazioni modulo
        private byte[] modelloStampa;
        public byte[] ModelloStampa
        {
            get { return modelloStampa; }
            set { modelloStampa = value; }
        }

        //flag per sapere se il workflow ha una o più stampe associate - da mantenere il modello stampe in byte per il pregresso
        private List<ModelloStampa> aModelloStampe = new List<ModelloStampa>();
        public List<ModelloStampa> ListaModelloStampe
        {
            get { return aModelloStampe; }
            set { aModelloStampe = value; }
        }

        private List<AggiornaDbEsterno> aDBEsterno = new List<AggiornaDbEsterno>();
        public List<AggiornaDbEsterno> ListaDBEsterno
        {
            get { return aDBEsterno; }
            set { aDBEsterno = value; }
        }

        private string oggetto = "";
        public string Oggetto
        {
            get { return oggetto; }
            set { oggetto = value; }
        }

        private string help = "";
        public string Help
        {
            get { return help; }
            set { help = value; }
        }



        // Elenca le azioni contenute in tutti gli stati del workflow 
        public List<ZVAzione> Azioni
        {
            get
            {
                List<ZVAzione> lsa = new List<ZVAzione>();
                foreach (ZVStato st in Stati)
                    lsa.AddRange(st.Azioni);
                return lsa;
            }
        }
        public List<ZVOperazione> Operazioni
        {
            get
            {
                List<ZVOperazione> lsp = new List<ZVOperazione>();
                foreach (ZVStato stato in Stati)
                    foreach (ZVAzione azione in stato.Azioni)
                        lsp.AddRange(azione.Operazioni);
                return lsp;
            }
        }

        private bool stessoID = false;
        public bool StessoID
        {
            get { return stessoID; }
            set { stessoID = value; }
        }
        #endregion

        #region Proprieta ParametriAttivazioneAutomatica

        //private string tipologia = ZV_Comune.Globals.VA_TIPOLOGIA_ATTIVAZAUTO_NOAUTO; //dbg_gdc: veniva inizializzato con "no" che non � un valore di quelli nel combobox
        private string tipologia = "";
        public string Tipologia
        {
            get { return tipologia; }
            set { tipologia = value; }
        }

        private bool fineMese;
        public bool FineMese
        {
            get { return fineMese; }
            set { fineMese = value; }
        }

        private string seFestivo;
        public string SeFestivo
        {
            get { return seFestivo; }
            set { seFestivo = value; }
        }

        private int giornoDelPeriodo;
        public int GiornoDelPeriodo
        {
            get { return giornoDelPeriodo; }
            set { giornoDelPeriodo = value; }
        }

        private string azioneEseguire;
        public string AzioneEseguire
        {
            get { return azioneEseguire; }
            set { azioneEseguire = value; }
        }

        #endregion

        #region Implementazione Interfaccia per Design

        // get read write e nomeCompetenza su workflow.
        private bool attivo;
        [NonSerialized]
        public object ResponseWeb = null;

        public bool Attivo
        {
            get { return attivo; }
            set { attivo = value; }
        }

        private bool stdRiservato;
        public bool StdRiservato
        {
            get { return stdRiservato; }
            set { stdRiservato = value; }
        }

        public bool NascondiRiepilogo { get; set; } = false;
        public bool NonConsenteElimina { get; set; } = false;
        public bool NonConsenteSalva { get; set; } = false;
        public bool NonConsenteStampa { get; set; } = false;
        public bool NonConsenteCompetenze { get; set; } = false;
        public bool ConsenteAzioniMultiple { get; set; } = false;
        public bool NonConsenteStorico { get; set; } = false;

        public string Name
        {
            get { return Titolo; }
        }


        public ZVOperazione OperazioneCompetenze  // copia istanza azione competenze
        {
            get
            {
                foreach (ZVOperazione ope in AzioneAperturaStandard.Operazioni)
                {
                    if (ope.GetType().Name == ZVOperazione.ClasseCompetenzaWorkflow)
                        return ope;
                }
                return null;
            }
        }

        public List<string> OperazioniDaSalvareInTabella { get; set; } = new List<string>();

        #endregion

        #region Sottoclassi

        // AB - 20170201: Nella classi seguenti ho dovuto definire campi pubblici al posto delle proprietà poichè, essendo la classe [Serializable], 
        //                l'eventuale serielizzazione in formato json veniva sporcata.
        /// <summary>
        /// Classe che identifica un'operazione inizializzabile dall'esterno
        /// </summary>
        [Serializable]
        public class ZVOperazioneInizializzabile
        {
            /// <summary>
            /// Nome operazione.
            /// </summary>
            public string Nome;

            /// <summary>
            /// Tipo operazione.
            /// </summary>
            public string Tipo;

            /// <summary>
            /// Etichetta operazione.
            /// </summary>
            public string Label;

            /// <summary>
            /// Lista delle proprietà inizializzabili
            /// </summary>
            public List<string> Parametri;

            /// <summary>
            /// Costruttore di default
            /// </summary>
            public ZVOperazioneInizializzabile()
            {
                Parametri = new List<string>();
            }

            /// <summary>
            /// Costruttore 
            /// </summary>
            /// <param name="op">
            /// Operazione con cui inizializzare l'entità.
            /// </param>
            public ZVOperazioneInizializzabile(ZVOperazione op) : this()
            {
                Nome = op.Name;
                Tipo = op.GetType().Name;
                Label = op.Label;
                Parametri.AddRange(op.ParametriEsterniValorizzabili);
            }
        }

        /// <summary>
        /// Classe che identifica un'operazione inizializzabile dall'esterno
        /// </summary>
        [Serializable]
        public class ZVAzioniEseguibili
        {
            /// <summary>
            /// Nome azione.
            /// </summary>
            public string Nome;

            /// <summary>
            /// Etichetta azione.
            /// </summary>
            public string Label;

            /// <summary>
            /// Costruttore di default
            /// </summary>
            public ZVAzioniEseguibili()
            {

            }

            /// <summary>
            /// Costruttore 
            /// </summary>
            /// <param name="az">
            /// Azione con cui inizializzare l'entità.
            /// </param>
            public ZVAzioniEseguibili(ZVAzione az) : this()
            {
                Nome = az.Name;
                Label = az.Etichetta;
            }

            /// <summary>
            /// Costruttore 
            /// </summary>
            /// <param name="nome">Nome dell'azione.</param>
            /// <param name="label">Label dell'azione.</param>
            public ZVAzioniEseguibili(string nome, string label) : this()
            {
                Nome = nome;
                Label = label;
            }
        }

        /// <summary>
        /// Classe che identifica l'insieme delle informazioni utili per un wf automatico
        /// </summary>
        [Serializable]
        public class ZVInfoWorkflowAutomatici
        {
            /// <summary>
            /// Nome operazione.
            /// </summary>
            public List<ZVOperazioneInizializzabile> OperazioniInizializzabili;

            /// <summary>
            /// Tipo operazione.
            /// </summary>
            public List<ZVAzioniEseguibili> AzioniEsegubili;

            /// <summary>
            /// Assume true se il work esiste ed � attivo, false altrimenti
            /// </summary>
            public bool Disponibile;
        }

        /// <summary>
        /// Classe che identifica le abilitazioni a Tableau Cliente.
        /// </summary>
        [Serializable]
        public class AbilitazioniTC
        {
            /// <summary>
            /// Costruttore.
            /// </summary>
            public AbilitazioniTC()
            {
                // impone i default
                Accesso = false;
                NoDipendenti = true;
                NoScudati = true;
                VedeTutto = false;

                ElencoFiliali = new List<String>();
                ElencoSettoristiCredito = new List<String>();
                ElencoSettoristiFinanziari = new List<String>();

            }

            /// <summary>
            /// Accesso a Tableau Cliente
            /// </summary>
            public Boolean Accesso { get; set; }
            /// <summary>
            /// Diniego di visibilit� Dipendenti
            /// </summary>
            public Boolean NoDipendenti { get; set; }
            /// <summary>
            /// Diniego di visibilit� Scudati
            /// </summary>
            public Boolean NoScudati { get; set; }
            /// <summary>
            /// Vede tutto ad esclusione di Dipendenti e Scudati che utilizzano le relative restrizioni
            /// </summary>
            public Boolean VedeTutto { get; set; }
            /// <summary>
            /// Elenco delle filiali a cui ha accesso (non in formato "ESO|...") ma solo codice. Default � una lista vuota.
            /// </summary>
            public List<String> ElencoFiliali { get; set; }

            /// <summary>
            /// Elenco dei setttoristi credito a cui ha accesso (non in formato "RIS|...") ma solo codice. Default � una lista vuota.
            /// </summary>
            public List<String> ElencoSettoristiCredito { get; set; }

            /// <summary>
            /// Elenco dei settoristi finanziari a cui ha accesso (non in formato "RIS|...") ma solo codice. Default � una lista vuota.
            /// </summary>
            public List<String> ElencoSettoristiFinanziari { get; set; }
        }


        #endregion
    }
}
