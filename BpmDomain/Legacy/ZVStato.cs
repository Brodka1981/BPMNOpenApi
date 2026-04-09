using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ZV_Engine
{
    [Serializable]
    public class ZVStato : ZV_Engine.IZVEngineObject
    {
        public ZVStato()
        {
        }

        public event EventHandler OnClose;

        public void Close()
        {
            if (OnClose != null)
                OnClose(this, new EventArgs());
        }

        public ZVStato(ZVWorkflow iWorkflow)
        {
            workflow = iWorkflow;
            ZVAzione azioneApertura = new ZVAzione();
            azioneApertura.Name = ZVAzione.Apertura;
            azioneApertura.Persistenza = ZV_Engine.ZVAzione.TipoPersistenza.Stato;
            azioneApertura.Riepilogo = true;
            AddAzione(azioneApertura);
            ZVAzione azioneStato = new ZVAzione();
            azioneStato.Name = ZVAzione.ImpostaStato;
            AddAzione(azioneStato);
            ZVOperazione operazioneCompetenze = ZVOperazione.NewOperazione(ZVOperazione.ClasseCompetenzaStato);
            if (operazioneCompetenze != null)
                azioneStato.AddOperazione(operazioneCompetenze);
        }

        #region proprieta e metodi d'uso a Run Time (processo)
        [NonSerialized]
        ZVWorkflow workflow;
        public ZVWorkflow Workflow   // workflow di appartenenza (sempre presente sia a run time che a design)
        {
            get { return workflow; }
            set { workflow = value; }
        }
        [NonSerialized]
        bool lettore;
        [NonSerialized]
        bool setLettore;
        public bool Lettore
        {
            get
            {
                if (!setLettore)
                {
                    setLettore = true;
                    if (OperazioneCompetenze == null)
                        lettore = true;
                    else
                        lettore = OperazioneCompetenze.Eseguibile(ContestoUtilizzo.Lettura);
                }
                return lettore;
            }
        }
        [NonSerialized]
        bool autore;
        [NonSerialized]
        bool setAutore = false;
        public bool Autore
        {
            get
            {
                if (!setAutore)
                {
                    setAutore = true;
                    if (OperazioneCompetenze == null)
                        autore = true;
                    else
                        autore = OperazioneCompetenze.Eseguibile(ContestoUtilizzo.Esegui);
                }
                return autore;
            }
        }



        // Ritorna una lista di azioni che sono al momento eseguibili 
        public List<ZVAzione> AzioniEseguibili
        {
            get
            {
                return GetAzioniEseguibili(false);
            }
        }

        /// <summary>
        /// Ritorna una lista di azioni che sono al momento eseguibili (incluse nascoste se richiesto)
        /// </summary>
        /// <param name="IncludiNascoste"></param>
        /// <returns></returns>
        private List<ZVAzione> GetAzioniEseguibili(bool IncludiNascoste)
        {
            List<ZVAzione> azioniEseguibili = new List<ZVAzione>();

            //if ( this.Workflow.UtenteCorrenteEliminatore && !this.Workflow.Nuovo  )
            if (ZVWorkflow.UtenteCorrenteEliminatore && !this.Workflow.Nuovo && !this.workflow.NonConsenteElimina)
            {
                ZVAzione azs = AzioneEliminazioneDefinitiva;

                azioniEseguibili.Add(azs);
            }

            if (!Autore)
                return azioniEseguibili;

            if (interfacciaEditabile && !(Iniziale && this.Workflow.RichiedenteAnonimo) && !this.workflow.NonConsenteSalva)
            {
                ZVAzione azs = new ZVAzione(ZVAzione.Salva, this);

                // AB - 20170825: Modifica per far visualizzare solo l'icona
                azs.IconaWeb = "Save";
                azs.IsSoloIcona = true;
                // AB - 20170929: Gestione delle azioni di sistema
                azs.AzioneDiSistema = true;

                //AQ ---
                if (!this.CheckObbligatoriQuandoSalva)
                    azs.Comportamento = ZVAzione.enumComportamentoAzione.NonValidareObbl;
                else
                    azs.Comportamento = ZVAzione.enumComportamentoAzione.Normale;
                //---

                azioniEseguibili.Add(azs);
            }
            bool requisitoSeparatore = false;
            foreach (ZVAzione azione in Azioni)
            {
                if (azione.Separatore && azione.Attivo)
                    if (requisitoSeparatore)
                        break;
                    else
                        continue;

                if (!IncludiNascoste)
                    if (azione.Nascosta)
                        continue;

                if (!azione.IsAzioneStandard &&
                    azione.Eseguibile(false))
                {                                   // La singola azione è eseguibile ?
                    azioniEseguibili.Add(azione);   // Se si aggiunge alla lista da restituire
                    foreach (ZVOperazione ope in azione.OperazioniAttive)
                    {
                        if (ope.RequisitoSeparatore)
                            requisitoSeparatore = true;
                    }
                }
            }
            return azioniEseguibili;
        }


        /// <summary>
        /// Restituisce i testi di base contenuti nel design (utilizzato in ricerca)
        /// </summary>
        public virtual string TestiContenutiNelDesign()
        {
            StringBuilder AzSTRINGs = new StringBuilder();
            AzSTRINGs.AppendFormat("{0} ", Name ?? string.Empty);
            AzSTRINGs.AppendFormat("{0} ", Titolo ?? string.Empty);
            return AzSTRINGs.ToString();
        }


        public ZVAzione AzioneDaNome(string nomeAzione)
        {
            foreach (ZVAzione azione in Azioni)
                if (azione.Name.Equals(nomeAzione))
                    return azione;

            if (ZVAzione.AperturaStandard.Equals(nomeAzione))
                return this.AzioneAperturaStandard;

            if (ZVAzione.EliminazioneDefinitiva.Equals(nomeAzione))
                return this.AzioneEliminazioneDefinitiva;

            return null;
        }

        /// <summary>
        /// Ritorna l'azione specifica se questa è disponibile tra le azioni eseguibili - pregresso
        /// </summary>
        /// <param name="nomeAzione"></param>
        /// <returns></returns>
        public ZVAzione AzioneEseguibileDaNome(string nomeAzione)
        {
            return AzioneEseguibileDaNome(nomeAzione, false);
        }

        // AB - 20170208: Recupera dell'azione specificata dall'elenco delle azioni eseguibili
        /// <summary>
        /// Ritorna l'azione specifica se questa è disponibile tra le azioni eseguibili
        /// </summary>
        /// <param name="nomeAzione"></param>
        /// <param name="includiNascoste"></param>
        /// <returns></returns>
        public ZVAzione AzioneEseguibileDaNome(string nomeAzione, bool includiNascoste)
        {
            foreach (ZVAzione azione in GetAzioniEseguibili(includiNascoste))
                if (azione.Name.Equals(nomeAzione))
                    return azione;

            return null;
        }

        public ZVStato GetCopia()
        {
            MemoryStream msStatoCorrente;
            BinaryFormatter bf = new BinaryFormatter();
            msStatoCorrente = new MemoryStream();
            bf.Serialize(msStatoCorrente, this);
            msStatoCorrente.Flush();
            msStatoCorrente.Position = 0;
            ZVStato copia = ((ZVStato)bf.Deserialize(msStatoCorrente));
            copia.Workflow = this.Workflow;
            foreach (ZVAzione az in copia.Azioni)
            {
                az.Stato = copia;
                foreach (ZVOperazione op in az.Operazioni)
                    op.Azione = az;
            }
            return copia;
        }

        public bool IsInCompetenze(Dictionary<string, bool> CodiciUtente, string TipoCompetenza)
        {
            bool isInCompetenze = false;

            List<string> ls = CodiciUtente.Keys.ToList();

            foreach (ZVCompetenzeDS.ZV_CompetenzeRow rowAUT in Competenze.ZV_Competenze.Select(new StringBuilder().AppendFormat("TipoCompetenza='{0}'", TipoCompetenza).ToString()))
            {
                if (ls.FindIndex(x => x.Equals(rowAUT.Competenza, StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    isInCompetenze = true;
                    break;
                }
            }

            return isInCompetenze;
        }

        public ZVCompetenzeDS Competenze
        {
            get
            {
                return (OperazioneCompetenze as IZVCompetenze).Competenze;
            }

        }
        public void AggiornaCompetenze(List<string> tipiCompetenza)
        {
            (OperazioneCompetenze as IZVCompetenze).AggiornaCompetenze(tipiCompetenza);
        }

        public void RicaricaCompetenzeDaDB()
        {
            (OperazioneCompetenze as IZVCompetenze).RicaricaCompetenzeDaDB();
        }

        // Ritorna un Dataset con una riga per ogni volta che si è salvato questo stato (inteso come nome stato)
        public ZVStatiDS InfoStatoEseguito()
        {
            return ZVDataLayer.StatiLeggiByProcesso(IdProcesso, nomeStato);
        }

        //IdProcesso,NomeStato,Titolo,Categoria,dataInizio,dataFine,DataScadenza
        private int idStato = -1;    // -1 indica in inserimento che deve essere associato allo stato corrente. 
        public int IdStato
        {
            get { return idStato; }
            set { idStato = value; }
        }

        private string nomeStatoPrec = "";
        public string NomeStatoPrec
        {
            get { return nomeStatoPrec; }
            set { nomeStatoPrec = value; }
        }

        public int IdProcesso
        {
            get { return Workflow.IdProcesso; }    // ritorna l'id del Processo di appartenenza (se nuovo -1)
        }

        private DateTime dataInizio;         // impostato da apertura nuovo processo o cambio stato
        public DateTime DataInizio
        {
            get { return dataInizio; }
            set { dataInizio = value; }
        }

        private DateTime dataFine;           // impostato da cambio allo stato successivo o a fine workflow
        public DateTime DataFine
        {
            get { return dataFine; }
            set { dataFine = value; }
        }

        private DateTime dataScadenza;
        public DateTime DataScadenza
        {
            get { return dataScadenza; }
            set { dataScadenza = value; }
        }

        private int numeroGiorniScadenza;
        public int NumeroGiorniScadenza
        {
            get { return numeroGiorniScadenza; }
            set { numeroGiorniScadenza = value; }
        }

        public void InitStato()  // Per caricare eventuali dati presenti nel DB
        {
            setLettore = false;
            setAutore = false;

            if (workflow.CambioStatoInCorso)
                idStato = -1;
            if (IdStato >= 0)
            {
                ZVStatiDS stds = ZVDataLayer.StatiLeggi(IdStato);  // leggi con chiave id stato
                if (stds.ZV_Stati.Rows.Count == 1)
                {
                    ZVStatiDS.ZV_StatiRow row = stds.ZV_Stati.Rows[0] as ZVStatiDS.ZV_StatiRow;
                    IdStato = row.IdStato;
                    dataInizio = row.DataInizio;
                    if (!row.IsDataFineNull())// date non sempre presenti sul DB
                        dataFine = row.DataFine;
                    if (!row.IsDataScadenzaNull()) // date non sempre presenti sul DB
                        DataScadenza = row.DataScadenza;
                    Name = row.NomeStato;
                    Titolo = row.TitoloStato;
                    Categoria = row.CategoriaStato;
                    NomeStatoPrec = row.NomeStatoPrec;
                }
            }
            if (!workflow.CambioStatoInCorso)
            {
                Workflow.AzioneAperturaStandardInit();
                Workflow.AzioneAperturaStandard.InitAzione();
            }
            foreach (ZVAzione az in this.Azioni)
            { if (az.Attivo) az.InitAzione(); }
        }

        bool nuovo = false;  // Impostato a nuovo se inserimento
        public bool Nuovo
        {
            get { return nuovo || idStato < 0; }
            set { nuovo = value; }
        }

        public void Inserisci()
        {
            if (idStato >= 0)
            {
                nuovo = false;
                return;
            }
            nuovo = true;
            dataInizio = DateTime.Now;
            IdStato = ZVDataLayer.StatiAggiorna(this.GetDataSet(false));
            Workflow.StatiInseriti.Add(this);
        }

        public void Elimina()
        {
            ZVStatiDS st = GetDataSet(true);
            ZVDataLayer.StatiAggiorna(st);
        }
        // restituisce nel formato data set i dati dello stato a run time  
        public ZVStatiDS GetDataSet(bool elimina)
        {
            ZVStatiDS stds = new ZVStatiDS();
            ZVStatiDS.ZV_StatiRow row = stds.ZV_Stati.NewZV_StatiRow();
            row.IdStato = idStato;
            row.IdProcesso = IdProcesso;
            row.DataInizio = dataInizio;
            row.DataFine = dataFine;
            row.DataScadenza = DataScadenza;
            row.NomeStato = Name;
            row.TitoloStato = Titolo;
            row.CategoriaStato = Categoria;
            row.NomeStatoPrec = NomeStatoPrec;
            stds.ZV_Stati.AddZV_StatiRow(row);
            if (elimina)
            {
                row.AcceptChanges();
                row.Delete();
            }
            else
                if (IdStato > 0)
            {
                row.AcceptChanges();
                row.SetModified();
            }
            return stds;
        }



        public ZVOperazione OperazioneCompetenze
        {
            get
            {
                foreach (ZVOperazione ope in AzioneImpostaStato.Operazioni)
                {
                    if (ope.GetType().Name == ZVOperazione.ClasseCompetenzaStato)
                        return ope;
                }
                return null;
            }
        }
        #endregion

        #region Proprieta salvate a design

        private string nomeStato;
        public string Name
        {
            get { return nomeStato; }
            set { nomeStato = value; }
        }

        private string azioneCollegata;
        public string AzioneCollegata
        {
            get { return azioneCollegata; }
            set { azioneCollegata = value; }
        }

        private string titolo;
        public string Titolo
        {
            get
            {
                if (titolo == null)
                    return nomeStato;
                return titolo;
            }
            set { titolo = value; }
        }

        private bool ritornoPrec;
        public bool RitornoPrec
        {
            get { return ritornoPrec; }
            set { ritornoPrec = value; }
        }

        private string help;
        public string Help
        {
            get { return help; }
            set { help = value; }
        }

        private bool attivo = true;
        public bool Attivo
        {
            get { return attivo; }
            set
            {
                attivo = value;
                workflow.Drawer.Refresh();
            }
        }

        private bool iniziale;
        public bool Iniziale
        {
            get { return iniziale; }
            set { iniziale = value; }
        }

        private bool finale;
        public bool Finale
        {
            get { return finale; }
            set { finale = value; }
        }

        private bool duplicabile;
        public bool Duplicabile
        {
            get { return duplicabile; }
            set { duplicabile = value; }
        }

        private bool _checkObbligatoriQuandoSalva = false;
        public bool CheckObbligatoriQuandoSalva
        {
            get { return _checkObbligatoriQuandoSalva; }
            set { _checkObbligatoriQuandoSalva = value; }
        }

        private string categoria = "";
        public string Categoria
        {
            get { return categoria; }
            set { categoria = value; }
        }

        private int top = 0;
        public int Top
        {
            get { return top; }
            set { top = value; }
        }

        private int left = 0;
        public int Left
        {
            get { return left; }
            set { left = value; }
        }

        private List<ZVAzione> azioni = new List<ZVAzione>();
        public List<ZVAzione> Azioni
        {
            get { return azioni; }
            set { azioni = value; }
        }

        public ZVAzione AzioneScadenza
        {
            get
            {
                if (iniziale)
                    foreach (ZVAzione az in Azioni)
                        if (az.Name == ZVAzione.Scadenza)
                            return az;
                return null;
            }
        }

        public ZVAzione AzioneApertura
        {
            get
            {
                foreach (ZVAzione az in Azioni)
                    if (az.Name == ZVAzione.Apertura)
                        return az;
                return null;
            }
        }
        public ZVAzione AzioneAperturaStandard
        {
            get
            {
                ZVAzione az = Workflow.AzioneAperturaStandard;
                az.Stato = this;
                return az;
            }
        }

        public ZVAzione AzioneEliminazioneDefinitiva
        {
            get
            {
                ZVAzione azs = new ZVAzione(ZVAzione.EliminazioneDefinitiva, this);
                azs.ColoreWeb = "redEl";
                azs.IconaWeb = "Trash";
                azs.IsSoloIcona = true;
                azs.ChiediConferma = true;
                azs.Duplicabile = false;
                azs.Comportamento = ZVAzione.enumComportamentoAzione.NonSalvare;
                azs.Nascosta = false;
                azs.Riepilogo = false;
                azs.Etichetta = ZVAzione.EliminazioneDefinitiva;

                // AB - 20170929: Gestione delle azioni di sistema
                azs.AzioneDiSistema = true;

                azs.Stato = this;
                return azs;
            }
        }

        public ZVAzione AzioneImpostaStato
        {
            get
            {
                foreach (ZVAzione az in Azioni)
                    if (az.Name == ZVAzione.ImpostaStato)
                        return az;
                return null;
            }
        }

        private string derivanteDa;
        public string DerivanteDa
        {
            get { return derivanteDa; }
            set { derivanteDa = value; }
        }

        private bool eliminabile = false;
        public bool Eliminabile
        {
            get { return eliminabile; }
            set { eliminabile = value; }
        }

        #endregion


    }
}
