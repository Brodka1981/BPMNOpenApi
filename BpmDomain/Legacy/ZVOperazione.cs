using System.Collections;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ZV_Engine
{
    /// <summary>
    /// Classe che definisce le caratteristiche base delle operazioni.
    /// </summary>
    [Serializable]
    public abstract class ZVOperazione : ZV_Engine.IZVEngineObject
    {

        #region Definizioni
        [field: NonSerialized]
        public event ZVErrorEventHandler ErroriEvent;

        public delegate void ZVErrorEventHandler(object sender, List<ZVErrore> errori);

        public const string ClasseCambioStato = "ZVCambioStato";  //"Cambio stato";
        public const string ClasseCompetenzaWorkflow = "ZVCompetenzaWorkflow";  //"Competenze";
        public const string ClasseCompetenzaStato = "ZVCompetenzaStato";        //"CompetenzeStato";
        public const string ClasseCompetenzaAzione = "ZVCompetenzaAzione";      //CompetenzeAzione";
        //MR 05/06/2019 commentato per operazione controllo Formale inesistente
        //public const string ClasseOperazioneControllo = "ZVControlloFormale";   // "OperazioneControllo";
        public const string ClasseOperazioneStampa = "ZVStampa";
        public const string ClasseOperazioneVisualizzaPdf = "ZVVisualizzaPdf";
        public const string ClasseOperazioneStampaScaricaAllegaWord = "ZVStampaScaricaAllegaWord";
        public const string ClasseOperazioneRedazioneWord = "ZVRedazioneWord";
        public const string ClasseOperazionePaginaWeb = "ZVPaginaWeb";
        public const string ClasseOperazioneStampaCuspro = "ZVStampaCusPro";
        // AB - 20191003: Aggiunte nuove costanti
        public const string ClasseOperazioneCampoTabella = "ZVCampoTabella";
        public const string ClasseOperazioneCampoTabellaEstesa = "ZVCampoTabellaEstesa";
        public const string ClasseOperazioneCampoTesto = "ZVCampoTesto";

        // AB - 20150623: come mai hai aggiunto questa costante??? Non è definita per tutte le operazioni...
        //public const string ClasseOperazioneAssegnaProtocollo = "ZVNumeroProtocollo";   // "OperazioneAssegnaProtocollo";

        public const string ClasseOggettoContesto = "";        // Contesto esterno

        // AB - 20160225: Parametri di Criptografia
        private const String SaltValue = "71210c410";
        private const String HashAlgorithm = "SHA1";
        private const int PasswordIterations = 2;
        private const String InitVector = "9???Z?)IY???:??b";
        private const int KeySize = 128;

        public struct TipoSpParameter
        {
            public const string TESTO = "Testo";
            public const string DATA = "Data";
            public const string NUMERO = "Numero";
        }

        /// <summary>
        /// Tipi di comportamento dell'operazione
        /// </summary>
        public enum enumComportamentoOperazione
        {
            /// Normale
            Normale,
            /// Operazione i cui valori non devono essere salvati in DB 
            NonSalvare
        }

        #endregion

        #region Metodi Attivati a design

        /// <summary>
        /// Ritorna il controllo per il Design.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <returns>Controllo per il Design.</returns>
        public virtual Control ControlDesign(ContestoUtilizzo contesto)
        {
            return null;
        }

        /// <summary>
        /// Operazioni aggiuntive da eseguire alla conferma della modifica di un'operazione lato Design(ad esempio: alvare le informazioni esterne all'operazione). 
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        public virtual void ConfermaDesign(ContestoUtilizzo contesto)
        {

        }

        /// <summary>
        /// Controlla la corretta configurazione dell'operazione
        /// </summary>
        /// <param name="contesto">Contesto di utlizzo.</param>
        public virtual void ControlloFormale(ContestoUtilizzo contesto)
        {
            // il default è che la validazione abbia successo
        }

        /// <summary>
        /// Eventiuali aggiornamenti esterni da eseguire in caso di controllo formale corretto sull'operazione.
        /// </summary>
        /// <param name="contesto">Contesto di utizzo.</param>
        /// <param name="delete"></param>
        public virtual void AggiornaDefinizione(ContestoUtilizzo contesto, bool delete)
        {
            // Workflow.IdWorkflow
        }

        /// <summary>
        /// Allinea una determinata operazione di workflow con le modifiche apportate alla stessa nel template da cui deriva.
        /// </summary>
        /// <param name="operazioneTemplate">Operazione da allineare.</param>
        public virtual void AggiornaWorkflowDaTemplate(ZVOperazione operazioneTemplate)
        {
            // Es. this.proprieta=operazioneTemplate.proprieta
        }

        #endregion
        #region Metodi Attivati a run time
        /// <summary>
        /// E' attivato alla creazione di un nuovo processo per utte le operazioni 
        /// presenti nel workflow (impostazioni da fare una sola volta all'inizio)
        /// </summary>
        //public virtual void NuovoProcesso()  // Maurizio commentato 09/03/
        //{
        //}

        /// <summary>
        /// Ritorna true se l'operazione è eseguibile nel contesto specificato, false altrimenti.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <returns>True se l'operazione è eseguibile nel contesto specificato, false altrimenti.</returns>
        public virtual bool Eseguibile(ContestoUtilizzo contesto)
        {
            return true;
        }

        /// <summary>
        /// Ritorna il controllo per l'esecuzione in ambiente WindowsForm.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <param name="interfacciaEditabile">Assume true se il controllo è editabile, false altrimenti.</param>
        /// <returns>Controllo per l'esecuzione in ambiente WindowsForm.</returns>
        public virtual Control ControlRuntime(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        {
            interfacciaEditabile = false;
            return null;
        }

        /// <summary>
        /// Ritorna il controllo per l'esecuzione in ambiente Web.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <param name="interfacciaEditabile">Assume true se il controllo è editabile, false altrimenti.</param>
        /// <returns>Controllo per l'esecuzione in ambiente Web.</returns>
        public virtual WebControl ControlRuntimeWeb(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        {
            interfacciaEditabile = false;
            return null;
        }
        // Attivato dopo conferma dell'interfaccia utente 
        // Effettuare gli aggiornamenti dell'operazione diversi dai campi ATTRSalva
        // gestiti autoamaticamante
        /// <summary>
        /// Esegue, durante l'esecuzione di un'azione, attivtà di aggiornamento delle proprietà dell'operazione che non presentano l'attributo AttrSalva.
        /// </summary>
        /// <param name="contesto">Contesto di utlizzo.</param>
        public virtual void Esegui(ContestoUtilizzo contesto)
        {
        }

        // Attivato a tutte le operazioni per le quali era stato attivato l'esegui in caso 
        // di interruzione del programma
        /// <summary>
        /// Annulla, in caso di errore nell'esecuzione di un'azione, tutti gli effetti dell'eventuale funzione Esegui(...) precedentemente lanciata.
        /// </summary>
        public virtual void Annulla()
        {
            //Aggiorna tabella ZVLog
            ZVLogDS logDs = null;
            ZVDataLayer.LogMessage(ref logDs, $"Operazione '{Name}' annullata in seguito ad errore", ZVAzione.LogLvL.Warning,
                                    Workflow.IdWorkflow, Workflow.IdProcesso, Stato.IdStato, Azione.IdAzione, IdOperazione,
                                      Workflow.Titolo, ZVAzione.LogSrC.NetReport);
        }

        bool isDesign = false;
        // Obsoleto esistono i due metodi di design e run time
        // si consiglia di usare solo nel caso il control a run time e a desing è lo stesso  
        public virtual Control Control(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        {
            interfacciaEditabile = true;
            if (contesto == ContestoUtilizzo.Template || contesto == ContestoUtilizzo.Workflow)
            {
                isDesign = true;
                return ControlDesign(contesto);
            }
            isDesign = false;
            return ControlRuntime(contesto, out interfacciaEditabile);
        }

        /*public virtual WebControl ControlWeb(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        {
            interfacciaEditabile = true;
            isDesign = false;
            return ControlRuntimeWeb(contesto, out interfacciaEditabile);
        }*/

        // AB - 20180412: Metodi per la gestione del formato json del valore dell'operazione
        #region Metodi per la gestione del formato json del valore dell'operazione

        /// <summary>
        /// Tipo di elaborazione dalla quale sarà consultato i valore json dell'operazione. 
        /// </summary>
        public enum TipoElaborazione
        {
            /// <summary>
            /// Analisi di workfoow.
            /// </summary>
            Analisi,

            /// <summary>
            /// Ricerca salvate.
            /// </summary>
            Ricerca
        }

        /// <summary>
        /// Ritorna il valore dell'operazione in formato json.
        /// </summary>
        /// <param name="tipoElaborazione">Tipo di elaborazione dalla quale sarà consultato i valore json dell'operazione.</param>
        /// <returns>Valore dell'operazione in formato json.</returns>
        public virtual string ValoreJson(TipoElaborazione tipoElaborazione)
        {
            string valore = "";
            if (Criptato)
                valore = "***Criptato***";

            return valore;
        }

        /// <summary>
        /// Ritorna il tipo del valore dell'operazione in formato json.
        /// </summary>
        /// <param name="tipoElaborazione">Tipo di elaborazione dalla quale sarà consultato i valore json dell'operazione.</param>
        /// <returns>Tipo del valore dell'operazione in formato json.</returns>
        public virtual string TipoJson(TipoElaborazione tipoElaborazione)
        {
            string tipo = TipiJson.TipoString;
            if (Criptato)
                tipo = TipiJson.TipoCrypt;

            return tipo;
        }

        /// <summary>
        /// Ritorna il tipo json corrispondente al Type specificato.
        /// </summary>
        /// <param name="type">Type da trattare.</param>
        /// <returns>Tipo json corrispondente al Type specificato.</returns>
        public static string GetTipoJson(Type type)
        {
            switch (type.FullName)
            {
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                    return TipiJson.TipoInt;
                case "System.Decimal":
                case "System.Double":
                    return TipiJson.TipoFloat;
                case "System.DateTime":
                    return TipiJson.TipoDate;
                case "System.Boolean":
                    return TipiJson.TipoBool;
                case "System.String":
                    return TipiJson.TipoString;
            }

            return TipiJson.TipoObject;
        }

        /// <summary>
        /// Tipi di valore json.
        /// </summary>
        public struct TipiJson
        {
            /// <summary>
            /// Tipo string
            /// </summary>
            public const string TipoString = "string";
            /// <summary>
            /// Tipo date
            /// </summary>
            public const string TipoDate = "date";
            /// <summary>
            /// Tipo date time
            /// </summary>
            public const string TipoDateTime = "datetime";
            /// <summary>
            /// Tipo number
            /// </summary>
            public const string TipoNumber = "number";
            /// <summary>
            /// Tipo float
            /// </summary>
            public const string TipoFloat = "float";
            /// <summary>
            /// Tipo int
            /// </summary>
            public const string TipoInt = "int";
            /// <summary>
            /// Tipo bool
            /// </summary>
            public const string TipoBool = "bool";
            /// <summary>
            /// Tipo crypt
            /// </summary>
            public const string TipoCrypt = "crypt";
            /// <summary>
            /// Tipo object
            /// </summary>
            public const string TipoObject = "object";
        }

        #endregion

        /// <summary>
        /// Restituisce i testi di base contenuti nel design (utilizzato in ricerca)
        /// </summary>
        public virtual string TestiContenutiNelDesign()
        {
            StringBuilder OpSTRINGs = new StringBuilder();
            OpSTRINGs.AppendFormat("{0} ", Name ?? string.Empty);
            OpSTRINGs.AppendFormat("{0} ", Label ?? string.Empty);
            OpSTRINGs.AppendFormat("{0} ", HelpTooltip ?? string.Empty);

            // estrazione di ValoreJson da rivedere, fornisce risultati non necessari
            // come il nome dei campi json che falsano il risultato
            // OpSTRINGs.AppendFormat("{0} ", ValoreJson(TipoElaborazione.Analisi) ?? string.Empty);

            return OpSTRINGs.ToString();
        }


        #endregion



        #region Gestione errori
        public virtual bool Errore
        {
            get { return Errori.Count > 0; }
        }

        [NonSerialized]
        private List<ZVErrore> errori;
        public List<ZVErrore> Errori
        {
            get
            {
                if (errori == null)
                    errori = new List<ZVErrore>();
                return errori;
            }

        }

        public virtual void SetErrore(string descrizione, bool condizione)
        {
            if (condizione)
            {
                AddErrore((ControlOpEseguiWeb as WebControl).ID, descrizione);

            }
            else
                RemoveErrore(descrizione);
        }

        public virtual void AddErrore(object sender, string descrizione)
        {
            AddErrore(new ZVErrore(sender, descrizione, ZVTipoErroreEnum.Errore));
        }

        public virtual void AddMessaggio(object sender, string descrizione)
        {
            AddErrore(new ZVErrore(sender, descrizione, ZVTipoErroreEnum.Messaggio));
        }
        // Interrompe la sequenza dell'azione in corso
        public virtual void AddCancellazione()
        {
            AddErrore(new ZVErrore(null, "", ZVTipoErroreEnum.Cancellazione));
        }
        bool erroriControlloFormale = false;
        public bool ErroriControlloFormale
        {
            get { return erroriControlloFormale; }
            set { erroriControlloFormale = value; }
        }

        public virtual void AddErrore(ZVErrore errore)
        {
            // AB - 201507014: rimosso perchè non serve
            //if (ErroriEvent == null && !ZVWorkflow.WorkflowIsWeb)
            //    return;

            string msgErrore;
            if (ErroriControlloFormale)
            {
                //msgErrore = Stato.Name + "-" + Azione.Name + "-" + Name + ": " + errore.Messaggio;
                msgErrore = new StringBuilder().AppendFormat("{0}-{1}-{2}: {3}", Stato.Name, Azione.Name, Name, errore.Messaggio).ToString();
                errore.Messaggio = msgErrore;
                Workflow.AddErrore(errore);
                return;
            }

            string sOperLabel = (string.IsNullOrEmpty(Label)) ? Name : Label;

            int charLocation = sOperLabel.IndexOf("\n", StringComparison.Ordinal);

            if (charLocation > 0)
            {
                sOperLabel = sOperLabel.Substring(0, charLocation);
            }

            msgErrore = new StringBuilder((isDesign ? "" : new StringBuilder(sOperLabel).Append(": ").ToString())).Append(errore.Messaggio).ToString();
            //OLDCODE msgErrore = (isDesign ? "" : Name) + errore.Messaggio;
            // \gdc
            errore.Messaggio = msgErrore;
            bool trovato = false;
            foreach (ZVErrore er in Errori)
                if (er.Sender.Equals(errore.Sender) && er.Messaggio == errore.Messaggio)
                    trovato = true;
            if (!trovato)
                Errori.Add(errore);
            if (ErroriEvent != null)
                ErroriEvent(this, Errori);
        }

        // Cancella tutti gli errori presenti con la descrizione inviata
        public virtual void RemoveErrore(string messaggio)
        {
            if (ErroriEvent == null && !ZVWorkflow.WorkflowIsWeb)
                return;
            string msgErrore;
            bool cancellata = false;
            if (ErroriControlloFormale)
            {
                //msgErrore = Stato.Name + "-" + Azione.Name + "-" + Name + ": " + messaggio;
                msgErrore = new StringBuilder().AppendFormat("{0}-{1}-{2}: {3}", Stato.Name, Azione.Name, Name, messaggio).ToString();
                Workflow.RemoveErrore(msgErrore);
                return;
            }
            //gdc: modificato il messaggio di errore (con il vecchio codice il nome della operazione e il messaggio erano attaccati senza spazi)
            msgErrore = new StringBuilder((isDesign ? "" : new StringBuilder(Name).Append(": ").ToString())).Append(messaggio).ToString();
            //OLDCODE msgErrore = (isDesign ? "" : Name) + messaggio;
            // \gdc
            messaggio = msgErrore;
            List<ZVErrore> remove = new List<ZVErrore>();
            foreach (ZVErrore er in Errori)
                if (er.Messaggio == messaggio)
                {
                    remove.Add(er);
                    cancellata = true;
                }
            foreach (ZVErrore er in remove)
                Errori.Remove(er);

            if (!cancellata) return;

            if (ErroriEvent != null)
                ErroriEvent(this, Errori);
        }

        public virtual void ClearErrori()
        {
            if (ErroriEvent == null && !ZVWorkflow.WorkflowIsWeb)
                return;
            if (ErroriControlloFormale)
            {
                Workflow.ClearErrori();
                return;
            }
            Errori.Clear();
            if (ErroriEvent != null)
                ErroriEvent(this, Errori);
        }

        #endregion

        #region Visibilita struttura workflow
        [NonSerialized]
        protected ZVAzione azione;
        public virtual ZVAzione Azione
        {
            set { azione = value; }
            get { return azione; }
        }

        public ZVStato Stato
        {
            get { return Azione.Stato; }
        }

        public ZVWorkflow Workflow
        {
            get { return Azione.Workflow; }
        }
        #endregion

        #region Altro

        /// <summary>
        /// Controllo dell'operazione che contiene effettivamente il valore della stessa in modalità windows form.
        /// Esempio: TextBox, ComboBox, ecc...
        /// </summary>


        /// <summary>
        /// Assume true se l'operazione è di competenza, false altrimenti.
        /// </summary>
        public bool IsOperazioneCompetenza
        {
            get
            {
                return this.GetType().Name == ZVOperazione.ClasseCompetenzaWorkflow ||
                       this.GetType().Name == ZVOperazione.ClasseCompetenzaStato ||
                       this.GetType().Name == ZVOperazione.ClasseCompetenzaAzione;
            }
        }

        /// <summary>
        /// Assume true se l'operazione in stampa viene trattata come un bookmark, false altrimenti.
        /// </summary>
        public bool IsOperazioneConBookmarkInStampa
        {
            get
            {
                return this.GetType().Name == ZVOperazione.ClasseOperazioneCampoTabella ||
                       this.GetType().Name == ZVOperazione.ClasseOperazioneCampoTabellaEstesa ||
                       (this.GetType().Name == ZVOperazione.ClasseOperazioneCampoTesto && Convert.ToInt32(this.GetType().GetProperty("NumeroRighe").GetValue(this, null)) > 1);
                /*
                return (this.GetType().Name.Contains("Tabella") || 
                        (this.GetType().Name.Contains("Testo") && !this.GetType().Name.Contains("CKEditor")) &&
                            Convert.ToInt32(this.GetType().GetProperty("NumeroRighe").GetValue(this, null)) > 1);
                 */
            }
        }

        /// <summary>
        /// Assume true se l'operazione è di analisi, false altrimenti.
        /// </summary>
        public bool IsOperazioneAnalisi
        {
            get
            {
                foreach (Attribute attr in this.GetType().GetCustomAttributes(false))
                {
                    // verifico se l'operazione ha l'attributo ricerca.
                    if (attr.GetType() == typeof(AttrAnalisi))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Ritorna true se il testo specificato è in formato HTML, false altrimenti.
        /// </summary>
        /// <param name="textToCheck">Testo da controllare.</param>
        /// <returns>True se il testo specificato è in formato HTML, false altrimenti.</returns>
        public bool IsTextHTML(string textToCheck)
        {
            try
            {
                //considero html se contiene tag <> ed inizia con <
                if (!string.IsNullOrEmpty(textToCheck))
                    return Regex.IsMatch(textToCheck, "^<(.|\n)*?>");
                //       return Regex.IsMatch(textToCheck, "^<(.|\n)*?>") && textToCheck.Trim().StartsWith("<");
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // AB - 20170703: Aggiunto flag per sapere se l'oprazione ha dei valori di defauly
        /// <summary>
        /// Assume valore true se l'operazione ha dei valori di default, false altrimenti.
        /// </summary>
        public bool HasDefaultValue { get; set; }

        // AB - 20170804: Aggiunto flag per sapere se è stato sovrascritto il valore di default dell'operazione
        [NonSerialized]
        private bool defaultValueOverwrite;
        /// <summary>
        /// Assume valore true se se è stato sovrascritto il valore di default dell'operazione, false altrimenti.
        /// </summary>
        [AttrSalva, AttrNascondiInWord]
        public bool DefaultValueOverwrite
        {
            get
            {
                if (!HasDefaultValue)
                    return false;
                return defaultValueOverwrite;
            }
            set
            {
                defaultValueOverwrite = value;
            }
        }

        [AttrSalva, AttrNascondiInWord]
        public bool IsSaved { get; set; } = false;

        // AB - 20190909: Aggiunta proprietà per la definizione del comportamento dell'operazione.
        [NonSerialized]
        enumComportamentoOperazione _comportamentoazione = enumComportamentoOperazione.Normale;
        /// <summary>
        /// Comportamento dell'operazione.
        /// </summary>
        public enumComportamentoOperazione Comportamento
        {
            get { return _comportamentoazione; }
            set { _comportamentoazione = value; }
        }

        public virtual bool RequisitoSeparatore
        {
            get { return false; }
        }

        /// <summary>
        /// Azione che contiene l'operazione.
        /// </summary>
        public IZVEngineObject Parent
        {
            get { return Azione; }
        }

        static string dll;

        [NonSerialized]
        static Dictionary<string, Type> operazioni = null;
        /// <summary>
        /// Elenco di tutte le operazioni configurabili.
        /// </summary>
        public static Dictionary<string, Type> Operazioni
        {
            get
            {
                if (operazioni == null)
                {
                    Assembly ass = null;
                    operazioni = new Dictionary<string, Type>();

                    List<string> sortList = new List<string>();
                    Dictionary<string, Type> sortListType = new Dictionary<string, Type>();

                    if (dll == null)
                    {
                        string exePath = "";
                        // AB - 20160208: Controllo di essere in un contesto web visto che l'operazione potrebbe essere invocata dal motore di scadenze web che gira in ambiente windows
                        //if (ZVWorkflow.WorkflowIsWeb)
                        if (ZVWorkflow.WorkflowIsWeb && System.Web.HttpContext.Current != null)
                            exePath = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/bin"), "ZV_Engine.dll");
                        else
                            exePath = System.IO.Path.Combine(Environment.CurrentDirectory, "ZV_Engine.dll");
                        Configuration bDllAppFileConfiguration =
                            ConfigurationManager.OpenExeConfiguration(exePath);
                        dll = bDllAppFileConfiguration.AppSettings.Settings["DllOperazioni"].Value;
                    }
                    foreach (string nomedll in dll.Split(','))
                    {
                        try
                        {
                            ass = AppDomain.CurrentDomain.Load(nomedll);
                            foreach (Type type in ass.GetTypes())
                                if (type.IsSubclassOf(typeof(ZVOperazione)) || (type.IsSubclassOf(typeof(WebControl))))
                                {
                                    sortList.Add(type.Name);
                                    sortListType.Add(type.Name, type);
                                }
                        }
                        catch /*(Exception ex)*/{ }
                    }
                    try
                    {
                        ass = AppDomain.CurrentDomain.Load("ZV_Engine");
                        foreach (Type type in ass.GetTypes())
                        {
                            // AB - 20180618: Aggiunto controllo per evitare chiavi duplicate
                            if (!sortListType.ContainsKey(type.Name))
                            {
                                sortList.Add(type.Name);
                                sortListType.Add(type.Name, type);
                            }
                        }
                    }
                    catch /*(Exception ex)*/ { }

                    sortList.Sort();
                    foreach (string a in sortList)
                    {
                        if (!operazioni.ContainsKey(a)) operazioni.Add(a, sortListType[a]);
                    }
                }
                return operazioni;
            }
        }



        public static ZVOperazione NewOperazione(string typeName)
        {
            if (Operazioni.ContainsKey(typeName))
            {
                Type tipo = Operazioni[typeName];
                return (ZVOperazione)tipo.Assembly.CreateInstance(tipo.FullName);
            }
            return null;
        }



        /// <summary>
        /// Esegue il metodo statico della classe specificata.
        /// </summary>
        /// <param name="typeName">Nome della classe contenente il metodo da eseguire.</param>
        /// <param name="methodName">Nome del metodo statico da eseguire.</param>
        /// <param name="args">Argomenti da passare al metodo da eseguire.</param>
        /// <returns>Risultato dell'esecuzione.</returns>
        public static object CallStaticMethod(string typeName, string methodName, object[] args)
        {
            Type tipo = Operazioni[typeName];
            MethodInfo staticMethodInfo = tipo.GetMethod(methodName);
            return staticMethodInfo.Invoke(null, args);
        }


        public virtual List<Serializza> SerializzaAggiorna()
        {
            try
            {
                if (Workflow.OperazioniIsSavedRequired == null) //pregresso
                    IsSaved = true;
                else
                    IsSaved = (Workflow.OperazioniIsSavedRequired.Contains(this.Name));
            }
            catch (Exception) { IsSaved = true; }

            List<Serializza> valori = new List<Serializza>();
            foreach (PropertyInfo propertyInfo in this.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (Attribute attr in propertyInfo.GetCustomAttributes(new AttrSalva().GetType(), true))
                {
                    try
                    {
                        object o = propertyInfo.GetValue(this, null);
                        if (o == null)
                            continue;

                        // AB - 20160225: Gestione della criptografia dei valori testuali
                        CSD.Framework.Security.Cryptography crypt = null;
                        if (Criptato)
                            crypt = Workflow.Crypt;

                        //bool defaultValueOverwrite;
                        Serializza item = SerializzaAttributo(propertyInfo.Name, o, this/*, out defaultValueOverwrite*/, crypt);
                        //DefaultValueOverwrite = defaultValueOverwrite;

                        valori.Add(item);
                    }
                    catch (CSD.Framework.Security.CryptographyException ex)
                    {
                        throw ex;
                    }
                    catch { }
                }
            }
            return valori;
        }

        // AB - 20160225: Gestione della criptografia dei valori testuali
        //public static Serializza SerializzaAttributo(string nome, object o)
        /// <summary>
        /// Ritorna il valore serializzato dell'attributo specificato.
        /// </summary>
        /// <param name="nome">Nome dell'attributo da trattare.</param>
        /// <param name="o">Valore dell'attributo da trattare.</param>
        /// <param name="operazione">Operazione da trattare.</param>
        /// <param name="crypt">Eventuale oggetto con cui criptato il valore serializzato.</param>
        /// <returns></returns>
        public static Serializza SerializzaAttributo(string nome, object o, ZVOperazione operazione, CSD.Framework.Security.Cryptography crypt = null)
        {
            // AB - 20160606: Spostato il controllo del valore vuote in questo metodo
            //if (o == null || ValutaVuoto(o))
            //    return null;

            Serializza item = new Serializza();

            //#10862 WF
            //forzatura per cancellazione valore vuoto derivato dalla serializzazione json
            if (operazione != null)
                if ((operazione.GetType().Name.Equals("ZVCampoTabellaEstesa") || operazione.GetType().Name.Equals("ZVEwsAccantonamenti")) && nome.Equals("DatiJson"))
                    if (o == null || string.IsNullOrEmpty(o.ToString()) || o.Equals("[]"))
                        o = string.Empty;
            //\----


            // AB - 20160606: Spostato il controllo del valore vuote in questo metodo
            // AB - 20170703: Non controllo se il campo è vuoto nel caso in cui questo abbia un valore di defult perchè devo permettere all'utente forzare il valore vuoto
            //                al posto del default.
            //if (ValutaVuoto(o))
            /*defaultValueOverwrite = false;
            if (hasDefaultValue)
                defaultValueOverwrite = true;
                */
            if (operazione != null && CheckDefaultValueOverwrite(nome, operazione))
                operazione.DefaultValueOverwrite = true;
            else if (ValutaVuoto(o))
                item.Vuoto = true;

            //forzatura
            //if (nome.Equals("IsSaved"))
            //    item.Vuoto = ValutaVuoto(o);

            switch (Type.GetTypeCode(o.GetType()))
            {
                case System.TypeCode.String:
                case System.TypeCode.DateTime:
                case System.TypeCode.Boolean:
                    {
                        item.ValoreA = o.ToString();
                        break;
                    }
                case System.TypeCode.Int16:
                case System.TypeCode.Int32:
                case System.TypeCode.Int64:
                case System.TypeCode.UInt16:
                case System.TypeCode.UInt32:
                case System.TypeCode.UInt64:
                case System.TypeCode.SByte:
                case System.TypeCode.Byte:
                case System.TypeCode.Double:
                case System.TypeCode.Single:
                case System.TypeCode.Decimal:
                    {
                        item.ValoreN = Convert.ToDouble(o);
                        break;
                    }
                default:
                    {
                        if (o.GetType() == typeof(byte[]))
                        {
                            item.ValoreB = ZVCompression.Compress(o as byte[]);
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder();
                            StringWriter sw = new StringWriter(sb);

                            if (o.GetType().BaseType == typeof(DataSet))
                                ((DataSet)o).WriteXml(sw, XmlWriteMode.IgnoreSchema);
                            else
                                new XmlSerializer(o.GetType()).Serialize(sw, o);

                            item.ValoreA = sb.ToString();
                        }
                        break;
                    }
            }
            item.Attributo = o.GetType().Name;
            item.Proprieta = nome;

            // AB - 20160225: Cirpto il ValoreA
            if (item.ValoreA != null && crypt != null)
            {
                item.ValoreA = crypt.EncryptByDBFullParams(item.ValoreA, SaltValue, HashAlgorithm, PasswordIterations, InitVector, KeySize);
            }

            return item;
        }

        /// <summary>
        /// Ritorna true se il valore specificato è un valore vuoto(inutile da salvare), false altrimenti.
        /// </summary>
        /// <param name="o">Valore da controllare.</param>
        /// <returns>True se il valore specificato è un valore vuoto(inutile da salvare), false altrimenti.</returns>
        private static bool ValutaVuoto(object o)
        {
            bool vuoto = false;
            if (o is string)
            {
                vuoto = o.ToString().Trim() == string.Empty;
            }
            else if (o is IList && o.GetType().IsGenericType)
            {
                vuoto = (o as IList).Count == 0;
            }
            else if (o is double)
            {
                vuoto = ((double)o) == 0;
            }
            else if (o is bool)
            {
                vuoto = !((bool)o);
            }
            else if (o is DateTime)
            {
                vuoto = ((DateTime)o) == DateTime.MinValue;
            }
            else if (o is DataSet)
            {
                vuoto = ((DataSet)o).Tables.Count == 0 || (((DataSet)o).Tables.Count > 0 && ((DataSet)o).Tables[0].Rows.Count == 0);
            }
            else if (o is Enum && o.GetType().Name.Equals("enTipoInformazioni"))
            {
                // MR 01/08/2016 infoPubbliche è di default, la non presenza in DB significa che è pubblico,
                // si vuole evitare d isalvare in DB informazioni di default e raramente modificate
                vuoto = o.ToString().Equals("infoPubbliche");
            }

            return vuoto;
        }

        // AB - 20180517: Controlla se il valore di default è stato sovrascritto per la proprietà specificata.
        /// <summary>
        /// Controlla se nell'operazione specificata è già stato salvato un valore che ha sovrascritto il valore di default.
        /// </summary>
        /// <param name="nome">Nome della proprietà da trattare.</param>
        /// <param name="operazione">Operazione da trattare.</param>
        /// <returns>True se il valore di default è stato sovrascritto, false altrimenti.</returns>
        private static bool CheckDefaultValueOverwrite(string nome, ZVOperazione operazione)
        {
            // IMPORTANTE: La logica corretta si basa sul principio che se, durante la deserializzazione da DB di un'operazione, trovo valorizzata una proprietà che
            //             viene inizializzata con quanto definito nel valore di default, allora il valore di default per questa operazione è stato sovrascritto.
            //             Se, per una specifica operazione, la proprietà con cui viene configurato il valore di defualt è diversa da quella che viene inizializzata 
            //             con il valore di default(quella con AttrSalva), allora, in questo metodo, sarà quest'ultima la proprietà da controllare.
            bool defaultValueOverwrite = false;
            if (operazione.HasDefaultValue)
            {
                switch (operazione.GetType().Name)
                {
                    case "ZVCampoTesto":
                    case "ZVCampoCKEditorTesto":
                    case "ZVCampoNumero":
                    case "ZVCampoCheckBox":
                    case "ZVCampoDataScadenza":
                    case "ZVCampoData":
                    case "ZVCampoComboBox":
                        // AB - 20190305: Modifica per essere sicuri che la valorizzazione del flag "defaultValueOverwrite"avvenga solo verificando la proprietà che può essere 
                        //                inizializzata con il valore di default. Questa evita che il flag venga resettato da eventuali altre proprietà salvate per l'operazione.
                        //defaultValueOverwrite = nome.Equals("valore", StringComparison.InvariantCultureIgnoreCase);
                        if (nome.Equals("valore", StringComparison.InvariantCultureIgnoreCase))
                            defaultValueOverwrite = true;
                        break;

                    case "ZVEwsAccantonamenti":
                        // AB - 20190305: Modifica per essere sicuri che la valorizzazione del flag "defaultValueOverwrite"avvenga solo verificando la proprietà che può essere 
                        //                inizializzata con il valore di default. Questa evita che il flag venga resettato da eventuali altre proprietà salvate per l'operazione.
                        //defaultValueOverwrite = nome.Equals("DatiJson", StringComparison.InvariantCultureIgnoreCase);
                        if (nome.Equals("DatiJson", StringComparison.InvariantCultureIgnoreCase))
                            defaultValueOverwrite = true;
                        break;
                    // AB - 20180301: Modificata proprietà verificata per il campo data
                    /*
                    case "ZVCampoData":
                        defaultValueOverwrite = nome.Equals("dataDelGiorno", StringComparison.InvariantCultureIgnoreCase);
                        break;                    
                    case "ZVCampoComboBox":
                        defaultValueOverwrite = nome.Equals("valoreDefault", StringComparison.InvariantCultureIgnoreCase);
                        break;
                    */
                    case "ZVCompetenzaAzione":
                        // AB - 20190305: Modifica per essere sicuri che la valorizzazione del flag "defaultValueOverwrite"avvenga solo verificando la proprietà che può essere 
                        //                inizializzata con il valore di default. Questa evita che il flag venga resettato da eventuali altre proprietà salvate per l'operazione.
                        //defaultValueOverwrite = nome.Equals("informazioniVisibiliA", StringComparison.InvariantCultureIgnoreCase);
                        if (nome.Equals("informazioniVisibiliA", StringComparison.InvariantCultureIgnoreCase))
                            defaultValueOverwrite = true;
                        break;
                }
            }
            return defaultValueOverwrite;
        }

        /// <summary>
        /// Deserializza l'operazione applicando la lista di valori specificati.
        /// </summary>
        /// <param name="valori">Lista di valori da applicare all'operazione.</param>
        /// <param name="decryptValue">Impostare a true se si vuole che anche vengano decriptati i valori cifrati, false altrimenti.</param>
        public void DeSerializza(List<Serializza> valori, bool decryptValue = true)
        {
            foreach (Serializza item in valori)
                try
                {
                    Type eleType;
                    PropertyInfo propertyInfo = this.GetType().GetProperty(item.Proprieta, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfo == null)
                        continue;
                    eleType = propertyInfo.PropertyType;

                    // AB - 20160225: Gestione della criptografia dei valori testuali
                    //object valore = DeSerializzaAttributo(item, eleType);
                    CSD.Framework.Security.Cryptography crypt = null;
                    if (Criptato && decryptValue)
                        crypt = Workflow.Crypt;
                    object valore = DeSerializzaAttributo(item, eleType, crypt);

                    propertyInfo.SetValue(this, valore, null);
                }
                catch (CSD.Framework.Security.CryptographyException ex)
                {
                    throw ex;
                }
                catch { }
        }


        // AB - 20160225: Gestione della criptografia dei valori testuali
        /// <summary>
        /// Ritorna il valore tipizzato contenuto nel record specificato.
        /// </summary>
        /// <param name="item">Record con il valore da tipizzare.</param>
        /// <param name="eleType">Tipo del valore da tornare.</param>
        /// <param name="crypt">Modulo con cui criptare il valore da tornare. Imopstare a null se il valore non deve essere criptato.</param>
        /// <returns>Valore tipizzato contenuto nel record specificato.</returns>
        public static object DeSerializzaAttributo(Serializza item, Type eleType, CSD.Framework.Security.Cryptography crypt = null)
        {
            // AB - 20160225: Decirpto il ValoreA
            if (item.ValoreA != null && crypt != null)
            {
                item.ValoreA = crypt.DecryptByDBFullParams(item.ValoreA, SaltValue, HashAlgorithm, PasswordIterations, InitVector, KeySize);
            }

            object valore = null;
            switch (Type.GetTypeCode(eleType))
            {
                case System.TypeCode.String:
                    {
                        valore = item.ValoreA;
                        break;
                    }
                case System.TypeCode.Int32:
                    {
                        valore = Convert.ToInt32(item.ValoreN);
                        break;
                    }
                case System.TypeCode.Int64:
                    {
                        valore = Convert.ToInt64(item.ValoreN);
                        break;
                    }
                case System.TypeCode.Double:
                    {
                        valore = item.ValoreN;
                        break;
                    }
                case System.TypeCode.Single:
                    {
                        valore = Convert.ToSingle(item.ValoreN);
                        break;
                    }
                case System.TypeCode.Decimal:
                    {
                        valore = Convert.ToDecimal(item.ValoreN);
                        break;
                    }
                case System.TypeCode.DateTime:
                    {
                        if (String.IsNullOrEmpty(item.ValoreA))
                            valore = DateTime.MinValue;
                        else
                            valore = DateTime.Parse(new StringBuilder(item.ValoreA).Replace(".", ":").ToString());
                        break;
                    }
                case System.TypeCode.Boolean:
                    {
                        if (String.IsNullOrEmpty(item.ValoreA))
                            valore = false;
                        else
                            valore = Boolean.Parse(item.ValoreA);
                        break;
                    }
                default:
                    {
                        if (eleType == typeof(byte[]))
                            try
                            {
                                valore = ZVCompression.Decompress(item.ValoreB);
                            }
                            catch
                            {
                                valore = item.ValoreB;
                            }
                        else
                            if (eleType.BaseType == typeof(DataSet))
                        {
                            StringReader sr = new StringReader(item.ValoreA);
                            DataSet ds = Activator.CreateInstance(eleType, true) as DataSet;
                            if (ds != null && !string.IsNullOrEmpty(item.ValoreA))
                                ds.ReadXml(sr);
                            valore = ds;
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(item.ValoreA))
                                valore = null;
                            else
                            {
                                StringReader sw = new StringReader(item.ValoreA);
                                XmlSerializer s = new XmlSerializer(eleType);
                                valore = s.Deserialize(sw);
                            }
                        }
                        break;
                    }
            }
            return valore;
        }

        /// <summary>
        /// Ritorna un clone dell'operazione.
        /// </summary>
        /// <returns>Clone dell'operazione.</returns>
        public ZVOperazione GetCopia()
        {

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream msOperazioneCorrente = new MemoryStream();
            bf.Serialize(msOperazioneCorrente, this);
            msOperazioneCorrente.Flush();
            msOperazioneCorrente.Position = 0;
            ZVOperazione copia = ((ZVOperazione)bf.Deserialize(msOperazioneCorrente));
            return copia;
        }
        #endregion

        #region definizioni classi attributi correlati
        /// <summary>
        /// Valore serializzato di una proprietà di operazione.
        /// </summary>
        public class Serializza
        {
            /// <summary>
            /// Nome della proprietà.
            /// </summary>
            public string Proprieta;
            /// <summary>
            /// Valore alfanumerico.
            /// </summary>
            public string ValoreA;
            /// <summary>
            /// Valore numerido.
            /// </summary>
            public double ValoreN;
            /// <summary>
            /// Valore binario.
            /// </summary>
            public byte[] ValoreB;
            /// <summary>
            /// Tipo della proprieta
            /// </summary>
            public string Attributo;
            /// <summary>
            /// Impostare a true se la proprietà ha un valore vuoto, false altrimenti.
            /// </summary>
            public bool Vuoto;
        }
    }

    // Run time 

    /// <summary>
    /// Proprietà salvata in tabella ZV_Operazione a run time
    /// </summary>
    public class AttrSalva : System.Attribute
    {
        /// <summary>
        /// Nome.
        /// </summary>
        public string nome = "";
        /// <summary>
        /// Ordinamento.
        /// </summary>
        public int ordine = 0;
    }

    /// <summary>
    /// Variabile visibile (la visibilita dipende anche dall'attributo precedente) da usare per il calcolo delle evidenze. 
    /// </summary>
    public class AttrVisibile : System.Attribute
    {

    }

    /// <summary>
    /// Variabile nascosta nell'elenco delle variabili utilizzabili in word
    /// </summary>
    public class AttrNascondiInWord : System.Attribute
    {

    }

    public class AttrNascondiInCampiDipendenze : System.Attribute
    {

    }

    // Design
    /// <summary>
    /// Nome descrittivo dell'operazione.
    /// </summary>
    public class AttrName : System.Attribute
    {
        /// <summary>
        /// Nome descrittivo dell'operazione.
        /// </summary>
        public string Name;

        /// <summary>
        /// Costruttore delle classe.
        /// </summary>
        /// <param name="name">Nome descrittivo dell'operazione.</param>
        public AttrName(string name)
        {
            this.Name = name;
        }
    }

    /// <summary>
    /// Attributo per disabilitare operazione (utile in casi temporanei)
    /// </summary>
    public class AttrOperazioneDisabilitata : System.Attribute { }

    /// <summary>
    /// Tipo operazione che si puo aggiungere a template.
    /// </summary>
    public class AttrTemplate : System.Attribute { }

    /// <summary>
    /// Tipo operazione che si puo aggiungere a workflow.
    /// </summary>
    public class AttrWorkflow : System.Attribute { }

    /// <summary>
    /// Tipo operazione che si puo aggiungere a workflow non in apertura o apertura standard.
    /// </summary>
    public class AttrWorkflowAzione : System.Attribute { }

    /// <summary>
    /// Tipo operazione che si puo aggiungere a template non in apertura o apertura standard.
    /// </summary>
    public class AttrTemplateAzione : System.Attribute { }

    /// <summary>
    /// Tipo operazione che si puo aggiungere operazione inseribile solo in Apertura Standard.
    /// </summary>
    public class AttrAperturaStandard : System.Attribute { }

    /// <summary>
    /// Tipo operazione che non si puo aggiungere in apertura normale.
    /// </summary>
    public class AttrEscludiAperturaNormale : System.Attribute { }

    /// <summary>
    /// Tipo operazione che non si puo aggiungere in apertura standard.
    /// </summary>
    public class AttrEscludiAperturaStandard : System.Attribute { }

    /// <summary>
    /// Tipo operazione inseribile solo da chi è abilitato alla configurazione di template
    /// </summary>
    public class AttrSoloConfigTemplate : System.Attribute { }

    /// <summary>
    /// Tipo operazione che viene considerata nel processo di ricerca.
    /// </summary>
    public class AttrRicerca : System.Attribute { }

    /// <summary>
    /// Tipo operazione che viene considerata nel processo di analisi.
    /// </summary>
    public class AttrAnalisi : System.Attribute { }

    /// <summary>
    /// Contesto di utilizzo del workflow.
    /// </summary>
    public enum ContestoUtilizzo
    {
        /// <summary>
        /// Esegui.
        /// </summary>
        Esegui,
        /// <summary>
        /// Esegui in modifica.
        /// </summary>
        EseguiInModifica,
        /// <summary>
        /// Lettura.
        /// </summary>
        Lettura,
        /// <summary>
        /// Storico.
        /// </summary>
        Storico,
        /// <summary>
        /// Riepilogo.
        /// </summary>
        Riepilogo,
        /// <summary>
        /// Riepilogo in lettura.
        /// </summary>
        RiepilogoInLettura,
        /// <summary>
        /// Template.
        /// </summary>
        Template,
        /// <summary>
        /// Workflow.
        /// </summary>
        Workflow
    };

    /// <summary>
    /// Tipo di errore.
    /// </summary>
    public enum ZVTipoErroreEnum
    {
        /// <summary>
        /// Errore.
        /// </summary>
        Errore,
        /// <summary>
        /// Messaggio.
        /// </summary>
        Messaggio,
        /// <summary>
        /// Cancellazione.
        /// </summary>
        Cancellazione
    }

    /// <summary>
    /// Errore.
    /// </summary>
    [Serializable]
    public class ZVErrore
    {
        /// <summary>
        /// Tipo di errore.
        /// </summary>
        public ZVTipoErroreEnum Tipo;
        /// <summary>
        /// Oggetto che ha generato l'errore.
        /// </summary>
        public object Sender;
        /// <summary>
        /// Descrizione dell'errore.
        /// </summary>
        public string Messaggio;
        /// <summary>
        /// Colore originale dell'oggetto che ha generato l'errore.
        /// </summary>
        public Color ColoreOriginale;

        /// <summary>
        /// Costruttore di una segnalazione di tipo Errore
        /// </summary>
        /// <param name="sender">Oggetto che ha generato l'errore.</param>
        /// <param name="descrizione">Descrizione dell'errore.</param>
        public ZVErrore(object sender, string descrizione)
        {
            Sender = sender;
            Messaggio = descrizione;
            Tipo = ZVTipoErroreEnum.Errore;
        }

        /// <summary>
        /// Costruttore di una segnalazione di tipo specificat.
        /// </summary>
        /// <param name="sender">Oggetto che ha generato l'errore.</param>
        /// <param name="descrizione">Descrizione dell'errore.</param>
        /// <param name="tipo">Tipo di errore.</param>
        public ZVErrore(object sender, string descrizione, ZVTipoErroreEnum tipo)
        {
            Sender = sender;
            Messaggio = descrizione;
            Tipo = tipo;
        }
    }

    #endregion

}
