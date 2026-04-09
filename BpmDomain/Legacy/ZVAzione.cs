using System.Data;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace ZV_Engine
{
    /// <summary>
    /// Azione di workflow.
    /// </summary>
    [Serializable]
    public class ZVAzione : ZV_Engine.IZVEngineObject
    {
        #region Proprieta e metodi d'uso a Run Time (processo)
        public enum TipoPersistenza { Azione, StatoCorrente, Stato, Workflow, Utente };

        public const string AperturaStandard = "Apertura standard";
        public const string Apertura = "Apertura";
        public const string Scadenza = "Azione scadenza";
        public const string ImpostaStato = "Imposta stato";
        public const string Salva = "Salva";
        public const string EliminazioneDefinitiva = " Elimina workflow - ATTENZIONE: il processo è irreversibile! ";

        public enum enumAzioneRiepilogo
        {
            Nessuno = 0,
            Lettura = 1,
            Modifica = 2
        }

        /// <summary>
        /// Tipi di comportamento dell'azione.
        /// </summary>
        public enum enumComportamentoAzione
        {
            /// <summary>
            /// Normale
            /// </summary>
            Normale,
            /// <summary>
            /// Non controllare i campi obbligatori
            /// </summary>
            NonValidareObbl,
            /// <summary>
            /// Non salvare i valori delle operazioni
            /// </summary>
            NonSalvare
        }

        public struct LogLvL
        {
            public const string Info = "INFO";
            public const string Debug = "DEBUG";
            public const string Warning = "WARN";
            public const string Error = "ERR";
            public const string Critical = "CRIT";
        }

        public struct LogSrC
        {
            public const string NetReport = "NETREPORT";
            public const string Designer = "DESIGNER";
            public const string TabelleSistema = "TABELLE_SISTEMA";
            public const string TabelleUtente = "TABELLE_UTENTE";
            public const string Schedulatore = "SCHEDULATORE";
        }

        /// <summary>
        /// Assume true se l'azione è di sistema(non creata dall'utente), false altrimenti.
        /// </summary>
        public bool IsAzioneStandard
        {
            get
            {
                return Name == ZVAzione.AperturaStandard ||
                       Name == ZVAzione.Apertura ||
                       Name == ZVAzione.Scadenza ||
                       Name == ZVAzione.ImpostaStato;
            }
        }

        private bool _isSoloIcona = false;
        /// <summary>
        /// Assume true se il pulsante dell'azione deve visualizzare solo l'icona senza label, false altrimenti.
        /// </summary>
        public bool IsSoloIcona
        {
            get { return _isSoloIcona; }
            set { _isSoloIcona = value; }
        }

        /// <summary>
        /// Assume true se l'azione è di eliminazione definitiva, false altrimenti.
        /// </summary>
        public bool IsEliminazioneDefinitiva
        {
            get
            {
                return Name == ZVAzione.EliminazioneDefinitiva;
            }
        }
        /// <summary>
        /// Assume true se l'azione è di apertura, false altrimenti.
        /// </summary>
        public bool IsApertura
        {
            get
            {
                return Name == ZVAzione.AperturaStandard ||
                       Name == ZVAzione.Apertura;
            }
        }

        /// <summary>
        /// Assume true se l'azione è di apertura standard, false altrimenti.
        /// </summary>
        public bool IsAperturaStandard
        {
            get
            {
                return Name == ZVAzione.AperturaStandard;
            }
        }

        // AB - 20170929: Gestione delle azioni di sistema
        /// <summary>
        /// Assume true se l'azione è di sistema, false altrimenti.
        /// Le azioni di sistema sono quelle azione che non vengono definite dell'utente in design, ma sono create automaticamente dell'engine(es: Salva) 
        /// </summary>
        public bool AzioneDiSistema { get; set; }

        // public event Interfaccia;
        // Richiede alle singole operazioni se sono eseguibili nel contesto attuale.
        private bool Eseguibile(ContestoUtilizzo contesto)
        {
            bool attive = false;

            if (contesto == ContestoUtilizzo.Storico)
                if (((ZV_Engine.IZVCompetenzeWorkflow)Workflow.OperazioneCompetenze).UtenteCorrenteSupervisore)
                    return true;
                else
                    return false;

            //MR 05/04/2016
            //Se il workflow è dichiarato con Richiedente Anonimo, vengono saltat ii controlli delle operazioni 
            //nello Stato Iniziale del Workflow
            if ((contesto == ContestoUtilizzo.Esegui) && attivo && this.Workflow.StatoCorrente.Iniziale && this.Workflow.RichiedenteAnonimo)

                return true;

            //if (attivo && (Stato.Autore || (Stato.Lettore && (contesto == ContestoUtilizzo.Lettura ||
            //    contesto == ContestoUtilizzo.RiepilogoInLettura || contesto == ContestoUtilizzo.Storico
            //    ))))  // Scarta azioni disattivate
            if (attivo && Stato.Lettore)
                foreach (ZVOperazione operazione in
                            contesto != ContestoUtilizzo.Riepilogo && contesto != ContestoUtilizzo.RiepilogoInLettura ?
                                OperazioniAttive :
                                OperazioniRiepilogo.FindAll(ope => ope.GetType().Name.Equals("ZVCompetenzaAzione")))
                {
                    if (!operazione.Eseguibile(contesto))
                        return false;  // Se un operazione non è esgubile non lo è tutta l'azione   
                    attive = true;
                }


            return attive; // tutte le operazioni sono eseguibili e ce ne sono
        }

        /// <summary>
        /// Stato a cui appartiene l'azione.
        /// </summary>
        public IZVEngineObject Parent
        {
            get { return Stato; }
        }

        /// <summary>
        /// Costrruttore di default
        /// </summary>
        public ZVAzione()
        {
        }

        /// <summary>
        /// Costruttore.
        /// </summary>
        /// <param name="iName">Nome dell'azione.</param>
        /// <param name="iStato">Stato a cui appartiene.</param>
        public ZVAzione(string iName, ZVStato iStato)
        {
            Name = iName;
            Stato = iStato;
            if (!IsAzioneStandard)
            {
                ZVOperazione operazioneCompetenze = ZVOperazione.NewOperazione(ZVOperazione.ClasseCompetenzaAzione);
                AddOperazione(operazioneCompetenze);
            }
        }

        // AB - 20180925: Modifica della logica di pulizia degli errori
        /// <summary>
        /// Pulisce tutti gli errori delle operazioni attive.
        /// </summary>
        public void ClearErroriOperazioniAttive()
        {
            foreach (ZVOperazione op in OperazioniAttive)
            {
                op.ClearErrori();
            }
        }

        private int idAzione = -1;  // Per record nuovo l'id è assegnato dal database
        /// <summary>
        /// ID dell'azione.
        /// </summary>
        public int IdAzione
        {
            get { return idAzione; }
            set
            {
                idAzione = value;
            }
        }
        [NonSerialized]
        private int virtualKey = 0;  // Chiave virtuale per persistenza diversa da azione.
        public int VirtualKey
        {
            get
            {
                if (virtualKey == 0)
                    return idAzione;
                else
                    return virtualKey;
            }
            set
            {
                virtualKey = value;
            }
        }
        [NonSerialized]
        private int statoVirtualKey = 0;  // Chiave virtuale per persistenza diversa da stato.
        public int StatoVirtualKey
        {
            get
            {
                if (statoVirtualKey == 0)
                    return this.Stato.IdStato;
                else
                    return statoVirtualKey;
            }
            set { statoVirtualKey = value; }
        }

        public bool CambioStatoCompleto   // Ritorna se è possibile effettuare il cambio stato
        {
            get { return !CambioStatoInclusivo || UltimoAttore; }
        }

        private bool ultimoAttore;  // Da impostare a carico delle competenze 
        public bool UltimoAttore    // solo per azioni inclusiva quando l'azione è
        {                           // eseguita dall'ultimo attore che doveva confermare 
            get { return ultimoAttore; }
            set { ultimoAttore = value; }
        }

        private DateTime dataAzione;  // inserita uatomaticamante alla prima registrazione
        public DateTime DataAzione    // nel caso di modifica di un azione la dato e ora è assieme alle competenze
        {
            get { return dataAzione; }
            set { dataAzione = value; }
        }
        public ZVCompetenzeDS Competenze
        {
            get
            {
                if (OperazioneCompetenze == null)
                    return new ZVCompetenzeDS();
                return (OperazioneCompetenze as IZVCompetenze).Competenze;
            }
        }

        ZVAzione.enumComportamentoAzione _comportamentoazione = ZVAzione.enumComportamentoAzione.Normale;
        public ZVAzione.enumComportamentoAzione Comportamento
        {
            get
            { return _comportamentoazione; }
            set
            { _comportamentoazione = value; }
        }

        string _categoria = string.Empty;
        public string Categoria
        {
            get
            { return _categoria; }
            set
            { _categoria = value; }
        }

        public string UidAzione
        {
            get
            {
                string _ret = string.Empty;
                if (this.Stato != null)
                    _ret = new StringBuilder().AppendFormat("{0}~{1}", this.Stato.Name, this.Name).ToString();

                return _ret;
            }
        }

        /// <summary>
        /// Restituisce i testi di base contenuti nel design (utilizzato in ricerca)
        /// </summary>
        public virtual string TestiContenutiNelDesign()
        {
            StringBuilder AzSTRINGs = new StringBuilder();
            AzSTRINGs.AppendFormat("{0} ", Name ?? string.Empty);
            AzSTRINGs.AppendFormat("{0} ", Etichetta ?? string.Empty);
            return AzSTRINGs.ToString();
        }

        /// <summary>
        ///   Restituisce l'interfaccia Windows di una azione
        /// </summary>
        /// <param name="lettura">Se true, forza l'interfaccia della azione in sola lettura (utente lettore) anche se l'utente fosse autorizzato alla modifica (utente autore)</param>
        /// <param name="operazioni">Interfaccia utente della azione</param>
        /// <returns></returns>
        public bool Interfaccia(bool lettura, bool primaChiamata, out List<Control> control, out List<ZVOperazione> operazioni)
        {
            return Interfaccia(lettura, primaChiamata, out control, out operazioni, false);
        }

        public bool Interfaccia(bool lettura, bool primaChiamata, out List<Control> control, out List<ZVOperazione> operazioni, bool all)
        {
            bool InterfacciaEditabile;
            bool esiste = Interfaccia(ContestoEsegui(lettura), primaChiamata, out control, out operazioni, out InterfacciaEditabile, all);
            if (InterfacciaEditabile)
                Stato.InterfacciaEditabile = true;
            return esiste;
        }

        public bool InterfacciaWeb(bool lettura, bool primaChiamata, out List<WebControl> control, out List<ZVOperazione> operazioni)
        {
            bool InterfacciaEditabile;
            bool esiste = InterfacciaWeb(ContestoEsegui(lettura), primaChiamata, out control, out operazioni, out InterfacciaEditabile, true);
            if (InterfacciaEditabile)
                Stato.InterfacciaEditabile = true;
            return esiste;
        }

        public bool InterfacciaRiepilogo(bool lettura, bool primaChiamata, out List<Control> control, out List<ZVOperazione> operazioni)
        {
            bool InterfacciaEditabile;
            return Interfaccia(ContestoRiepilogo(lettura), primaChiamata, out control, out operazioni, out InterfacciaEditabile, false);
        }

        public bool InterfacciaRiepilogoWeb(bool lettura, bool primaChiamata, out List<WebControl> control, out List<ZVOperazione> operazioni)
        {
            bool InterfacciaEditabile;
            return InterfacciaWeb(ContestoRiepilogo(lettura), primaChiamata, out control, out operazioni, out InterfacciaEditabile, false);
        }

        public bool InterfacciaStorico(bool lettura, bool primaChiamata, out List<Control> control, out List<ZVOperazione> operazioni)
        {
            bool InterfacciaEditabile;
            return Interfaccia(ContestoUtilizzo.Storico, primaChiamata, out control, out operazioni, out InterfacciaEditabile, false);
        }

        public bool InterfacciaStoricoWeb(bool lettura, bool primaChiamata, out List<WebControl> control, out List<ZVOperazione> operazioni)
        {
            bool InterfacciaEditabile;
            return InterfacciaWeb(ContestoUtilizzo.Storico, primaChiamata, out control, out operazioni, out InterfacciaEditabile, false);
        }

        public bool Eseguibile(bool lettura)
        {
            return Eseguibile(ContestoEsegui(lettura));
        }

        public bool EseguibileStorico()
        {
            return Eseguibile(ContestoUtilizzo.Storico);
        }

        // AB - 20180605: aggiunto variabile azioneRiepilogo per conoscere in che contesto di riepilogo ci si trova
        /// <summary>
        /// Eseguibilita dell'azione in riepilogo.
        /// </summary>
        [NonSerialized]
        public enumAzioneRiepilogo azioneRiepilogo = enumAzioneRiepilogo.Nessuno;
        /// <summary>
        /// Eseguibilita dell'azione in riepilogo.
        /// </summary>
        public enumAzioneRiepilogo AzioneRiepilogo
        {
            get { return azioneRiepilogo; }
            set { azioneRiepilogo = value; }
        }

        /// <summary>
        /// Calcola l'eseguibilita dell'azione in riepilogo
        /// </summary>
        /// <returns></returns>
        public enumAzioneRiepilogo EseguibileRiepilogo()
        {
            // AB - 20180605: gestione variabile azioneRiepilogo.
            /*
            if (Eseguibile(ContestoUtilizzo.Riepilogo))
            {
                return enumAzioneRiepilogo.Modifica;
            }
            else if (Eseguibile(ContestoUtilizzo.RiepilogoInLettura))
            {
                return enumAzioneRiepilogo.Lettura;
            }
            return enumAzioneRiepilogo.Nessuno;
            */

            AzioneRiepilogo = enumAzioneRiepilogo.Nessuno;
            if (Eseguibile(ContestoUtilizzo.Riepilogo))
            {
                AzioneRiepilogo = enumAzioneRiepilogo.Modifica;
            }
            else if (Eseguibile(ContestoUtilizzo.RiepilogoInLettura))
            {
                AzioneRiepilogo = enumAzioneRiepilogo.Lettura;
            }
            return AzioneRiepilogo;
        }

        int indiceOperazione = 0;
        List<ZVOperazione> operazioniTemp = null;

        private bool Interfaccia(ContestoUtilizzo contesto, bool primaChiamata, out List<Control> controlOperazioni, out List<ZVOperazione> operazioni, out bool interfacciaEditabile, bool all)
        {
            interfacciaEditabile = false;
            controlOperazioni = new List<Control>();
            operazioni = new List<ZVOperazione>();
            if (primaChiamata)
            {
                if (!Stato.Autore && !Stato.Lettore && !Stato.Workflow.RichiedenteAnonimo)
                    return false;
                InitAzione();   // rilegge i dati per limitare i problemi di concorrenza
                indiceOperazione = 0;
                if (contesto == ContestoUtilizzo.Riepilogo || contesto == ContestoUtilizzo.RiepilogoInLettura)
                    operazioniTemp = OperazioniRiepilogo;
                else
                    operazioniTemp = OperazioniAttive;
            }
            int numeroOpe = 0;
            int indiceOpeContenuta = 0;
            Control contenitore = null;
            bool wInterfacciaEditabile = false;
            while (indiceOperazione < operazioniTemp.Count)
            {
                ZVOperazione oper = operazioniTemp[indiceOperazione];
                indiceOperazione++;
                operazioni.Add(oper);
                Control interfacciaOperazione = null;
                try
                {

                    //reset proprietà forzata delle operazioni
                    try
                    {
                        PropertyInfo prop = oper.GetType().GetProperty("ForzaNonObbligatorio");
                        if (null != prop)
                            prop.SetValue(oper, false, null);
                    }
                    catch (Exception) { }
                    //----------------


                    if (oper.GetType() == ZVOperazione.NewOperazione("ZVSeparatoreBox").GetType() &&
                        (contesto != ContestoUtilizzo.Template || contesto == ContestoUtilizzo.Workflow))
                    {
                        bool aa = false;
                        contenitore = oper.Control(contesto, out aa);
                        numeroOpe = (int)oper.GetType().GetProperty("NumOperazioni").GetValue(oper, null);
                        contenitore.TabIndex = indiceOperazione;
                        contenitore.Visible = oper.CtrVisibile;
                    }
                    else if (contenitore != null && indiceOpeContenuta < numeroOpe)
                    {
                        bool inter = false;
                        Control ctr = oper.Control(contesto, out inter);
                        if (inter)
                            wInterfacciaEditabile = true;
                        if (((oper.GetType() != ZVOperazione.NewOperazione("ZVCampoLabel").GetType()) || (!oper.CtrAttivo && oper.GetType() != ZVOperazione.NewOperazione("ZVCampoLabel").GetType())) && ctr != null)
                            ctr.TabIndex = indiceOpeContenuta; // 19/12/12 le operazioni contenute in SeparatoreBox hanno una numerazione indipendente del tabindex //indiceOperazione;

                        if (ctr != null)
                        {
                            contenitore.GetType().InvokeMember("AddControl", System.Reflection.BindingFlags.InvokeMethod, null, contenitore, new object[] { ctr }); //gdc: aggiunto pannello interno alla groupbox

                            // AB - 20170615: Se il controllo web è stato creato, imposto che il controllo è visibile
                            oper.CtrVisibile = true;
                        }
                        indiceOpeContenuta++;
                    }
                    else
                    {
                        interfacciaOperazione = oper.Control(contesto, out wInterfacciaEditabile);

                        // AB - 20170615: Se il controllo web è stato creato, imposto che il controllo è visibile
                        if (interfacciaOperazione != null)
                            oper.CtrVisibile = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.Label lbl = new System.Windows.Forms.Label();
#if DEBUG
                    lbl.Text = new StringBuilder().AppendFormat("Errore creazione {0}: {1}", this.Name, ex.Message).ToString();
#else
                    lbl.Text = new StringBuilder("Errore creazione:").Append(this.Name).ToString();
#endif
                    interfacciaOperazione = lbl;
                }
                if (numeroOpe == indiceOpeContenuta && contenitore != null)
                {
                    controlOperazioni.Add(contenitore);
                    contenitore = null;
                    indiceOpeContenuta = 0;
                }
                if (interfacciaOperazione != null)
                {
                    if (oper.GetType() != ZVOperazione.NewOperazione("ZVCampoLabel").GetType() && oper.CtrAttivo)
                        interfacciaOperazione.TabIndex = indiceOperazione;
                    controlOperazioni.Add(interfacciaOperazione);
                    if (wInterfacciaEditabile)
                        interfacciaEditabile = true;
                }
                if (indiceOperazione < operazioniTemp.Count)
                {
                    ZVOperazione operSuc = operazioniTemp[indiceOperazione];
                    if (!all && (oper.InterfacciaSingola || operSuc.InterfacciaSingola))
                        return true;
                }
            }
            if (wInterfacciaEditabile)
                interfacciaEditabile = true;
            return false;
        }

        private bool InterfacciaWeb(ContestoUtilizzo contesto, bool primaChiamata, out List<WebControl> controlOperazioni, out List<ZVOperazione> operazioni, out bool interfacciaEditabile, bool all)
        {


            interfacciaEditabile = false;
            controlOperazioni = new List<WebControl>();
            operazioni = new List<ZVOperazione>();

            try
            {
                List<string> idWorkflowManutenzione = String.IsNullOrEmpty(ConfigurationManager.AppSettings["idWorkflowManutenzione"]) ? new List<string>() : ConfigurationManager.AppSettings["idWorkflowManutenzione"].Split(';').ToList();
                if (idWorkflowManutenzione.Contains(Workflow.IdWorkflow.ToString()) || idWorkflowManutenzione.Contains(Workflow.Categoria)) //Verifica lista idWorkflow da escludere 
                {
                    string msgWorkflowManutenzione = String.IsNullOrEmpty(ConfigurationManager.AppSettings["msgWorkflowManutenzione"]) ? "" : ConfigurationManager.AppSettings["msgWorkflowManutenzione"];
                    throw new ApplicationException(msgWorkflowManutenzione);
                }
            }
            catch (ApplicationException ex)
            {
                throw ex;


            }
            catch (Exception ex)
            {

            }


            if (primaChiamata)
            {
                if (!Stato.Autore && !Stato.Lettore && !Stato.Workflow.RichiedenteAnonimo)
                    return false;
                //             InitAzione();   //web non si puo rileggere dati oggetto in memoria ma controlli ricreati ad ogni post
                indiceOperazione = 0;
                if (contesto == ContestoUtilizzo.Riepilogo || contesto == ContestoUtilizzo.RiepilogoInLettura)
                    operazioniTemp = OperazioniRiepilogo;
                else
                    operazioniTemp = OperazioniAttive;
            }

            int numeroOpe = 0;
            int indiceOpeContenuta = 0;
            WebControl contenitore = null;
            bool wInterfacciaEditabile = false;
            while (indiceOperazione < operazioniTemp.Count)
            {
                ZVOperazione oper = operazioniTemp[indiceOperazione];
                indiceOperazione++;
                operazioni.Add(oper);
                WebControl interfacciaOperazione = null;
                try
                {

                    //reset proprietà forzata delle operazioni
                    try
                    {
                        PropertyInfo prop = oper.GetType().GetProperty("ForzaNonObbligatorio");
                        if (null != prop)
                            prop.SetValue(oper, false, null);
                    }
                    catch (Exception) { }
                    //----------------



                    if (oper.GetType() == ZVOperazione.NewOperazione("ZVSeparatoreBox").GetType() &&
                        (contesto != ContestoUtilizzo.Template || contesto == ContestoUtilizzo.Workflow))
                    {
                        bool aa = false;
                        //contenitore = oper.ControlWeb(contesto, out aa);
                        contenitore = oper.ControlRuntimeWeb(contesto, out aa);
                        numeroOpe = (int)oper.GetType().GetProperty("NumOperazioni").GetValue(oper, null);
                        // AB - 20180628: Corretto anomalia di nascondimento/visualizzazione di un separatore box in funzione del nascondimento/visualizzazione 
                        //                delle operazioni interne da c#
                        //contenitore.Visible = oper.CtrVisibile;
                        if (!oper.CtrVisibile)
                            // Imposto solo la non visibilità del controllo
                            contenitore.Visible = oper.CtrVisibile;
                    }
                    else if (contenitore != null && indiceOpeContenuta < numeroOpe)
                    {
                        bool inter = false;
                        //WebControl ctr = oper.ControlWeb(contesto, out inter);
                        WebControl ctr = oper.ControlRuntimeWeb(contesto, out inter);
                        if (inter)
                            wInterfacciaEditabile = true;

                        if (ctr != null)
                        {
                            contenitore.GetType().InvokeMember("AddControl", System.Reflection.BindingFlags.InvokeMethod, null, contenitore, new object[] { ctr }); //gdc: aggiunto pannello interno alla groupbox

                            // AB - 20170615: Se il controllo web è stato creato, imposto che il controllo è visibile
                            oper.CtrVisibile = true;
                        }
                        indiceOpeContenuta++;
                    }
                    else
                    {
                        // if (oper.GetType() != ZVOperazione.NewOperazione("ZVPaginaWeb").GetType())
                        //interfacciaOperazione = oper.ControlWeb(contesto, out wInterfacciaEditabile);
                        interfacciaOperazione = oper.ControlRuntimeWeb(contesto, out wInterfacciaEditabile);

                        // AB - 20170615: Se il controllo web è stato creato, imposto che il controllo è visibile
                        if (interfacciaOperazione != null)
                            oper.CtrVisibile = true;
                    }
                }
                catch (Exception ex)
                {
                    if (ZVProxy<DataSet>.Proxy.CurrentParms.ActivateDebug)
                    {
                        StringBuilder msg = new StringBuilder("ZVAzione-InterfacciaWeb: ").AppendLine(ex.Message).Append(ex.StackTrace);
                        if (ex.InnerException != null)
                        {
                            msg.AppendFormat("\n{0}\n{1}", ex.InnerException.Message, ex.InnerException.StackTrace);
                        }
                        Logging.writeLog(msg.ToString(), ZVProxy<DataSet>.Proxy.CurrentParms.DebugFile);
                    }
                }
                if (numeroOpe == indiceOpeContenuta && contenitore != null)
                {
                    // AB - 20160630: salvo sul separatore box se contiene operazioni editabili.
                    contenitore.GetType().GetProperty("InterfacciaEditabile").SetValue(contenitore, wInterfacciaEditabile, null);
                    controlOperazioni.Add(contenitore);
                    contenitore = null;
                    indiceOpeContenuta = 0;
                    // AB - 20160608: Imposto lo stato di interfaccia editabile nel caso in cui almeno una delle operazioni contenute sia editabile
                    if (wInterfacciaEditabile)
                        interfacciaEditabile = true;
                }
                if (interfacciaOperazione != null)
                {
                    controlOperazioni.Add(interfacciaOperazione);
                    if (wInterfacciaEditabile)
                        interfacciaEditabile = true;
                }
                if (indiceOperazione < operazioniTemp.Count)
                {
                    ZVOperazione operSuc = operazioniTemp[indiceOperazione];
                    if (!all && (oper.InterfacciaSingola || operSuc.InterfacciaSingola))
                        return true;
                }
            }

            if (wInterfacciaEditabile)
                interfacciaEditabile = true;
            return false;
        }

        ContestoUtilizzo ContestoEsegui(bool lettura)
        {
            if (lettura) // || !Stato.Autore)
                return ContestoUtilizzo.Lettura;
            if (inModifica)
                return ContestoUtilizzo.EseguiInModifica;
            return ContestoUtilizzo.Esegui;
        }
        ContestoUtilizzo ContestoRiepilogo(bool lettura)
        {
            if (lettura) // || !Stato.Autore)
                return ContestoUtilizzo.RiepilogoInLettura;
            return ContestoUtilizzo.Riepilogo;
        }

        private void Inserisci(bool automatica)
        {
            this.Workflow.AzioniInserite.Add(this);
            dataAzione = DateTime.Now;
            this.IdAzione = ZVDataLayer.AzioniAggiorna(this.GetDataSet(false, automatica));
        }

        private void Elimina()  // usato se senza transazione
        {
            ZVDataLayer.AzioniAggiorna(this.GetDataSet(true, false));
        }

        public void Esegui(bool collegata)
        {
            if (Stato.Autore || Stato.Workflow.RichiedenteAnonimo)
                Esegui(false, collegata);
        }

        public void Esegui()
        {
            if (Stato.Autore || Stato.Workflow.RichiedenteAnonimo || this.IsEliminazioneDefinitiva)
                Esegui(false, false);
        }

        public void AttivaEsegui()  // da server
        {
            Esegui(false, false);
            if (!String.IsNullOrEmpty(AzioneCollegata) && AzioneCollegata != this.Name)
            {
                ZVAzione azcol = this.Workflow.StatoCorrente.AzioneDaNome(AzioneCollegata);
                if (azcol != null)
                    azcol.Esegui(false, true);
            }
        }

        /// <summary>
        /// versione di AttivaEsegui con log eventi, utilizzato per log eventi scadenze
        /// </summary>
        /// <param name="idScadenza"></param>
        public void AttivaEseguiScadenzaConLog(int idScadenza)
        {
            AttivaEsegui();
            if (this.Workflow.OperazioniEseguite != null)
                foreach (ZVOperazione itemOP in this.Workflow.OperazioniEseguite)
                    ZVDataLayer.InsertIntoScadenzeLog(idScadenza, Workflow.IdProcesso, itemOP);
        }

        public void EseguiRiepilogo()
        {
            Esegui(true, false);
        }

        public void EliminaSet(bool Elimina)
        {
            // inserisce true nella colonna Eliminato della tabella ZV_Azioni
            // tutte le viste escludono le azioni Eliminate

            ZVAzioniDS ZVAzioneds = this.GetDataSet(false, false);

            foreach (ZVAzioniDS.ZV_AzioniRow r in ZVAzioneds.ZV_Azioni.Rows)
            {

                r.Eliminato = Elimina;
                r.AcceptChanges();
                r.SetModified();

            }

            ZVDataLayer.AzioniAggiorna(ZVAzioneds);

        }

        // salva proprietà di workflow stato e azione a Run Time
        private void Esegui(bool riepilogo, bool collegata)
        {
            //l'azione di eliminazione definitiva non controlla altre modifiche
            if (!Workflow.Nuovo && !IsEliminazioneDefinitiva)
            {
                ZVProcessiDS prcds = ZVDataLayer.ProcessiLeggi(Workflow.IdProcesso);
                if (prcds.ZV_Processi.Rows.Count > 0)
                {
                    // Se il processo è già in fase di aggiornamento attivazione di 
                    // una exception e a carico dell'interfaccia decidere se e come 
                    // proseguire (Aspettare e riprovare o avvertire l'utente)
                    if ((prcds.ZV_Processi.Rows[0] as ZVProcessiDS.ZV_ProcessiRow).AzioneInCorso)
                    {
                        System.Threading.Thread.Sleep(3000);
                        /*  if ((prcds.ZV_Processi.Rows[0] as ZVProcessiDS.ZV_ProcessiRow).AzioneInCorso)
                              throw new ExceptionAzioneInCorso("Azione in corso"); */
                    }
                    // Se non in esecuzione controlla se lo stato è coerente con l'oggetto in memoria.
                    if ((Workflow.StatoCorrente.IdStato > 0) &&   // controllo solo se non è un nuovo stato
                    (prcds.ZV_Processi.Rows[0] as ZVProcessiDS.ZV_ProcessiRow).IdStatoCorrente != Workflow.StatoCorrente.IdStato)
                        throw new ExceptionWorkflowInconsistente("Workflow non piu coerente (Stato corrente modificato)");
                }
            }
            try
            {
                // aggiorna se non nuovo
                if (!Workflow.Nuovo)
                    ZVDataLayer.ProcessiAggiorna(Workflow.GetDataSet(false, true));

                Workflow.StatiInseriti = new List<ZVStato>();
                Workflow.AzioniInserite = new List<ZVAzione>();
                Workflow.OperazioniEseguite = new List<ZVOperazione>();
                Workflow.OperazioneInCorso = null;

                // Esegui azioni di apertura standard
                // Se azione non ancora eseguita o già eseguita nello stato stesso Stato
                // contesto modifica altrimenti contesto lettura 

                this.Stato.Inserisci();

                ZVOperazioniDS infoOperazioniAperturaStandard = null;
                ZVOperazioniDS infoOperazioniApertura = null;

                if (Workflow.Nuovo)
                {
                    this.Stato.AzioneImpostaStato.EseguiPrivate(true, false, this);
                    infoOperazioniImpostaStato = Stato.AzioneImpostaStato.InfoOperazioniEseguite;

                    // AB - 20150216: non va fatto in questo punto
                    //ImpostaScadenzeWorkflow();  // scadenza complessiva del workflow

                    ImpostaScadenzeStato();  // scadenza in primo stato
                }

                if (!IsAzioneStandard && !riepilogo && !collegata && !IsEliminazioneDefinitiva)
                {
                    if (Stato.AzioneAperturaStandard.Eseguibile(false))
                    {
                        this.Stato.AzioneAperturaStandard.EseguiPrivate(true, riepilogo, this);
                        infoOperazioniAperturaStandard = this.Stato.AzioneAperturaStandard.InfoOperazioniEseguite;
                    }
                    if (Stato.AzioneApertura != null && Stato.AzioneApertura.Eseguibile(false))
                    {
                        Stato.AzioneApertura.EseguiPrivate(true, riepilogo, this);
                        infoOperazioniApertura = Stato.AzioneApertura.InfoOperazioniEseguite;
                    }
                }
                //In caso di eliminazione definitiva non elabora le operazioni einserisce -1 come idStato Corrente 
                ZVProcessiDS ProcessoCorrente; //BUGFIX 20170728//  ZVProcessiDS ProcessoCorrente = Workflow.GetDataSet(false, false);
                ZVLogDS logDs = new ZVLogDS();
                if (!IsEliminazioneDefinitiva)
                {
                    EseguiPrivate(false, riepilogo, this);
                    ProcessoCorrente = Workflow.GetDataSet(false, false); //BUGFIX 20170728//
                }
                else
                {

                    ProcessoCorrente = Workflow.GetDataSet(false, false); //BUGFIX 20170728//
                    ZVProcessiDS.ZV_ProcessiRow row = ProcessoCorrente.ZV_Processi.Rows[0] as ZVProcessiDS.ZV_ProcessiRow;
                    row.IdStatoCorrente = -1;

                    //Aggiorna tabella ZVLog
                    ZVDataLayer.LogMessage(ref logDs, "Eliminazione definitiva processo", LogLvL.Info,
                                            this.Workflow.IdWorkflow, this.Workflow.IdProcesso, this.Workflow.StatoCorrente.IdStato,
                                            this.Workflow.Titolo, LogSrC.NetReport);

                    if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Session != null)
                    {
                        System.Web.HttpContext.Current.Session["ZVWorkflow_ElimDef"] = this.Workflow.IdProcesso.ToString();
                    }

                }

                //long tm = 0;
                //Stopwatch sp = new Stopwatch();
                //sp.Start();

                // Salva sono inviati al proxy i data set con le sole modifiche 
                ZVDataLayer.ProcessiAggiorna(
                        ProcessoCorrente,
                        (ZVCompetenzeDS)Workflow.Competenze.GetChanges(),
                        statoOrigine != null ? (ZVCompetenzeDS)statoOrigine.Competenze.GetChanges() : null,
                        (ZVCompetenzeDS)this.Competenze.GetChanges(),
                        (ZVCompetenzeDS)Workflow.StatoCorrente.Competenze.GetChanges(),
                        Workflow.ScadenzeNotNull ? (ZVScadenzeDS)Workflow.Scadenze.GetChanges() : null,
                        Workflow.ClassificazioneNotNull ? (ZVClassificazioneDS)Workflow.Classificazione.GetChanges() : null,
                        Workflow.EvidenzeNotNull ? (ZVEvidenzeDS)Workflow.Evidenze.GetChanges() : null,
                        statoOrigine != null ? statoOrigine.GetDataSet(false) : null,
                        Workflow.StatoCorrente.GetDataSet(false),
                        infoOperazioniAperturaStandard != null ? (ZVOperazioniDS)infoOperazioniAperturaStandard.GetChanges() : null,
                        infoOperazioniApertura != null ? (ZVOperazioniDS)infoOperazioniApertura.GetChanges() : null,
                        InfoOperazioniEseguite != null ? (ZVOperazioniDS)InfoOperazioniEseguite.GetChanges() : null,
                        infoOperazioniImpostaStato != null ? (ZVOperazioniDS)infoOperazioniImpostaStato.GetChanges() : null,
                        Workflow.InfoOperazioniWorkflowNotNull ? (ZVOperazioniDS)Workflow.GetAttributiWorkflowDataSet.GetChanges() : null,
                        Workflow.InfoOperazioniWorkflowNotNull ? Workflow.ClienteGetDataSet(!Workflow.IsClienteCodiceRiferimento()) : null,
                        logDs);

                //sp.Stop();
                //tm = sp.ElapsedMilliseconds;

                //sp.Reset();

                //sp.Start();

                if (Workflow.OperazioniDaSalvareInTabella.Count > 0 &&
                     infoOperazioniAperturaStandard != null &&
                     infoOperazioniAperturaStandard.GetChanges() != null)
                {
                    DataTable _elencoOp = infoOperazioniAperturaStandard.Tables["ZV_Operazioni"];
                    SaveToDbAdHoc(_elencoOp);
                }

                //sp.Stop();
                //tm = sp.ElapsedMilliseconds;
                Workflow.RefreshStato();

            }
            catch (Exception ex)
            {
                //// per errore attiva il metodo Annulla() per tutte le operazioni eseguite 
                //foreach (ZVOperazione operazione in Workflow.OperazioniEseguite)
                //    operazione.Annulla(); // L'operazione può annullare risultati esterni 
                //// Attiva eliminazione eventuali inserimenti se senza transazione                 
                //foreach (ZVAzione az in Workflow.AzioniInserite)
                //    az.Elimina();
                //foreach (ZVStato st in Workflow.StatiInseriti)
                //    st.Elimina();

                //// AB - 20170911: ripristino dello stato precedende all'azione
                ////Workflow.EliminaSeNuovo();
                //Workflow.EliminaSeNuovo(Stato.IdStato); // elimina se nuovo o sblocca se gia presente

                // ripristino
                StringBuilder errore = new StringBuilder("ERRORE ");

                if (Workflow.OperazioneInCorso != null)
                {
                    errore.AppendFormat("Operazione in corso: {0} - ", Workflow.OperazioneInCorso.Name);
                }
                errore.Append(ex.Message);
                throw new CustomNetReportException(Workflow.OperazioneInCorso, errore.ToString(), ex);
            }
            //finally
            //{

            //}
        }



        private void DtAddItem(ref DataTable _DtDati, string _NomeCampo, string _Valore)
        {
            DataRow _dr = _DtDati.NewRow();
            _dr["NomeCampo"] = Utility.SpecTo_(_NomeCampo);
            _dr["Valore"] = _Valore;
            _DtDati.Rows.Add(_dr);
        }

        private void SaveToDbAdHoc(DataTable ElencoOperazioni)
        {

            DataTable dtValoriDaSalvare = new DataTable();
            dtValoriDaSalvare.TableName = "DtCampoValore";
            dtValoriDaSalvare.Columns.Add("NomeCampo", typeof(string));
            dtValoriDaSalvare.Columns.Add("Valore", typeof(string));


            try
            {
                //long tm = 0;
                //Stopwatch sp = new Stopwatch();
                //sp.Start();

                foreach (DataRow _sRowOp in ElencoOperazioni.Rows)
                {
                    string _key = new StringBuilder().AppendFormat("{0}~{1}~", _sRowOp["NomeOperazione"], _sRowOp["Proprieta"]).ToString();
                    string _any = Workflow.OperazioniDaSalvareInTabella.Where(x => x.StartsWith(_key)).FirstOrDefault();

                    if (!string.IsNullOrEmpty(_any))
                    {
                        _key = _any;
                        string _ValoreA = _sRowOp["ValoreA"] != DBNull.Value ? _sRowOp["ValoreA"].ToString() : string.Empty;
                        string _ValoreB = _sRowOp["ValoreB"] != DBNull.Value ? _sRowOp["ValoreB"].ToString() : string.Empty;
                        string _ValoreN = _sRowOp["ValoreN"] != DBNull.Value ? _sRowOp["ValoreN"].ToString() : string.Empty;

                        switch (_sRowOp["Attributo"].ToString())
                        {

                            case "Double":
                                DtAddItem(ref dtValoriDaSalvare, _key, new StringBuilder(_ValoreN).Replace(".", "").Replace(",", ".").ToString());
                                break;

                            // case "Double": error
                            case "Int32":
                            case "enCreazioneDocumento": //ENUMS?
                            case "enTipoInformazioni": //ENUMS?
                                DtAddItem(ref dtValoriDaSalvare, _key, _ValoreN);
                                break;

                            case "DateTime":
                                ZVOperazione wfOperation = Workflow.OperazioniEseguite.Find(x => x.Name.Equals(_sRowOp["NomeOperazione"].ToString()));
                                if (wfOperation != null)
                                {
                                    Type t = wfOperation.GetType();
                                    if (t.GetProperty(_sRowOp["Proprieta"].ToString()) != null)
                                    {
                                        DateTime _dt = (DateTime)t.GetProperty(_sRowOp["Proprieta"].ToString()).GetValue(wfOperation, null);
                                        DtAddItem(ref dtValoriDaSalvare, _key, _dt.ToString("s"));
                                    }
                                }
                                break;

                            case "Boolean":
                            case "String":
                                DtAddItem(ref dtValoriDaSalvare, _key, _ValoreA);
                                break;

                            //---------------------------

                            case "Guid":
                                XmlDocument _xGuid = new XmlDocument();
                                _xGuid.LoadXml(_ValoreA);
                                DtAddItem(ref dtValoriDaSalvare, _key, _xGuid.GetElementsByTagName("guid")[0].InnerText);
                                break;

                            case "List`1":
                                StringBuilder _sList = new StringBuilder();
                                XmlDocument _xList = new XmlDocument();
                                _xList.LoadXml(_ValoreA);

                                foreach (XmlNode _child in _xList.GetElementsByTagName("ArrayOfString")[0].ChildNodes)
                                    //_sList += $"{_child.InnerText}; ";
                                    _sList.AppendFormat("{0}; ", _child.InnerText);

                                DtAddItem(ref dtValoriDaSalvare, _key, _sList.ToString());
                                break;

                            case "clsDatiAggiuntivi":
                                StringBuilder _sDtAgg = new StringBuilder();
                                XmlDocument _xDtAgg = new XmlDocument();
                                _xDtAgg.LoadXml(_ValoreA);

                                foreach (XmlNode _child in _xDtAgg.GetElementsByTagName("clsDatiAggiuntivi")[0].ChildNodes)
                                    //_sDtAgg += $"{_child.Name}: {_child.InnerText}; ";
                                    _sDtAgg.AppendFormat("{0}: {1}; ", _child.Name, _child.InnerText);

                                DtAddItem(ref dtValoriDaSalvare, _key, _sDtAgg.ToString());
                                break;


                            default:
                                DtAddItem(ref dtValoriDaSalvare, _key, _ValoreA);
                                break;

                        }
                    }
                }

                //sp.Stop();
                //tm = sp.ElapsedMilliseconds;

                //sp.Reset();

                //sp.Start();
                ZVDataLayer.PopolaDatiOperazioni(Workflow.IdWorkflow, Workflow.IdProcesso, dtValoriDaSalvare);
                //sp.Stop();

                //tm = sp.ElapsedMilliseconds;

            }
            catch (Exception ex)
            {
                if (ZVProxy<DataSet>.Proxy.CurrentParms.ActivateDebug)
                    Logging.writeLog(new StringBuilder("ZVAzione SaveToDbAdHoc Exception: ").Append(ex.Message).ToString(), ZVProxy<DataSet>.Proxy.CurrentParms.DebugFile);
            }
            //--------------------------------------------------------

        }

        [NonSerialized]
        ZVStato statoOrigine = null;   // variabili Data set processo corrente per Salva

        public ZVStato StatoOrigine
        {
            get { return statoOrigine; }
            set { statoOrigine = value; }
        }

        [NonSerialized]
        ZVOperazioniDS infoOperazioniImpostaStato = null;  // E SALVATO LO STATO 

        // AB - 20151229: mi salvo lo stato iniziale dell'azione
        [NonSerialized]
        ZVAzione azioneOrigine = null;   // variabili con lo stato dell'azione prime delle eventuali modifiche

        public void CambiaStato(ZVCambioStato operazioneCambioStato)
        {
            if (Nome_nuovo_stato == ZVCambioStato.NonCambiaStato || Nome_nuovo_stato == "")
                return;

            // AB - 20160216: al primo cambio stato creo la scedenza di workflow
            ImpostaScadenzeWorkflow();  // scadenza complessiva del workflow

            Workflow.CambioStatoInCorso = true;
            // Aggiornamento stato corrente con data ora di fine
            DateTime dt = DateTime.Now;
            Stato.DataFine = dt;  // data fine corrente e inizio nuovo
            statoOrigine = Stato; // Salva stato di Origine            

            operazioneCambioStato.NomeStatoPartenza = StatoOrigine.Name;
            operazioneCambioStato.IdStatoPartenza = StatoOrigine.IdStato;

            string nome = Nome_nuovo_stato;
            if (nome == ZVCambioStato.RitornaPrecedente)
                nome = Stato.NomeStatoPrec;
            Workflow.NomeStatoCorrente = nome;  // CambiaStato stato corrente
                                                // imposta nuovo stato
            Workflow.StatoCorrente.DataInizio = dt;  // data inizio inserisce fine precedente

            operazioneCambioStato.NomeStatoArrivo = Workflow.StatoCorrente.Name;
            operazioneCambioStato.IdStatoArrivo = Workflow.StatoCorrente.IdStato;

            Workflow.StatoCorrente.NomeStatoPrec = statoOrigine.Name;
            if (Workflow.StatoCorrente.Finale)   // Se ultimo stato del workflow
            {
                Workflow.StatoCorrente.DataFine = DateTime.Now; // data fine dello stato
                Workflow.DataFine = Workflow.StatoCorrente.DataFine;  // data fine workflow
                Workflow.Chiuso = true;
            }
            ImpostaScadenzeStato();
            Workflow.StatoCorrente.Inserisci();
            AggiornaScadenzeStato();
            Workflow.StatoCorrente.AzioneImpostaStato.StatoOrigine = statoOrigine;
            Workflow.StatoCorrente.AzioneImpostaStato.EseguiPrivate(true, false, this);
            infoOperazioniImpostaStato = Workflow.StatoCorrente.AzioneImpostaStato.InfoOperazioniEseguite;
            Workflow.CambioStatoInCorso = false;
        }

        private void ImpostaScadenzeWorkflow()
        {
            // AB - 20160216: Controllo se siamo nel primo cambio stato
            //if (Workflow.Nuovo && Stato.AzioneScadenza != null)
            if (Workflow.StatoCorrente.Iniziale && Stato.AzioneScadenza != null)
            {
                ZVAzione azwrk = Stato.AzioneScadenza;
                //MR - 20171129: non generare scadenze nel caso di azioni senza operazione ( escludendo le competenze di azione)
                if (azwrk.OperazioniAttive.FindAll(t => (t.GetType().Name != (ZVOperazione.ClasseCompetenzaAzione))).Count > 0)
                {
                    if (azwrk.NumeroGiorniScadenza > 0)
                    {
                        ZVScadenzeDS.ZV_ScadenzeRow row = Workflow.Scadenze.ZV_Scadenze.NewZV_ScadenzeRow();
                        row.IdProcesso = Workflow.IdProcesso;
                        row.IdStato = 0;
                        row.NomeAzione = ZVAzione.Scadenza;
                        row.DataScadenza = ZVCalcolaScadenza.Scadenza(azwrk.NumeroGiorniScadenza, Stato.AzioneScadenza.Lavorativo);
                        row.Elaborata = false;
                        row.StatoCorrente = false;
                        row.Manuale = azwrk.Manuale;
                        Workflow.Scadenze.ZV_Scadenze.AddZV_ScadenzeRow(row);
                        Workflow.DataScadenza = row.DataScadenza;

                    }
                }
            }
        }

        private void ImpostaScadenzeStato()
        {
            DateTime dtmin = DateTime.MaxValue;
            foreach (ZVAzione az in Workflow.StatoCorrente.Azioni)
            {
                //MR - 20171129: non generare scadenze nel caso di azioni senza operazione ( escludendo le competenze di azione)
                if (az.Name != ZVAzione.Scadenza && az.NumeroGiorniScadenza > 0 && az.Attivo && az.OperazioniAttive.FindAll(t => (t.GetType().Name != (ZVOperazione.ClasseCompetenzaAzione))).Count > 0)
                {
                    ZVScadenzeDS.ZV_ScadenzeRow row = Workflow.Scadenze.ZV_Scadenze.NewZV_ScadenzeRow();
                    row.IdProcesso = Workflow.IdProcesso;

                    // AB - 20160216: Gestione in stato iniziale
                    //row.IdStato = -1;  // non valorizzato cambiato dopo inserimento stato
                    row.IdStato = Workflow.StatoCorrente.IdStato;

                    row.NomeAzione = az.Name;
                    row.Manuale = az.Manuale;
                    row.DataScadenza = ZVCalcolaScadenza.Scadenza(az.NumeroGiorniScadenza, az.Lavorativo);
                    if (dtmin > row.DataScadenza)
                        dtmin = row.DataScadenza;
                    row.Elaborata = false;
                    row.StatoCorrente = false;
                    Workflow.Scadenze.ZV_Scadenze.AddZV_ScadenzeRow(row);
                }
            }
            if (dtmin != DateTime.MaxValue)
                Workflow.StatoCorrente.DataScadenza = dtmin;
        }

        private void AggiornaScadenzeStato()
        {
            foreach (ZVScadenzeDS.ZV_ScadenzeRow row in Workflow.Scadenze.ZV_Scadenze.Rows)
                // AB - 20160216: Solo i -1 devono essere riadattati.
                //if (row.IdStato < 0)
                if (row.IdStato == -1)
                    row.IdStato = Workflow.StatoCorrente.IdStato;  // non valorizzato
        }

        public void AnnullaEsegui()
        {
            this.Workflow.RefreshStato();
        }

        private void EseguiPrivate(bool automatica, bool riepilogo)
        {
            //compatibilità pregresso
            EseguiPrivate(automatica, riepilogo, null);
        }

        //  In dipendenza del contesto attiva le operazioni 
        private void EseguiPrivate(bool automatica, bool riepilogo, ZVAzione azione)
        {
            this.Inserisci(automatica);

            // Elabora le operazioni contenute nell'azione principale
            foreach (ZVOperazione operazione in riepilogo ? OperazioniRiepilogo : OperazioniAttive)
            {
                // il cambio stato è inclusivo e non è l'ultimo attore previsto
                // le operazioni sono eseguite solo fino a prima del cambio stato 
                if (operazione.GetType() == typeof(ZVCambioStato) && !CambioStatoCompleto)
                    break;
                EseguiOperazione(ContestoEsegui(false), operazione, azione);
            }

            // Eventuali evidenze con testo parametrico configurato nell'azione SOLO AD ESEGUI
            if (GeneraEvidenza && ContestoEsegui(false) == ContestoUtilizzo.Esegui
                && !String.IsNullOrEmpty(this.TestoEvidenza))
            {
                ZVEvidenzeDS.ZV_EvidenzeRow row = Workflow.Evidenze.ZV_Evidenze.NewZV_EvidenzeRow();
                row.IdProcesso = Workflow.IdProcesso;
                // sostituzione delle varibili di workflow e di azione (operazioni)
                row.Evidenza = ZVEvidenza.TestoEvidenza(this.TestoEvidenza, Workflow, this);
                Workflow.Evidenze.ZV_Evidenze.AddZV_EvidenzeRow(row);
            }
        }

        private void EseguiOperazione(ContestoUtilizzo contesto, ZVOperazione operazione)
        {
            //compatibilità pregresso
            EseguiOperazione(contesto, operazione, null);
        }

        private void EseguiOperazione(ContestoUtilizzo contesto, ZVOperazione operazione, ZVAzione azione)
        {
            //--AQ: Determino se l'operazione ha REQUISITO come proprità
            Type t = operazione.GetType();

            //bool _hasRequisito = false;
            //if (t.GetProperty("Requisito") != null && t.GetProperty("Requisito").GetValue(operazione, null) != null)
            //    _hasRequisito = true;
            //----

            // AB - 20170904: Vanno eseguite tutte le operazioni, anche quelle non attive perchè potr

            Workflow.OperazioneInCorso = operazione;

            //TODO AQ:20200402 verificare logica: se "non salvare", non dovrebbe salvare i datai di "apertura standard" e salvare quelli dell'azione comunque 
            if (azione != null)
            {
                if (!azione.Comportamento.Equals(enumComportamentoAzione.NonSalvare) ||
                    (azione.Comportamento.Equals(enumComportamentoAzione.NonSalvare) && t.Name.Equals("ZVCambioStato")) || //per eseguire cambio stato
                    (azione.Comportamento.Equals(enumComportamentoAzione.NonSalvare) && t.Name.Equals("ZVInvioMail")) || //per eseguire invio email
                    (azione.Comportamento.Equals(enumComportamentoAzione.NonSalvare) && t.Name.Equals("ZVOggetto")))  //forzatura per il campo oggetto che viene utilizzato come nome del workflow / processo
                    operazione.Esegui(contesto);
            }
            else
                operazione.Esegui(contesto);
            //------

            Workflow.OperazioniEseguite.Add(operazione);  // aggiungi nelle operazioni eseguite

            // AB - 20170905: Aggiunto controllo per evitare di salvare le operazione che non sono attive o visibili in base al contesto
            if ((operazione.CtrAttivo || operazione.CtrVisibile)
                // AB - 20180411: Aggiunto controllo per evitare di salvare le operazione con Id = -1
                && operazione.IdOperazione != -1
                // AB - 20190909: Aggiunto controllo per evitare il salvataggio delle operazioni che non devono essere salvate
                && !operazione.Comportamento.Equals(ZVOperazione.enumComportamentoOperazione.NonSalvare)
                //--AQ: forzo a vuoto l'item se il comportamento azione non permette il salvataggio
                && ((azione != null && !azione.Comportamento.Equals(enumComportamentoAzione.NonSalvare))

                //TODO AQ:20200402 verificare logica: se "non salvare", non dovrebbe salvare i datai di "apertura standard" e salvare quelli dell'azione comunque 
                || (azione != null && azione.Comportamento.Equals(enumComportamentoAzione.NonSalvare) && t.Name.Equals("ZVInvioMail"))) //TODO 20200402 forzatura da verificare

                )
            {
                List<ZVOperazione.Serializza> items = operazione.SerializzaAggiorna();

                foreach (ZVOperazione.Serializza item in items)
                {
                    DataRow[] rows = InfoOperazioniEseguite.ZV_Operazioni.Select(new StringBuilder().AppendFormat("IdOperazione ='{0}' AND Proprieta='{1}'", operazione.IdOperazione, item.Proprieta).ToString());
                    ZVOperazioniDS.ZV_OperazioniRow row;
                    if (rows.Length > 0)
                    {
                        row = (ZVOperazioniDS.ZV_OperazioniRow)rows[0];
                        // AB - 20160607: se l'item non è valorizzato, lo rimuovo
                        if (item.Vuoto)
                        {
                            row.Delete();
                            continue;
                        }
                    }
                    else
                    {
                        // AB - 20160607: se l'item non è valorizzato, non lo aggiungo
                        if (item.Vuoto)
                        {
                            continue;
                        }

                        row = InfoOperazioniEseguite.ZV_Operazioni.NewZV_OperazioniRow();
                        row.IdProcesso = Workflow.IdProcesso;
                        row.IdStato = StatoVirtualKey;
                        row.IdAzione = VirtualKey;
                        row.Attributo = item.Attributo;
                        row.IdOperazione = operazione.IdOperazione;
                        row.NomeOperazione = operazione.Name;
                        row.Proprieta = item.Proprieta;
                        InfoOperazioniEseguite.ZV_Operazioni.AddZV_OperazioniRow(row);
                    }

                    if (item.ValoreA != null)
                        row.ValoreA = item.ValoreA;
                    else
                        row.SetValoreANull();
                    row.ValoreN = item.ValoreN;
                    if (item.ValoreB != null)
                        row.ValoreB = item.ValoreB;
                    else
                        row.SetValoreBNull();

                }
            }

            Workflow.OperazioneInCorso = null;
        }

        public ZVAzione GetCopia()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            ms.Flush();
            ms.Position = 0;
            ZVAzione copia = ((ZVAzione)bf.Deserialize(ms));
            copia.Stato = this.Stato;
            foreach (ZVOperazione op in copia.Operazioni)
                op.Azione = copia;
            return copia;
        }

        public void InitAzione()
        {
            // Verifica Informazioni di persistenza presenti
            if (!Workflow.Nuovo)
                ApriAzione(idAzione);

            // AB - 20151229: mi salvo lo stato iniziale dell'azione
            if (this.Attivo)
                azioneOrigine = GetCopia();
        }

        // AB - 20151229: Resetto l'azione al suo stato iniziale
        public void ResetAzione()
        {
            // sovrascrivo tutte le operazioni dell'azione con i valori iniziali
            if (azioneOrigine != null)
                for (int i = 0; i < this.Operazioni.Count; i++)
                {
                    this.Operazioni[i] = azioneOrigine.Operazioni[i].GetCopia();
                    this.Operazioni[i].Azione = this;
                }
        }

        // Carica le operazioni dal data set storico.
        public void ApriAzione(int widAzione)
        {
            infoOperazioniEseguite = null;
            if (widAzione > 0)
            {
                ZVAzioniDS azioniDs = ZVDataLayer.AzioniLeggi(widAzione);
                if (azioniDs.ZV_Azioni.Rows.Count == 1)
                {
                    ZVAzioniDS.ZV_AzioniRow rowaz = azioniDs.ZV_Azioni.Rows[0] as ZVAzioniDS.ZV_AzioniRow;
                    IdAzione = rowaz.IdAzione;
                    dataAzione = rowaz.DataAzione;
                    azioneAutomatica = rowaz.AzioneAutomatica;
                    inModifica = rowaz.InModifica;
                    Eliminato = rowaz.Eliminato;
                }
            }

            // AB - 20181217: Riattivata ottimizzazione e aggiunto order by IdOperazione nelle query di popolamento di InfoOperazioniEseguite!!!!
            // AB - 20181205: Ripristinato vecchio giro per problemi con ordinamento operazioni in InfoOperazioniEseguite!!!!
            // AB - 20180918: Ottimizzazione del ciclo di deserializzazione delle operazioni
            //                Sfrutando il fatto che sia OperazioniAttive che InfoOperazioniEseguite.ZV_Operazioni sono ordinate in ordine crescente di IdOperazione,
            //                evito di ciclare ogni volta tutte le righe InfoOperazioniEseguite.ZV_Operazioni, ma solo quelle successive all'ultima IdOperazione trattata.
            /*
            foreach (ZVOperazione ope in this.OperazioniAttive)
            {
                List<ZVOperazione.Serializza> dati = new List<ZVOperazione.Serializza>();
                foreach (ZVOperazioniDS.ZV_OperazioniRow row in InfoOperazioniEseguite.ZV_Operazioni.Rows)
                {
                    if (ope.IdOperazione == row.IdOperazione ||
                        row.NomeOperazione.Length == 0)
                    {
                        ZVOperazione.Serializza item = new ZVOperazione.Serializza();
                        item.Proprieta = row.Proprieta;
                        if (!row.IsValoreANull())
                            item.ValoreA = row.ValoreA;
                        if (!row.IsValoreNNull())
                            item.ValoreN = row.ValoreN;
                        if (!row.IsValoreBNull())
                            item.ValoreB = row.ValoreB;
                        dati.Add(item);
                    }
                }

                ope.DeSerializza(dati);
            }
            */
            int idxRowOperazione = 0;
            foreach (ZVOperazione ope in this.OperazioniAttive)
            {
                List<ZVOperazione.Serializza> dati = new List<ZVOperazione.Serializza>();
                //foreach (ZVOperazioniDS.ZV_OperazioniRow row in InfoOperazioniEseguite.ZV_Operazioni.Rows)
                while (idxRowOperazione < InfoOperazioniEseguite.ZV_Operazioni.Rows.Count &&
                       (ope.IdOperazione == (InfoOperazioniEseguite.ZV_Operazioni.Rows[idxRowOperazione] as ZVOperazioniDS.ZV_OperazioniRow).IdOperazione ||
                        (InfoOperazioniEseguite.ZV_Operazioni.Rows[idxRowOperazione] as ZVOperazioniDS.ZV_OperazioniRow).NomeOperazione.Length == 0))
                {
                    ZVOperazioniDS.ZV_OperazioniRow row = InfoOperazioniEseguite.ZV_Operazioni.Rows[idxRowOperazione] as ZVOperazioniDS.ZV_OperazioniRow;
                    if (ope.IdOperazione == row.IdOperazione ||
                        row.NomeOperazione.Length == 0)
                    {
                        ZVOperazione.Serializza item = new ZVOperazione.Serializza();
                        item.Proprieta = row.Proprieta;
                        if (!row.IsValoreANull())
                            item.ValoreA = row.ValoreA;
                        if (!row.IsValoreNNull())
                            item.ValoreN = row.ValoreN;
                        if (!row.IsValoreBNull())
                            item.ValoreB = row.ValoreB;
                        dati.Add(item);
                    }

                    idxRowOperazione++;
                }

                ope.DeSerializza(dati);
            }

        }

        private ZVAzioniDS GetDataSet(bool elimina, bool automatica)
        {
            ZVAzioniDS ads = new ZVAzioniDS();
            ZVAzioniDS.ZV_AzioniRow row = ads.ZV_Azioni.NewZV_AzioniRow();
            row.IdStato = Stato.IdStato;
            row.IdAzione = IdAzione;
            row.DataAzione = dataAzione;
            row.DescrizioneAzione = Etichetta;
            row.NomeAzione = nomeAzione;
            row.AzioneAutomatica = automatica;
            row.Riepilogo = Riepilogo;
            row.InModifica = inModifica;
            row.Eliminato = eliminato;
            ads.ZV_Azioni.AddZV_AzioniRow(row);
            return ads;
        }

        #endregion

        #region Proprieta di processo salvate a run time
        [NonSerialized]
        ZVStato stato;
        public ZVStato Stato
        {
            get { return stato; }
            set { stato = value; }
        }

        public ZVWorkflow Workflow   // workflow di appartenenza (sempre presente sia a run time che a design)
        {
            get { return stato.Workflow; }
        }

        [NonSerialized]
        ZVOperazioniDS infoOperazioniEseguite;
        bool inModifica = false;
        public bool InModifica
        {
            get { return inModifica; }
            set { inModifica = value; }
        }

        bool eliminato = false;
        public bool Eliminato
        {
            get { return eliminato; }
            set { eliminato = value; }
        }
        public ZVOperazioniDS InfoOperazioniEseguite
        {
            get
            {
                if (infoOperazioniEseguite != null)
                    return infoOperazioniEseguite;

                if (Workflow.Nuovo)
                {
                    infoOperazioniEseguite = new ZVOperazioniDS();
                    return infoOperazioniEseguite;
                }

                switch (Persistenza)
                {
                    //  Azione:L'azione è indipendente, ogni volta che premo il pulsante è un nuovo inserimento 
                    // Ricerca per :IdProcesso,IdStato,IdAzione
                    case TipoPersistenza.Azione:
                        {
                            if (idAzione < 0)
                                infoOperazioniEseguite = new ZVOperazioniDS();
                            else
                                infoOperazioniEseguite = ZVDataLayer.OperazioniLeggiByIdAzione(Workflow.IdProcesso, idAzione);
                            break;
                        }
                    // Stato in Corso:L'azione visualizza e aggiorna i dati della stessa azione solo finche sono nello stato corrente;
                    // IdProcesso,IdStato,NomeAzione
                    case TipoPersistenza.StatoCorrente:
                        {
                            infoOperazioniEseguite = ZVDataLayer.OperazioniLeggiByIdStatoNomeAzione(Workflow.IdProcesso, Stato.IdStato, this.Name);
                            break;
                        }
                    // Stato:L'azione visualizza e aggiorna i dati della stessa azione per lo stesso stato (anche se sono uscito e rientrato)	Corrente	
                    // IdProcesso,NomeStato,NomeAzione
                    case TipoPersistenza.Stato:
                        {
                            infoOperazioniEseguite = ZVDataLayer.OperazioniLeggiByNomeStatoNomeAzione(Workflow.IdProcesso, Stato.Name, this.Name);
                            break;
                        }
                    // Workflow: L'azione visualizza e aggiorna i dati della stessa azione per lo stesso workflow	
                    // IdProcesso,NomeAzione
                    case TipoPersistenza.Workflow:
                        {
                            infoOperazioniEseguite = ZVDataLayer.OperazioniLeggiByNomeAzione(Workflow.IdProcesso, this.Name);
                            break;
                        }
                    // Workflow: L'azione visualizza e aggiorna i dati della stessa azione per lo stesso utente	
                    // IdProcesso,Utente,NomeAzione
                    case TipoPersistenza.Utente:
                        {
                            // AB - 20180605: Aggiunta condizione per evitare la persistenza di azione in caso riepilogo
                            if (azioneRiepilogo == enumAzioneRiepilogo.Lettura)
                                if (idAzione < 0)
                                    // Evito di eseguire la query
                                    infoOperazioniEseguite = new ZVOperazioniDS();
                                else
                                    infoOperazioniEseguite = ZVDataLayer.OperazioniLeggiByIdAzione(Workflow.IdProcesso, idAzione);
                            else
                                infoOperazioniEseguite = ZVDataLayer.OperazioniLeggiByUtenteNomeAzione(Workflow.IdProcesso, ZVWorkflow.CodiceUtente.Trim(), this.Name);
                            break;
                        }
                }
                if (infoOperazioniEseguite.ZV_Operazioni.Rows.Count == 0)
                {
                    inModifica = false;
                }
                else
                {
                    inModifica = true;
                    StatoVirtualKey = (infoOperazioniEseguite.ZV_Operazioni.Rows[0] as ZVOperazioniDS.ZV_OperazioniRow).IdStato;
                    VirtualKey = (infoOperazioniEseguite.ZV_Operazioni.Rows[0] as ZVOperazioniDS.ZV_OperazioniRow).IdAzione;
                }
                return infoOperazioniEseguite;
            }
        }

        public void Remove(IZVEngineObject child)
        {
            ZVOperazione ope = (ZVOperazione)child;
            ope.Azione = null;
            Operazioni.Remove(ope);
            if (_drawer != null)
            {
                _drawer.Controls.Remove(child.Drawer);
                Drawer.ReSize();
            }
        }

        #endregion

        #region Proprieta salvate a design

        private string derivaDaStato;
        public string DerivaDaStato
        {
            get { return derivaDaStato; }
            set { derivaDaStato = value; }
        }

        public virtual bool Separatore
        {
            get { return false; }
        }

        private TipoPersistenza persistenza = TipoPersistenza.Azione;
        public TipoPersistenza Persistenza
        {
            get { return persistenza; }
            set { persistenza = value; }
        }

        string nome_nuovo_stato = "";
        public string Nome_nuovo_stato
        {
            get { return nome_nuovo_stato; }
            set { nome_nuovo_stato = value; }
        }

        private string nomeAzDuplicata = "";
        public string NomeAzDuplicata
        {
            get { return nomeAzDuplicata; }
            set { nomeAzDuplicata = value; }
        }

        private bool generaEvidenza;
        public bool GeneraEvidenza
        {
            get { return generaEvidenza; }
            set { generaEvidenza = value; }
        }
        private bool cambioStatoInclusivo;
        public bool CambioStatoInclusivo
        {
            get { return cambioStatoInclusivo; }
            set { cambioStatoInclusivo = value; }
        }

        private string testoEvidenza;
        public string TestoEvidenza
        {
            get { return testoEvidenza; }
            set { testoEvidenza = value; }
        }

        protected string nomeAzione;
        public string Name
        {
            get { return nomeAzione; }
            set { nomeAzione = value; }
        }



        private bool attivo = true;
        public bool Attivo
        {
            get
            {
                if (stato != null && !stato.Attivo)
                {
                    return false;
                }
                else
                {
                    if (Nome_nuovo_stato != ZVCambioStato.NonCambiaStato && Nome_nuovo_stato != "")
                    {
                        ZVStato st = Workflow.StatoDaNome(nome_nuovo_stato);
                        if (st != null && !st.Attivo)
                            return false;
                    }
                    return attivo;
                }
            }
            set
            {
                attivo = value;
            }
        }

        private string icona;
        public string Icona
        {
            get { return icona; }
            set { icona = value; }
        }

        private string iconaWeb;
        public string IconaWeb
        {
            get { return iconaWeb; }
            set { iconaWeb = value; }
        }

        private string coloreWeb;
        public string ColoreWeb
        {
            get { return coloreWeb; }
            set { coloreWeb = value; }
        }

        private bool nascosta;
        public bool Nascosta
        {
            get { return nascosta; }
            set { nascosta = value; }
        }

        private string etichetta;
        public virtual string Etichetta
        {
            get
            {
                if (etichetta == null)
                    return nomeAzione;
                return etichetta;
            }
            set { etichetta = value; }
        }

        private bool duplicabile;
        public bool Duplicabile
        {
            get { return duplicabile; }
            set { duplicabile = value; }
        }

        private bool chiediConferma;
        public bool ChiediConferma
        {
            get { return chiediConferma; }
            set { chiediConferma = value; }
        }

        public string AzioneCollegata
        {
            get { return stato.AzioneCollegata; }
            set { stato.AzioneCollegata = value; }
        }

        //******************
        [NonSerialized]
        private System.Drawing.Font font = null;
        public System.Drawing.Font Font
        {
            get { return font; }
            set { font = value; }
        }
        [NonSerialized]
        private System.Drawing.Color backColor = System.Drawing.SystemColors.Control;
        public System.Drawing.Color BackColor
        {
            get { return backColor; }
            set { backColor = value; }
        }
        //******************

        private List<ZVOperazione> operazioni = new List<ZVOperazione>();
        /// <summary>
        /// Lista di tutte le operazioni.
        /// </summary>
        public List<ZVOperazione> Operazioni
        {
            get
            {
                return operazioni;
            }
            set { operazioni = value; }
        }

        /// <summary>
        /// Lista delle operazioni utilizzabili in riepilogo.
        /// </summary>
        public List<ZVOperazione> OperazioniRiepilogo // solo operazioni per riepilogo
        {
            get
            {
                List<ZVOperazione> opat = new List<ZVOperazione>();
                foreach (ZVOperazione ope in Operazioni)
                    if (ope.Attivo && ope.Riepilogo)
                        opat.Add(ope);
                return opat;
            }
        }

        /// <summary>
        /// Lista delle operazioni utilizzabili in design.
        /// </summary>
        public List<ZVOperazione> OperazioniDrawing  // solo operazioni disegnabili
        {
            get
            {
                List<ZVOperazione> opat = new List<ZVOperazione>();
                foreach (ZVOperazione ope in Operazioni)
                    if (!ope.IsOperazioneCompetenza || ope.GetType().Name == ZVOperazione.ClasseCompetenzaWorkflow)
                        opat.Add(ope);
                return opat;
            }
        }

        /// <summary>
        /// Lista delle operazioni attive
        /// </summary>
        public List<ZVOperazione> OperazioniAttive  // operazioni da eseguire 
        {
            get
            {
                List<ZVOperazione> opat = new List<ZVOperazione>();
                foreach (ZVOperazione ope in Operazioni)
                    if (ope.Attivo)
                    {
                        opat.Add(ope);
                        if (ope.GetType() == typeof(ZVCambioStato))
                        {
                            if (!CambioStatoCompleto)
                                return opat;
                        }
                    }
                return opat;
            }
        }

        /// <summary>
        /// Lista delle operazioni utilizzabili per la ricerca.
        /// </summary>
        public List<ZVOperazione> OperazioniRicerca
        {
            get
            {
                List<ZVOperazione> opat = new List<ZVOperazione>();
                foreach (ZVOperazione ope in OperazioniAttive)
                {
                    foreach (Attribute attr in ope.GetType().GetCustomAttributes(false))
                    {
                        // verifico se l'operazione ha l'attributo ricerca.
                        if (attr.GetType() == typeof(AttrRicerca))
                        {
                            opat.Add(ope);
                            break;
                        }
                    }
                }

                return opat;
            }
        }

        /// <summary>
        /// Lista delle operazioni utilizzabili per l'analisi.
        /// </summary>
        public List<ZVOperazione> OperazioniAnalisi
        {
            get
            {
                List<ZVOperazione> opat = new List<ZVOperazione>();
                foreach (ZVOperazione ope in OperazioniAttive)
                {
                    foreach (Attribute attr in ope.GetType().GetCustomAttributes(false))
                    {
                        // verifico se l'operazione ha l'attributo ricerca.
                        if (attr.GetType() == typeof(AttrAnalisi))
                        {
                            opat.Add(ope);
                            break;
                        }
                    }
                }

                return opat;
            }
        }

        private bool riepilogo = false;
        public bool Riepilogo
        {
            get { return riepilogo; }
            set { riepilogo = value; }
        }
        private bool eliminaRunTime = false;
        public bool EliminaRunTime
        {
            get { return eliminaRunTime; }
            set { eliminaRunTime = value; }
        }

        private bool azioneAutomatica = false;
        public bool AzioneAutomatica
        {
            get { return azioneAutomatica; }
            set { azioneAutomatica = value; }
        }
        #endregion

    }

    public class ExceptionAzioneInCorso : Exception
    {
        public ExceptionAzioneInCorso(string message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ExceptionWorkflowInconsistente : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ExceptionWorkflowInconsistente(string message)
            : base(message)
        {
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ZVSeparatore : ZVAzione
    {
        /// <summary>
        /// 
        /// </summary>
        public ZVSeparatore()
        {
            nomeAzione = "ZVSeparatore";
            Eliminabile = true;
        }
        /// <summary>
        /// 
        /// </summary>
        public override bool Separatore
        {
            get { return true; }
        }
    }
}
