using System;
using System.Collections.Generic;
using ZV_Engine;
using ZV_DataSet;
using System.Windows.Forms;

namespace ZV_Operazioni
{
    /// <summary>
    /// Operazione campo di testo.
    /// </summary>
    [Serializable, AttrWorkflow, AttrTemplate, AttrName("Modulo - Testo informazione alfanumerica"), AttrRicerca, AttrAnalisi]
    public class ZVCampoTestoUI : ZVCampoComune
    {
        /// <summary>
        /// Parametri esterni gestiti nell'operazione.
        /// </summary>
        private struct ParametroEsterno
        {
            public const string Valore = "Valore";
        }

        /// <summary>
        /// Lista dei parametri esterni valorizzabili per l'operazione.
        /// </summary>
        public override List<String> ParametriEsterniValorizzabili
        {
            get
            {
                string[] pars = { ParametroEsterno.Valore };
                return new List<string>(pars);
            }
        }

        private int numeroRighe = 1;
        /// <summary>
        /// Numero di righe da visualizzare nell'interfaccia a runtime.
        /// </summary>
        public int NumeroRighe
        {
            get { return numeroRighe; }
            set { numeroRighe = value; }
        }

        private string nMaxCaratteri;
        /// <summary>
        /// Numero massimo di caratteri inseribili a runtime.
        /// </summary>
        public string NMaxCaratteri
        {
            get { return nMaxCaratteri; }
            set { nMaxCaratteri = value; }
        }
        private string placeHolder;
        /// <summary>
        /// Suggerimento da visualizzare nel controllo.
        /// </summary>
        public string PlaceHolder
        {
            get { return placeHolder; }
            set { placeHolder = value; }
        }
        //protected string valore;
        private string valore;
        /// <summary>
        /// Valore dell'operazione.
        /// </summary>
        [AttrSalva]
        public string Valore
        {
            get { return valore; }
            set { valore = value; }
        }

        /// <summary>
        /// Valore dell'operazione in formato stringa.
        /// </summary>
        [AttrVisibile]
        public string ValoreStringa
        {
            get { return Valore; }
        }

        [NonSerialized]
        DefRegExpDS regExpTesto;
        /// <summary>
        /// RegExp da applicare al testo editato a runtime.
        /// </summary>
        public DefRegExpDS RegExpTesto
        {
            get
            {
                if (regExpTesto == null)
                    regExpTesto = ZVDataLayer.RegExpLeggi(ZVDataLayer.TipoRegExp.workFlow, ZVDataLayer.FunzioneRegExp.Testo);

                return regExpTesto;
            }
        }

        /// <summary>
        /// Ritorna il controllo per il Design.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <returns>Controllo per il Design.</returns>
        public override Control ControlDesign(ContestoUtilizzo contesto)
        {
            return new ZVCampoTestoControl(this, contesto);
        }

        /// <summary>
        /// Ritorna il controllo per l'esecuzione in ambiente WindowsForm.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <param name="interfacciaEditabile">Assume true se il controllo è editabile, false altrimenti.</param>
        /// <returns>Controllo per l'esecuzione in ambiente WindowsForm.</returns>
        public override Control ControlRuntime(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        {
            interfacciaEditabile = false;
            if (!TipoControl(contesto, out interfacciaEditabile))
                return null;

            // AB - 20151201: inizializzazione dell'operazione da parametri esterni
            base.InizializzaDaParametriEsterni(contesto);
            // AB - 20151201: inizializzazione dell'operazione da info aggiuntive delle dipendenze
            base.InizializzaDaInfoAggiuntiveDipendenze();
            // Popolo le info aggiuntive previste per l'operazione 
            PopolaInfoAggiuntive();

            Control ctr = new ZVCampoTestoControlEsegui(this, contesto);
            ctr.Name = this.Name;
            //ctr.Enabled = editabile;
            return ctr;
        }

        [NonSerialized]
        System.Web.UI.WebControls.WebControl ctr;
        /// <summary>
        /// Interfaccia Web dell'operazione.
        /// </summary>
        public System.Web.UI.WebControls.WebControl Ctr
        {
            get { return ctr; }
            set { ctr = value; }
        }

        /// <summary>
        /// Ritorna il controllo per l'esecuzione in ambiente Web.
        /// </summary>
        /// <param name="contesto">Contesto di utilizzo.</param>
        /// <param name="interfacciaEditabile">Assume true se il controllo è editabile, false altrimenti.</param>
        /// <returns>Controllo per l'esecuzione in ambiente Web.</returns>
        public override System.Web.UI.WebControls.WebControl ControlRuntimeWeb(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        {
            interfacciaEditabile = false;
            if (!TipoControl(contesto, out interfacciaEditabile))
                return null;

            // AB - 20151201: inizializzazione dell'operazione da parametri esterni
            base.InizializzaDaParametriEsterni(contesto);
            // AB - 20151201: inizializzazione dell'operazione da info aggiuntive delle dipendenze
            base.InizializzaDaInfoAggiuntiveDipendenze();
            // Popolo le info aggiuntive previste per l'operazione 
            PopolaInfoAggiuntive();

            ctr = ZVOperazione.NewWebControl("ZVCampoTestoControlEseguiWeb", new object[2] { this, contesto });

            //ctr.Name = this.Name;
            //ctr.Enabled = editabile;
            return ctr;
        }
        
        /// <summary>
        /// Gestione del parametro esterno specificato.
        /// </summary>
        /// <param name="parName">Nome del parametro esterno da gestire.</param>
        /// <param name="value">Valore del parametro esterno da gestire.</param>
        /// <returns>True se il valore del parametro è stato applicato all'operazione, false altrimenti</returns>
        public override bool GestioneParametroEsterno(string parName, object value)
        {
            bool valoreApplicato = false;
            switch (parName)
            {
                case ParametroEsterno.Valore:
                    valoreApplicato = true;
                    Valore = value.ToString();
                    break;
            }

            return valoreApplicato;
        }

        /// <summary>
        /// Gestione del parametro dell'info aggiuntiva specificato.
        /// </summary>
        /// <param name="parName">Nome del parametro dell'info aggiuntiva da gestire.</param>
        /// <param name="gestioneInfoAggiuntiveCompletata">
        /// True se l'intera gestione delle info aggiuntive è stata completata, false altrimenti.
        /// </param>
        /// <returns>True se il valore del parametro è stato applicato all'operazione, false altrimenti</returns>
        public override bool GestioneParametroInfoAggiuntiva(string parName, out bool gestioneInfoAggiuntiveCompletata)
        {
            valore = Workflow.InfoAggiuntive[parName].ToString();
            gestioneInfoAggiuntiveCompletata = true;
            return true;
        }

        /// <summary>
        /// Caricamento nelle info aggiuntive del valore del parametro propagabile.
        /// </summary>
        /// <param name="lparName">Lista dei nomi dei parametri da caricare.</param>
        public override void CaricaValoriParametroPropagabile(List<string> lparName)
        {
            if (lparName.Count == 0)
                return;

            string parName = lparName[0];

            switch (parName)
            {
                case "Valore":
                    if (Workflow.InfoAggiuntive.ContainsKey(this.Name + "|Valore"))
                        Workflow.InfoAggiuntive[this.Name + "|Valore"] = this.Valore;
                    else
                        Workflow.InfoAggiuntive.Add(this.Name + "|Valore", this.Valore);
                    break;
            }
        }

        // AB - 20180529: Spostato dall'interfaccia all'operazione per consentirne il richiamo anche da c#
        /// <summary>
        /// Controlla se sono stati modificati dei valori nell'operazione.
        /// </summary>
        /// <param name="valoreIniziale">Valore iniziale del controllo.</param>
        public void ControllaValoriModificati(string valoreIniziale)
        {
            if (!Valore.Equals(valoreIniziale))
                Modificata = true;
            else
                Modificata = false;
        }

        // AB - 20180412: Metodi per la gestione del formato json del valore dell'operazione
        #region Metodi per la gestione del formato json del valore dell'operazione

        /// <summary>
        /// Ritorna il valore dell'operazione in formato json.
        /// </summary>
        /// <param name="tipoElaborazione">Tipo di elaborazione dalla quale sarà consultato i valore json dell'operazione.</param>
        /// <returns>Valore dell'operazione in formato json.</returns>
        public override string ValoreJson(TipoElaborazione tipoElaborazione)
        {
            if (Criptato)
                return base.ValoreJson(tipoElaborazione);

            return Valore;
        }
        
        #endregion
    }
}
