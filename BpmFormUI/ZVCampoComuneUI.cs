using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ZV_Engine;

namespace BPMFormUI
{
    /// <summary>
    /// Classe che definisce le caratteristiche base delle operazioni che consentono dipendenze e esecuzione del codice c#.
    /// </summary>
    [Serializable]
    public class ZVCampoComuneUI  
    {
        /// <summary>
        /// Lista degli stati in cui l'operazione non è visibile.
        /// </summary>
        protected List<string> statiSelNonVis = new List<string>();
        /// <summary>
        /// Lista degli stati in cui l'operazione non è visibile.
        /// </summary>
        public List<string> StatiSelNonVis
        {
            get { return statiSelNonVis; }
            set { statiSelNonVis = value; }
        }

        /// <summary>
        /// Lista degli stati in cui l'operazione è editabile.
        /// </summary>
        protected List<string> statiSelEdit = new List<string>();
        /// <summary>
        /// Lista degli stati in cui l'operazione è editabile.
        /// </summary>
        public List<string> StatiSelEdit
        {
            get { return statiSelEdit; }
            set { statiSelEdit = value; }
        }

        /// <summary>
        /// Altezza del controllo dell'operazione
        /// </summary>
        protected int ctrHeight = 30;
        /// <summary>
        /// Altezza del controllo dell'operazione
        /// </summary>
        public int CtrHeight
        {
            get { return ctrHeight; }
            set { ctrHeight = value; }
        }

        /// <summary>
        /// Codice c# configurato nell'operazione.
        /// </summary>
        protected string requisito = "";
        /// <summary>
        /// Codice c# configurato nell'operazione.
        /// </summary>
        public string Requisito
        {
            get { return requisito; }
            set { requisito = value; }
        }

        /// <summary>
        /// Colore dell'operazione.
        /// </summary>
        protected Color colore = Color.White;
        /// <summary>
        /// Colore dell'operazione.
        /// </summary>
        public Color Colore
        {
            get { return colore; }
            set { colore = value; }
        }

        /// <summary>
        /// Regular expressione da applicare all'operazione.
        /// </summary>
        protected String regExp = string.Empty;

        /// <summary>
        /// Regular expressione da applicare all'operazione.
        /// </summary>
        public String RegExp
        {
            get { return regExp; }
            set { regExp = value; }
        }

        /// <summary>
        /// Codice della regular expressione da applicare all'operazione.
        /// </summary>
        protected String codRegExp = string.Empty;
        /// <summary>
        /// Codice della regular expressione da applicare all'operazione.
        /// </summary>
        public String CodRegExp
        {
            get { return codRegExp; }
            set { codRegExp = value; }
        }
        /*
        /// <summary>
        /// Controllo formale.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        public override void ControlloFormale(ContestoUtilizzo contesto)
        {
            
        }*/

        /// <summary>
        /// Ritorna true se il controllo può essere creato nel contesto specificato e ne determina sia l'attivabilità e l'editabilità.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <param name="interfacciaEditabile">Assume true se il controllo è editabile, false altrimenti.</param>
        /// <returns>True se il controllo può essere creato nel contesto specificato, false altrimenti.</returns>
        public virtual bool TipoControl(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        {
            // AB - 20160609 - Spostata la verifica del ContestoUtilizzo.Lettura in AperturaStandard
            if (contesto == ContestoUtilizzo.Storico || contesto == ContestoUtilizzo.RiepilogoInLettura/* || contesto==ContestoUtilizzo.Lettura*/)
            {
                ctrAttivo = false;
                interfacciaEditabile = false;
                return true;
            }
            if (Azione.Name == ZVAzione.AperturaStandard)
            {
                if ((statiSelEdit.Contains(this.Azione.Stato.Name) &&
                    (contesto == ContestoUtilizzo.Esegui || contesto == ContestoUtilizzo.EseguiInModifica || (!ctrNONModRiep && contesto == ContestoUtilizzo.Riepilogo))))
                {
                    interfacciaEditabile = true;
                    return true;
                }

                if (ctrNONModRiep && contesto == ContestoUtilizzo.Riepilogo)
                {
                    interfacciaEditabile = false;
                    return true;
                }
                // AB - 20160609 - Spostata la verifica del ContestoUtilizzo.Lettura in AperturaStandard
                //if (!statiSelEdit.Contains(this.Azione.Stato.Name) && !statiSelNonVis.Contains(this.Azione.Stato.Name))
                if ((contesto == ContestoUtilizzo.Lettura || !statiSelEdit.Contains(this.Azione.Stato.Name)) &&
                    !statiSelNonVis.Contains(this.Azione.Stato.Name))
                {
                    interfacciaEditabile = false;
                    ctrAttivo = false;
                    return true;
                }
                if ((statiSelNonVis.Contains(this.Azione.Stato.Name)))
                {
                    interfacciaEditabile = false;
                    ctrAttivo = false;
                    return false;
                }
            }
            //ctrAttivo = this.ctrAttivo;
            interfacciaEditabile = true;
            return true;
        }

        //----
        /// <summary>
        /// Assume true se è forzata la non obbligatorietà dell'operazione, false altrimenti.
        /// </summary>
        protected bool _forzaNonObbligatorio = false;
        /// <summary>
        /// Assume true se è forzata la non obbligatorietà dell'operazione, false altrimenti.
        /// </summary>
        public virtual bool ForzaNonObbligatorio
        {
            get
            { return _forzaNonObbligatorio; }
            set
            { _forzaNonObbligatorio = value; }
        }

        //----

        /// <summary>
        /// Assume true se il controllo è obbligatorio, false altrimenti
        /// </summary>
        protected bool obbligatorio = false;
        /// <summary>
        /// Assume true se il controllo è obbligatorio, false altrimenti
        /// </summary>
        public virtual bool Obbligatorio
        {
            get { return (!_forzaNonObbligatorio ? obbligatorio : false); }
            set { obbligatorio = value; }
        }

        // AB - 20151120: Proprietà per la gestione delle operazioni da consulatare in fase di inizializzazione
        /// <summary>
        /// Lista delle operazione precedenti da cui è possibile ricevere valori per l'inizializzazione dell'operazione corrente.
        /// </summary>
        protected List<string> operazioniPerInizializzazione = new List<string>();
        /// <summary>
        /// Lista delle operazione precedenti da cui è possibile ricevere valori per l'inizializzazione dell'operazione corrente.
        /// </summary>
        public List<string> OperazioniPerInizializzazione
        {
            get { return operazioniPerInizializzazione; }
            set { operazioniPerInizializzazione = value; }
        }

        // AB - 20151201: Inizializzazione dell'operazione da parametri esterni
        /// <summary>
        /// Inizializza l'operazione in funzione degli eventuali parametri esterni impostati
        /// </summary>
        /// <param name="contesto">Contesto in cui ci si trova</param>
        /// <returns>True se dei parametri esterni sono stati effettivamente applicati all'operazione, false altrimenti.</returns>
        public bool InizializzaDaParametriEsterni(ContestoUtilizzo contesto)
        {
            if (Workflow.ParametriEsterni == null ||
                contesto != ContestoUtilizzo.Esegui)
                return false;

            bool valoriApplicati = false;
            foreach (string key in Workflow.ParametriEsterni.Keys)
            {
                string[] keyPars = key.Split('|');
                if (keyPars.Length <= 1 ||
                    !keyPars[0].Equals(this.Name, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                // Parametri specifici per l'operazione
                string parName = keyPars[keyPars.Length - 1];

                // Gestisco il parametro esterno
                valoriApplicati = GestioneParametroEsterno(parName, Workflow.ParametriEsterni[key]);

                if (valoriApplicati)
                    // Segnalo che l'operazione è stata modificata
                    Modificata = true;
            }

            return valoriApplicati;
        }

        // AB - 20151201: Inizializzazione dell'operazione da info aggiuntive
        /// <summary>
        /// Inizializza l'operazione in funzione delle info aggiuntive che sono state fornite dalle dipendenze
        /// </summary>
        /// <returns>True se dei parametri aggiuntivi sono stati effettivamente applicati all'operazione, false altrimenti.</returns>
        public bool InizializzaDaInfoAggiuntiveDipendenze()
        {
            if (Workflow.InfoAggiuntive == null)
                return false;

            bool valoriApplicati = false;
            bool gestioneInfoAggiuntiveCompletata = false;
            // Parametri derivanti da altre operazioni
            foreach (string par in OperazioniPerInizializzazione)
            {
                //string[] pars = (par + "||").Split('|'); //fallback
                string[] pars = new StringBuilder(par).Append("||").ToString().Split('|'); //fallback

                //20171130 : gestione di parametri con operazione|parametro|campo
                //in infoAggiuntive la chiave dei valori è sempre operazione|parametro
                // if (Workflow.InfoAggiuntive.ContainsKey(par))
                if (Workflow.InfoAggiuntive.ContainsKey(new StringBuilder().AppendFormat("{0}|{1}", pars[0], pars[1]).ToString()))
                {
                    // Controllo che siano stati modificati i valori dell'operazione da cui deve leggere l'info aggiuntiva
                    //string[] pars = par.Split('|');
                    foreach (ZVOperazione op in this.Azione.OperazioniAttive)
                    {
                        // Esco se ho raggiunto l'operazione stessa
                        if (op.Name == this.Name)
                            break;

                        // Se l'operazione non coincide con quella del parametro, passo alla successiva
                        if (op.Name != pars[0])
                            continue;

                        // Esco se l'operazione a cui si riferisce il parametro non è stata modificata
                        if (!op.Modificata)
                            break;

                        // Gestisco il parametro dell'info aggiuntiva
                        valoriApplicati = GestioneParametroInfoAggiuntiva(par, out gestioneInfoAggiuntiveCompletata);
                        if (valoriApplicati)
                            Modificata = true;

                        break;
                    }

                    // Verifico se la gestione delle info aggiuntive deve essere terminata
                    if (gestioneInfoAggiuntiveCompletata)
                        break;
                }
                //}
            }

            return valoriApplicati;
        }

        // AB - 20151201: Polpolamento delle info aggiuntive.
        /// <summary>
        /// Popolamento delle InfoAggiuntive previste per l'operazione.
        /// Questa operazione viene eseguita solo se i valori dell'operazione sono stati modificati.
        /// </summary>
        public void PopolaInfoAggiuntive()
        {
            if (!Modificata ||
                ParametriPropagabili.Count == 0)
                return;

            if (Workflow.InfoAggiuntive == null)
                Workflow.InfoAggiuntive = new Dictionary<string, object>();

            // Per ogni parametro propagabile previsto, carico nelle info aggiuntive il valore relativo
            CaricaValoriParametroPropagabile(ParametriPropagabili);
        }

        /// <summary>
        /// Gestione del parametro esterno specificato.
        /// </summary>
        /// <param name="parName">Nome del parametro esterno da gestire.</param>
        /// <param name="value">Valore del parametro esterno da gestire.</param>
        /// <returns>True se il valore del parametro è stato applicato all'operazione, false altrimeti</returns>
        public virtual bool GestioneParametroEsterno(string parName, object value)
        {
            return false;
        }

        /// <summary>
        /// Gestione del parametro dell'info aggiuntiva specificato.
        /// </summary>
        /// <param name="parName">Nome del parametro dell'info aggiuntiva da gestire.</param>
        /// <param name="gestioneInfoAggiuntiveCompletata">
        /// True se l'intera gestione delle info aggiuntive è stata completata, false altrimenti.
        /// </param>
        /// <returns>True se il valore del parametro è stato applicato all'operazione, false altrimeti</returns>
        public virtual bool GestioneParametroInfoAggiuntiva(string parName, out bool gestioneInfoAggiuntiveCompletata)
        {
            gestioneInfoAggiuntiveCompletata = false;
            return false;
        }

        /// <summary>
        /// Caricamento nelle info aggiuntive del valore del parametro propagabile.
        /// </summary>
        /// <param name="lparName">Lista dei nomi dei parametri da caricare.</param>
        public virtual void CaricaValoriParametroPropagabile(List<string> lparName)
        {
        }

        /// <summary>
        /// Aggiunta del valore in InfoAggiuntive
        /// </summary>
        /// <param name="opName">Nome dell'operazione.</param>
        /// <param name="parname">Nome del parametro.</param>
        /// <param name="value">Valore.</param>
        public void AddInfo(string opName, string parname, string value)
        {
            string key = new StringBuilder().AppendFormat("{0}|{1}", opName, parname).ToString();
            if (Workflow.InfoAggiuntive.ContainsKey(key))
                Workflow.InfoAggiuntive[key] = value;
            else
                Workflow.InfoAggiuntive.Add(key, value);
        }

        // AB - 20151201: Propaga le modifiche a tutte le operazioni dipendenti
        // TODO: Nelle versione windows bisogna resettare il flag "Modificata" al termine delle propagazione!!!!!!!!!!
        /// <summary>
        /// Propaga la modifica apportata all'operazione a tutte le sue dipendenti.
        /// </summary>
        /// <returns>La lista delle operazioni dipendenti modificate.</returns>
        public List<ZVOperazione> PropagaModifica()
        {
            List<ZVOperazione> operazioniModificate = new List<ZVOperazione>();

            bool trovata = false;
            foreach (ZVOperazione op in this.Azione.OperazioniAttive)
            {
                // AB - 20180530: Aggiunto controllo per evitare di trattare le operazione che non sono nè attive, nè visibili in base al contesto
                if (!op.CtrAttivo && !op.CtrVisibile)
                    continue;

                if (op.Name == this.Name)
                {
                    trovata = true;
                    PopolaInfoAggiuntive();
                }
                else
                {
                    if (!trovata)
                        continue;

                    ZVCampoComune operazione = op as ZVCampoComune;
                    if (operazione != null)
                    {
                        operazione.InizializzaDaInfoAggiuntiveDipendenze();

                        // AB - 20180528: Eseguo anche il c# dell'operazione
                        Type tipeGhost;
                        if (!String.IsNullOrWhiteSpace(operazione.Requisito))
                            //ZVCompiler.Esegui(operazione.Requisito, operazione, null, out tipeGhost, false, operazione.Workflow.SessionID, false);
                            ZVCompiler.Esegui(operazione.Requisito, operazione, null, out tipeGhost, false, operazione.Workflow.SessionID);

                        operazione.PopolaInfoAggiuntive();
                        if (operazione.Modificata)
                        {
                            operazioniModificate.Add(operazione);
                        }
                    }
                }
            }

            return operazioniModificate;
        }

        /// <summary>
        /// Etichetta dell'operazione.
        /// </summary>
        [AttrVisibile]
        public string LabelString
        {
            get { return Label; }
        }

        /// <summary>
        /// Allinea una determinata operazione di workflow con le modifiche apportate alla stessa nel template da cui deriva.
        /// </summary>
        /// <param name="operazioneTemplate">Operazione da allineare.</param>
        public override void AggiornaWorkflowDaTemplate(ZVOperazione operazioneTemplate)
        {
           

            base.AggiornaWorkflowDaTemplate(operazioneTemplate);
            this.requisito = (operazioneTemplate as ZVCampoComune).requisito;
        }
    }

    /// <summary>
    /// Entità regular expressione.
    /// </summary>
    public class RegExpCls
    {
        /// <summary>
        /// Codice della regular expressione.
        /// </summary>
        public string CodRegExp;
        /// <summary>
        /// Descrizione della regular expressione.
        /// </summary>
        public string DescrRegExp;
        /// <summary>
        /// Regular expressione.
        /// </summary>
        public string RegExp;

        /// <summary>
        /// Ritorna la descrizione della regular expressione.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return DescrRegExp;
        }
    }
}