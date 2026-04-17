using System.Text;

namespace ZV_Operazioni
{
    /// <summary>
    /// Operazione numero o importo
    /// </summary>
    [Serializable, AttrWorkflow, AttrTemplate, AttrName("Modulo - Numero o Importo"), AttrRicerca, AttrAnalisi]
    public class ZVCampoNumero : ZVCampoComune
    {

        public enum TipoConversione
        {
            Non_Prevista,
            Numero,
            Formato_Assegni,
            Formato_Notarile
        }

        // numeri singoli e piccoli numeri
        private List<string> _smallNumbers = new List<string>()
            { "zero", "uno", "due", "tre", "quattro", "cinque", "sei", "sette", "otto", "nove",
      "dieci", "undici", "dodici", "tredici", "quattordici", "quindici", "sedici", "diciasette",
      "diciotto", "diciannove"};

        // numeri delle decine
        private List<string> _tens = new List<string>()
            { "", "", "venti", "trenta", "quaranta", "cinquanta", "sessanta", "settanta", "ottanta", "novanta"};

        // numeri di scala 
        private List<string> _scaleNumbersSingle = new List<string>() { "", "cento", "mille" };
        // numeri di scala 
        private List<string> _scaleNumbers = new List<string>() { "", "mila", "milioni", "miliardi" };

        /// <summary>
        /// Parametri esterni gestiti nell'operazione
        /// </summary>
        private struct ParametroEsterno
        {
            public const string Valore = "Valore";
        }

        /// <summary>
        /// Lista dei parametri esterni valorizzabili per l'operazione
        /// </summary>
        public override List<String> ParametriEsterniValorizzabili
        {
            get
            {
                string[] pars = { ParametroEsterno.Valore };
                return new List<string>(pars);
            }
        }

        private string formatoDati = "";
        /// <summary>
        /// Formato standard da applicare al valore
        /// </summary>
        public string FormatoDati
        {
            get { return formatoDati; }
            set { formatoDati = value; }
        }
        private int conversioneLettere = (int)TipoConversione.Non_Prevista;
        /// <summary>
        /// Formato standard da applicare al valore
        /// </summary>
        public int ConversioneLettere
        {
            get { return conversioneLettere; }
            set { conversioneLettere = value; }
        }


        private double valore;
        /// <summary>
        /// Valore dell'operazione.
        /// </summary>
        [AttrSalva, AttrVisibile]
        public double Valore
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
            get
            {
                if (!String.IsNullOrEmpty(FormatoSpecifico))
                {
                    return Valore.ToString(FormatoSpecifico);
                }
                else
                {
                    switch (FormatoDati)
                    {
                        case "Decimal":
                            return Valore.ToString("n");

                        case "Importo":
                            return Valore.ToString("c");

                        default:
                            return Valore.ToString("g");
                    }
                }
            }
        }
        /// <summary>
        /// Valore dell'operazione in lettere.
        /// </summary>
        [AttrVisibile]
        public string ValoreLettere
        {
            get
            {
                if (Valore == null || Valore == double.MinValue || ConversioneLettere == int.MinValue)
                {
                    return string.Empty;
                }
                else
                {
                    return NumberToWords(Valore, (ZVCampoNumero.TipoConversione)ConversioneLettere);

                }
            }
        }



        private string formatoSpecifico;
        /// <summary>
        /// Formato non standard da applicare al valore dell'operazione.
        /// </summary>
        public string FormatoSpecifico
        {
            get { return formatoSpecifico; }
            set { formatoSpecifico = value; }
        }

        private bool accettaValore0;
        /// <summary>
        /// Assume true se l'operazione accetta valoria 0, false altrimenti.
        /// </summary>
        public bool AccettaValore0
        {
            get { return accettaValore0; }
            set { accettaValore0 = value; }
        }



        [NonSerialized]
        DefRegExpDS regExpNumero;
        /// <summary>
        /// RegExp da applicare al testo editato a runtime.
        /// </summary>
        public DefRegExpDS RegExpNumero
        {
            get
            {
                if (regExpNumero == null)
                    regExpNumero = ZVDataLayer.RegExpLeggi(ZVDataLayer.TipoRegExp.workFlow, ZVDataLayer.FunzioneRegExp.Numero);

                return regExpNumero;
            }
        }

        /// <summary>
        /// Operazioni aggiuntive da eseguire alla conferma della modifica di un'operazione lato Design(ad esempio: alvare le informazioni esterne all'operazione). 
        /// </summary>
        /// <param name="contesto"></param>
        //public override Control ControlDesign(ContestoUtilizzo contesto)
        //{
        //    return new ZVCampoNumeroControl(this, contesto);
        //}

        ///// <summary>
        ///// Ritorna il controllo per l'esecuzione in ambiente WindowsForm.
        ///// </summary>
        ///// <param name="contesto">Contesto di utilizzo.</param>
        ///// <param name="interfacciaEditabile">Assume true se il controllo è editabile, false altrimenti.</param>
        ///// <returns>Controllo per l'esecuzione in ambiente WindowsForm.</returns>
        //public override Control ControlRuntime(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        //{
        //    interfacciaEditabile = false;
        //    if (!TipoControl(contesto, out interfacciaEditabile))
        //        return null;

        //    // MR - 20160323: inizializzazione dell'operazione da parametri esterni
        //    base.InizializzaDaParametriEsterni(contesto);
        //    // MR - 20160323: inizializzazione dell'operazione da info aggiuntive delle dipendenze
        //    base.InizializzaDaInfoAggiuntiveDipendenze();
        //    // Popolo le info aggiuntive previste per l'operazione
        //    PopolaInfoAggiuntive();

        //    Control ctr;
        //    ctr = new ZVCampoNumeroControlEsegui(this, contesto);
        //    ctr.Name = this.Name;
        //    //ctr.Enabled = editabile;
        //    return ctr;
        //}

        //[NonSerialized]
        //System.Web.UI.WebControls.WebControl ctr;

        ///// <summary>
        ///// Interfaccia Web dell'operazione.
        ///// </summary>
        //public System.Web.UI.WebControls.WebControl Ctr
        //{
        //    get { return ctr; }
        //    set { ctr = value; }
        //}

        ///// <summary>
        ///// Ritorna il controllo per l'esecuzione in ambiente Web.
        ///// </summary>
        ///// <param name="contesto">Contesto di utilizzo.</param>
        ///// <param name="interfacciaEditabile">Assume true se il controllo è editabile, false altrimenti.</param>
        ///// <returns>Controllo per l'esecuzione in ambiente Web.</returns>
        //public override System.Web.UI.WebControls.WebControl ControlRuntimeWeb(ContestoUtilizzo contesto, out bool interfacciaEditabile)
        //{
        //    interfacciaEditabile = false;
        //    if (!TipoControl(contesto, out interfacciaEditabile))
        //        return null;
        //    // MR - 20160323: inizializzazione dell'operazione da parametri esterni
        //    base.InizializzaDaParametriEsterni(contesto);
        //    // MR - 20160323: inizializzazione dell'operazione da info aggiuntive delle dipendenze
        //    base.InizializzaDaInfoAggiuntiveDipendenze();
        //    // Popolo le info aggiuntive previste per l'operazione
        //    PopolaInfoAggiuntive();

        //    ctr = ZVOperazione.NewWebControl("ZVCampoNumeroControlEseguiWeb", new object[2] { this, contesto });

        //    //ctr.Name = this.Name;
        //    //ctr.Enabled = editabile;e
        //    return ctr;
        //}

        /// <summary>
        /// Ritorna true se il valore specificato è numerico, false altrimenti.
        /// </summary>
        /// <param name="value">Valore da controllare.</param>
        /// <returns>True se il valore specificato è numerico, false altrimenti.</returns>
        public bool IsNumber(object value)
        {
            return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
        }

        /// <summary>
        /// Gestione del parametro esterno specificato.
        /// </summary>
        /// <param name="parName">Nome del parametro esterno da gestire.</param>
        /// <param name="value">Valore del parametro esterno da gestire.</param>
        /// <returns>True se il valore del parametro è stato applicato all'operazione, false altrimeti</returns>
        public override bool GestioneParametroEsterno(string parName, object value)
        {
            bool valoreApplicato = false;
            bool isNumber = false;

            isNumber = (IsNumber(value) || value is string);

            double num;
            if (isNumber)
            {
                switch (parName)
                {
                    //case "Valore":
                    case ParametroEsterno.Valore:
                        if (IsNumber(value))
                        {
                            Valore = double.Parse(value.ToString());
                            valoreApplicato = true;
                        }
                        else if (value is string)
                        {
                            if (double.TryParse(value.ToString(), out num))
                            {
                                Valore = num;
                                valoreApplicato = true;
                            }
                            else
                            {
                                AddErrore(this.Name, new StringBuilder().AppendFormat("Errore nei dati passati ${0}:{1}", this.Name, value).ToString());
                            }
                        }

                        break;
                }
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
            double num;

            if (double.TryParse(Workflow.InfoAggiuntive[parName].ToString(), out num))
            {
                valore = num;
            }
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
                    string key = new StringBuilder(this.Name).Append("|Valore").ToString();
                    if (Workflow.InfoAggiuntive.ContainsKey(key))
                        Workflow.InfoAggiuntive[key] = this.Valore;
                    else
                        Workflow.InfoAggiuntive.Add(key, this.Valore);
                    break;
            }
        }

        // AB - 20180529: Spostato dall'interfaccia all'operazione per consentirne il richiamo anche da c#
        /// <summary>
        /// Controlla se sono stati modificati dei valori nell'operazione.
        /// </summary>
        /// <param name="valoreIniziale">Valore iniziale del controllo.</param>
        public void ControllaValoriModificati(double valoreIniziale)
        {
            if (Valore != valoreIniziale)
                Modificata = true;
            else
                Modificata = false;
        }



        private string NumberToWords(double number, TipoConversione tipoConversione)
        {
            if (tipoConversione.Equals(ZVCampoNumero.TipoConversione.Non_Prevista))
                return string.Empty;

            // Zero rule
            //if (number == 0)
            //{
            //    return _smallNumbers[0];
            //}

            // estraiamo un numero positivo
            double positive = Math.Abs(number);

            long positiveInteger;
            positiveInteger = (long)Math.Truncate(positive);
            double decimals = positive - positiveInteger;

            int numberLength = positiveInteger.ToString().Length;
            int numDigitGroups = numberLength / 3;
            if (numberLength % 3 > 0.0)
                numDigitGroups++;

            // Array per gruppi da 3 digits
            //int[] digitGroups = new int[6];
            int[] digitGroups = new int[numDigitGroups];

            // estrai gruppi di 3 dal numero positivo senza decimali
            //for (int i = 0; i < 6; i++)
            for (int i = 0; i < numDigitGroups; i++)
            {
                digitGroups[i] = Convert.ToInt16(positiveInteger % 1000);
                positiveInteger /= 1000;
            }

            // Converto ogni gruppo in parole
            string[] groupText = new string[6];

            //for (int i = 0; i < 6; i++)
            for (int i = 0; i < numDigitGroups; i++)
            {
                groupText[i] = ThreeDigitGroupToWords(digitGroups[i]);
            }

            // Ricombino 
            StringBuilder combined = new StringBuilder(groupText[0]);

            // Process the remaining groups in turn, smallest to largest
            //for (int i = 1; i < 6; i++)
            for (int i = 1; i < numDigitGroups; i++)
            {
                // aggiungo solo quelli non a zero
                if (digitGroups[i] != 0)
                {
                    // costruisce la stringa come numero in prefisso
                    StringBuilder prefix = new StringBuilder();

                    if (digitGroups[i] == 1)
                        prefix.Append(_scaleNumbersSingle[2]);
                    else
                        prefix.Append(groupText[i]).Append(_scaleNumbers[i]);

                    // Add the three-digit group to the combined string
                    combined.Insert(0, prefix);
                }
            }

            // Negativo and zero 
            if (number < 0)
                combined.Insert(0, "meno ");
            else if (number > -1 && number < 1)
                combined.Append(_smallNumbers[0]);

            //decimali
            if (decimals >= 0)
            {
                var s = string.Format("{0:0.00000}", decimals);

                s = s.TrimEnd('0');
                if (s.Length >= 2)
                {
                    string s_decimals = s.Substring(2);
                    if (!tipoConversione.Equals(ZVCampoNumero.TipoConversione.Numero))
                    {
                        s_decimals = s_decimals.PadRight(2, '0').Substring(0, 2);
                    }
                    if (!tipoConversione.Equals(ZVCampoNumero.TipoConversione.Formato_Notarile))
                    {
                        combined.AppendFormat("{0}", (string.IsNullOrWhiteSpace(s_decimals) ? string.Empty : "/" + s_decimals));
                    }
                    else
                    {

                        int i_decimals = int.Parse(s_decimals);
                        combined.AppendFormat(" virgola {0}", (i_decimals.Equals(0) ? (_smallNumbers[0] + " " + _smallNumbers[0]) :
                                                                      (i_decimals < 10 ? _smallNumbers[0] + " " + ThreeDigitGroupToWords(i_decimals) :
                                                                      ThreeDigitGroupToWords(i_decimals))));
                    }
                }
            }

            return combined.ToString();
        }

        // Convertire ogni gruppo di 3 numeri in parole
        private string ThreeDigitGroupToWords(int threeDigits)
        {
            // inizializzazione
            StringBuilder groupText = new StringBuilder();

            // testo se inserire le centinaia e/o decine
            int hundreds = threeDigits / 100;
            int tensUnits = threeDigits % 100;

            // regole centinaia
            if (hundreds != 0)
            {
                if (hundreds != 1)
                    groupText.Append(_smallNumbers[hundreds]);

                groupText.Append(_scaleNumbersSingle[1]);
            }
            // Determino decine ed unità
            int tens = tensUnits / 10;
            int units = tensUnits % 10;

            // regole decine
            if (tens >= 2)
            {
                groupText.Append(_tens[tens]);
                if (units != 0)
                {
                    if (units == 1 || units == 8)
                        //groupText = groupText.Substring(0, groupText.Length - 1);
                        groupText.Remove(groupText.Length - 1, 1);

                    groupText.Append(_smallNumbers[units]);
                }
            }
            else if (tensUnits != 0)
                groupText.Append(_smallNumbers[tensUnits]);

            return groupText.ToString();
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

            return Valore.ToString();
        }

        /// <summary>
        /// Ritorna il tipo del valore dell'operazione in formato json.
        /// </summary>
        /// <param name="tipoElaborazione">Tipo di elaborazione dalla quale sarà consultato i valore json dell'operazione.</param>
        /// <returns>Tipo del valore dell'operazione in formato json.</returns>
        public override string TipoJson(TipoElaborazione tipoElaborazione)
        {
            if (Criptato)
                return base.TipoJson(tipoElaborazione);

            return TipiJson.TipoFloat;
        }

        #endregion
    }
}