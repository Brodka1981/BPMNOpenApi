using BpmDomain.Engine;
using BpmDomain.Engine.Interfaces;
using BpmDomain.Factories.Interfaces;
using BpmDomain.Models;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Results;
using Moq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BpmApi.Tests
{
    [TestFixture]
    public class EngineTests
    {
        private Dictionary<string, object?>? _variables = [];
        private string? _processType;
        private long _processInstanceId  = 0;
        private long _processId = 0;
        private string _xml = "";
        private WorkflowDefinition? _workflowDefinition = default;
        private string _category;
        private GetProcessInstanceSqlResult _getProcessInstanceSqlResult = new();
        private GetProcessDefinitionSqlResult? _getProcessDefinitionSqlResult = new();
        private GetVariableSqlResult? _getVariableSqlResult = new();
        private List<WorkflowDefinitionInfo> _workflowDefinitionInfos = [];
        private List<JsonObject> _fields = [];

        private readonly Mock<IDefinitionRepository> _definitionRepo = new();
        private readonly Mock<IProcessInstanceRepository> _instanceRepo = new();
        private readonly Mock<ISqlCommonRepository> _contextRepo = new();
        private readonly Mock<IBpmnParserService> _parser = new();
        private readonly Mock<IAuthorizationService> _auth = new();
        private readonly Mock<IServiceFactory> _serviceFactory = new();
        private readonly Mock<IProcessDefinitionRepository> _processDefinitionRepo = new();

        private BpmEngine? _engine;
        private string _user;
        private string _company;
        private int _totalPages = 0;

        [SetUp]
        public Task SetUp()
        {
            PopulateData();

            return Task.CompletedTask;
        }

        [Test]
        public async Task StartProcess_InputIsValid_ReturnTrue()
        {
            //arrange
            _auth.Setup(_ => _.CanStartAsync(  It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Verifiable();

            _definitionRepo.Setup(_ => _.GetDefinitionXmlAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_xml)
            .Verifiable();

            _parser.Setup(_ => _.Parse(It.IsAny<string>()))
                .Returns(_workflowDefinition)
                .Verifiable();

            _processDefinitionRepo.Setup(_ => _.GetProcessDefinitionByTypeAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_getProcessDefinitionSqlResult))
                .Verifiable();

            _instanceRepo.Setup(_ => _.SaveAsync(It.IsAny<ProcessInstance>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(_processId))
            .Verifiable();

            _engine = new BpmEngine(_instanceRepo.Object, _processDefinitionRepo.Object, _contextRepo.Object, _parser.Object, _auth.Object, _serviceFactory.Object);

            //execute test
            var result = await _engine.StartProcessAsync(  new BpmDomain.Commands.StartProcessCommand(_processType, _variables, _company, _user), default);

            var response = result ?? new BpmDomain.Results.StartProcessResult() {ProcessId = 0 } ;

            //Assert
            Assert.That(_processInstanceId, Is.LessThan(response.ProcessId));
        }

        [Test]
        public async Task GetContext_InputIsValid_ReturnTrue()
        {
            //arrange
            _auth.Setup(_ => _.CanGetContextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Verifiable();

            _contextRepo.Setup(_ => _.GetProcessInstanceAsync(It.IsAny<GetProcessInstanceSqlParms>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_getProcessInstanceSqlResult)
            .Verifiable();

            _contextRepo.Setup(_ => _.GetProcessDefinitionAsync(It.IsAny<GetProcessDefinitionSqlParms>(), It.IsAny<CancellationToken>()))
                .Returns( Task.FromResult(_getProcessDefinitionSqlResult))
                .Verifiable();

            _parser.Setup(_ => _.Parse(It.IsAny<string>()))
                .Returns(_workflowDefinition)
                .Verifiable();

            _contextRepo.Setup(_ => _.IsUserActivityCompleteAsync(It.IsAny<IsUserActivityCompleteSqlParms>(), It.IsAny<CancellationToken>()))
                .Returns( Task.FromResult(false))
                .Verifiable();

            _contextRepo.Setup(_ => _.GetVariablesAsync(It.IsAny<GetVariableSqlParms>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_getVariableSqlResult))
                .Verifiable();

            _serviceFactory.Setup(_ => _.FieldResolve("Combobox").Execute(_fields[0], _variables))
                .Returns(_fields[0])
                .Verifiable();

            _serviceFactory.Setup(_ => _.FieldResolve("Text").Execute(_fields[1], _variables))
                .Returns(_fields[1])
                .Verifiable();

            _engine = new BpmEngine(_instanceRepo.Object, _processDefinitionRepo.Object, _contextRepo.Object, _parser.Object, _auth.Object, _serviceFactory.Object);

            //execute test
            var result = await _engine.GetContextAsync(new BpmDomain.Commands.GetContextCommand(_processInstanceId, _company, _user, null, null), default);

            var response = result ?? new BpmDomain.Results.GetContextResult() { ProcessId = 0 };

            //Assert
            Assert.That(_processInstanceId, Is.EqualTo(response?.ProcessId));
        }

        [Test]
        public async Task ListDefinitions_InputIsValid_ReturnTrue()
        {
            //arrange
            _auth.Setup(_ => _.CanGetDefinitionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Verifiable();

            _processDefinitionRepo.Setup(_ => _.GetProcessDefinitionsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_workflowDefinitionInfos)
            .Verifiable();

            _engine = new BpmEngine(_instanceRepo.Object, _processDefinitionRepo.Object, _contextRepo.Object, _parser.Object, _auth.Object, _serviceFactory.Object);

            //execute test
            var result = await _engine.GetDefinitionsAsync(new BpmDomain.Commands.GetDefinitionsCommand(_category, _company, _user), default);

            var response = result ?? [new BpmDomain.Results.GetDefinitionsResult() { Category = "" }];

            //Assert
            Is.GreaterThan(response?.ToList().Count > 0);
            Assert.That(_category, Is.EqualTo(response?.FirstOrDefault()?.Category));
        }

        [Test]
        public async Task SearchProcess_InputIsValid_ReturnTrue()
        {
            //arrange
            //_auth.Setup(_ => _.CanSearchProcessAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            //.Returns(Task.FromResult(true))
            //.Verifiable();

            //_contextRepo.GetProcessInstanceSearchAsync

            //_contextRepo.Setup(_ => _.GetProcessInstanceSearchAsync(It.IsAny<GetProcessInstanceSearchSqlParms>(), It.IsAny<CancellationToken>()))
            //.ReturnsAsync(Task.FromResult() )
            //.Verifiable();

            _engine = new BpmEngine(_instanceRepo.Object, _processDefinitionRepo.Object, _contextRepo.Object, _parser.Object, _auth.Object, _serviceFactory.Object);

            //execute test
            var result = await _engine.SearchProcessAsync(new BpmDomain.Commands.SearchProcessCommand(){ Category = [_category], Columns = ["column 1"] }, default);

            var response = result ?? new BpmDomain.Results.SearchProcessResult() { TotalPages = 0 };

            //Assert
            //Is.GreaterThan(response?.ToList().Count > 0);
            //Assert.That(_totalPages, Is.EqualTo(response?.FirstOrDefault()?.TotalPages));
            Assert.Fail(); //REMOVE FAIL and complete TEST
        }

        private void PopulateData()
        {
            #region populate variables
            _user = "user1";
            _company = "99900";
            _processType = "Inserimento Anagrafica 3";
            _processInstanceId = 4;
            _processId = 7;
            _category = "POC";
            _totalPages = 1;
            #region populate xml
            _xml = @"
            <?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
                <definitions xmlns=""http://www.omg.org/spec/BPMN/20100524/MODEL"" xmlns:bpmndi=""http://www.omg.org/spec/BPMN/20100524/DI"" xmlns:dc=""http://www.omg.org/spec/DD/20100524/DC"" xmlns:di=""http://www.omg.org/spec/DD/20100524/DI"" xmlns:tns=""http://example.com/zv/bpmn"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:yaoqiang=""http://bpmn.sourceforge.net"" xmlns:zv=""http://zv-engine.com/schema/user-task"" exporter=""Yaoqiang BPMN Editor"" exporterVersion=""6.2"" expressionLanguage=""http://www.w3.org/1999/XPath"" id=""Definitions_10268"" name="""" targetNamespace=""http://example.com/zv/bpmn"" typeLanguage=""http://www.w3.org/2001/XMLSchema"" xsi:schemaLocation=""http://www.omg.org/spec/BPMN/20100524/MODEL http://bpmn.sourceforge.net/schemas/BPMN20.xsd"">
                  <process id=""WF_10268"" isClosed=""false"" isExecutable=""true"" name=""AnagraficaPoc"" processType=""None"">
                    <extensionElements>
                      <yaoqiang:description/>
                      <yaoqiang:pageFormat height=""1187.7165354330707"" imageableHeight=""1177.7165354330707"" imageableWidth=""831.8897637795276"" imageableX=""5.0"" imageableY=""5.0"" orientation=""0"" width=""841.8897637795276""/>
                      <yaoqiang:page background=""#FFFFFF"" horizontalCount=""1"" verticalCount=""1""/>
                    </extensionElements>
                    <userTask completionQuantity=""1"" id=""stato_bozza"" implementation=""##unspecified"" isForCompensation=""false"" name=""Bozza"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""15"" format=""json"" type=""apertura"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCSD.ZVIntestazioneRichiesta"",""__assembly"":""ZV_OperazioniCSD"",""__name"":""Intestazione richiesta"",""mostraNomeWorkflow"":true,""mostraDataRichiesta"":true,""mostraUtenteRichiedente"":true,""mostraEvidenza"":true,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""name"":""Intestazione richiesta"",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""interfacciaSingola"":false,""riepilogo"":false,""linfoaggiuntive"":[],""ewsMonitoraggioCapogruppo"":false,""mimetizzato"":false,""_helptooltip"":"""",""isCtrVisibile"":true,""isCtrEnabled"":true,""scriviLogSeEseguitoComeScadenza"":false},{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaWorkflow"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaWorkflow"",""_usaGruppiPEF"":false,""listaCompetenzeAggiuntive"":[{""Nome"":""Amministratori"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati da esecutore"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati da esecutore 2"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati da esecutore 3"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Incaricato"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Incaricato 2"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Incaricato 3"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""InviaMailA"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriore approvazione"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriori incaricati"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriori incaricati 2"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriori incaricati 3"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Visibilità"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true}],""gestioneUORichiedente"":false,""trattaComeOrg"":false,""richiedenteAnonimo"":false,""tipologiaRichiesta"":2,""consideraAncheViceresponsabile"":true,""scalaLaGerarchia"":false,""livelliScalabili"":999,""richiedente"":"""",""uoRichiedente"":"""",""descrizioneUoRichiedente"":"""",""gestioneUOEvasione"":false,""statiAssegnamentoUOEvasione"":[],""chiAgiscePotraSempreVedere"":false,""visibileCliente"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""idOperazione"":1,""duplicata"":false,""ctrVisibile"":false,""interfacciaSingola"":false,""riepilogo"":false,""linfoaggiuntive"":[],""uoEvasione"":"""",""usaStruttureInterbanca"":false,""mimetizzato"":false,""_helptooltip"":"""",""isCtrVisibile"":true,""isCtrEnabled"":true,""scriviLogSeEseguitoComeScadenza"":false},{""__type"":""ZV_Operazioni.ZVOggetto"",""__assembly"":""ZV_Operazioni"",""__name"":""Oggetto"",""oggettoW"":""Inserimento nuovi dati anagrafici."",""valore"":"""",""reimpostaEsegui"":false,""_control"":false,""statiSelNonVis"":[],""statiSelEdit"":[""Bozza""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Oggetto"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""name"":""Oggetto"",""idOperazione"":2,""duplicata"":false,""ctrVisibile"":false,""interfacciaSingola"":false,""riepilogo"":false,""linfoaggiuntive"":[],""fontSizeW"":"""",""oggEvidenza"":false,""mimetizzato"":false,""_helptooltip"":"""",""isCtrVisibile"":true,""isCtrEnabled"":true,""scriviLogSeEseguitoComeScadenza"":false},{""__type"":""ZV_Operazioni.ZVCampoTesto"",""__assembly"":""ZV_Operazioni"",""__name"":""Note"",""numeroRighe"":1,""nMaxCaratteri"":"""",""valore"":"""",""statiSelNonVis"":[],""statiSelEdit"":[""Bozza""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":false,""operazioniPerInizializzazione"":[],""label"":""Note"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Note"",""idOperazione"":3,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoTesto"",""__assembly"":""ZV_Operazioni"",""__name"":""Nome"",""numeroRighe"":1,""nMaxCaratteri"":"""",""placeHolder"":"""",""valore"":"""",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Nome"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Nome"",""idOperazione"":4,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoTesto"",""__assembly"":""ZV_Operazioni"",""__name"":""Cognome"",""numeroRighe"":1,""nMaxCaratteri"":"""",""placeHolder"":"""",""valore"":"""",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Cognome"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Cognome"",""idOperazione"":5,""duplicata"":true,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoData"",""__assembly"":""ZV_Operazioni"",""__name"":""Data di nascita"",""nGiorni"":0,""dataDelGiorno"":false,""valore"":""0001-01-01T00:00:00"",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Data di nascita"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Data di nascita"",""idOperazione"":6,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoComboBox"",""__assembly"":""ZV_Operazioni"",""__name"":""Sesso"",""rgxEvidenza"":{""Pattern"":""&#9;&#9;&lt;(.*?)&#9;&#9;&gt;"",""Options"":0},""SemaforoParametriEsterni"":false,""listaParametriUtente"":[],""listaValori"":[""Maschio"",""Femmina"",""Preferisco non dichiararlo""],""listaCodici"":[""1"",""2"",""3""],""valoreDefault"":""3"",""_usaQueryPerCombo"":false,""_larghezzaEspansa"":false,""_UidQueryDatiEsterni"":"""",""valore"":""3"",""codice"":"""",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":false,""operazioniPerInizializzazione"":[],""label"":""Sesso"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Sesso"",""idOperazione"":7,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoCheckBox"",""__assembly"":""ZV_Operazioni"",""__name"":""Confermo la correttezza dei dati"",""valore"":false,""valoreDef"":false,""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Confermo la correttezza dei dati"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Confermo la correttezza dei dati"",""idOperazione"":8,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{ ""type"": ""combobox"", ""name"": ""combobox1"", ""label"": ""label combobox1"", ""codici"":""V"", ""code"": ""if (BaseMethods().GetPropertyValue().Equals(&#9;&quot;V&#9;&quot;)) { if (BaseMethods().GetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;value&#9;&quot;).Contains(&#9;&quot;valorefield1&#9;&quot;)) { BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;visibile&#9;&quot;, true); } else { BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;visibile&#9;&quot;, false); } } else { BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;value&#9;&quot;, &#9;&quot;&#9;&quot;); BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;visibile&#9;&quot;, false); }""},{ ""type"": ""text"", ""name"": ""field1"", ""label"": ""label field1"", ""value"": ""valorefield1""},{ ""type"": ""text"", ""name"": ""field2"", ""label"": ""label field2""},{ ""type"": ""text"", ""name"": ""field3"", ""label"": ""label field3""},{ ""type"": ""text"", ""name"": ""ZVEwsAccantonamenti"", ""label"": ""label ZVEwsAccantonamenti"", ""code"":""CustomMethods().AlternativeViewControl(true); CustomMethods().SetIRRPrecedente(true);""},{ ""type"": ""text"", ""name"": ""ZVEwsStrategie"", ""label"": ""label ZVEwsStrategie""}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_start_stato_bozza</incoming>
                      <outgoing>flow_bozza_1</outgoing>
                    </userTask>
                    <userTask completionQuantity=""1"" id=""stato_richiestacompletata"" implementation=""##unspecified"" isForCompensation=""false"" name=""Richiesta Completata"" startQuantity=""1"">
                      <incoming>flow_inserimentodatianagrafici_7</incoming>
                      <outgoing>flow_richiestacompletata_2</outgoing>
                      <outgoing>flow_stato_richiestacompletata_end</outgoing>
                    </userTask>
                    <userTask completionQuantity=""1"" id=""stato_richiestaeliminata"" implementation=""##unspecified"" isForCompensation=""false"" name=""Richiesta Eliminata"" startQuantity=""1"">
                      <incoming>flow_bozza_15</incoming>
                      <outgoing>flow_stato_richiestaeliminata_end</outgoing>
                    </userTask>
                    <userTask completionQuantity=""1"" id=""stato_inserimentodatianagrafici"" implementation=""##unspecified"" isForCompensation=""false"" name=""Inserimento Dati Anagrafici"" startQuantity=""1"">
                      <incoming>flow_bozza_8</incoming>
                      <incoming>flow_richiestacompletata_7</incoming>
                      <outgoing>flow_inserimentodatianagrafici_2</outgoing>
                    </userTask>
                    <exclusiveGateway gatewayDirection=""Unspecified"" id=""gw_bozza"" name=""scelta"">
                      <incoming>flow_bozza_1</incoming>
                      <outgoing>flow_bozza_3</outgoing>
                      <outgoing>flow_bozza_10</outgoing>
                    </exclusiveGateway>
                    <sequenceFlow id=""flow_bozza_1"" sourceRef=""stato_bozza"" targetRef=""gw_bozza""/>
                    <serviceTask completionQuantity=""1"" id=""op_pre_bozza_2"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""prereq"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":false,""letturaRichiedente"":true,""letturaResponsabileUORichiedente"":true,""letturaUORichiedente"":true,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":1,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_bozza_3</incoming>
                      <outgoing>flow_bozza_5</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_bozza_3"" name=""Inizia compilazione"" sourceRef=""gw_bozza"" targetRef=""op_pre_bozza_2""/>
                    <serviceTask completionQuantity=""1"" id=""op_bozza_6"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCambioStato"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_Engine.ZVCambioStato"",""__assembly"":""ZV_Engine"",""__name"":""ZVCambioStato"",""nomeStatoPartenza"":"""",""nomeStatoArrivo"":"""",""idStatoPartenza"":-1,""idStatoArrivo"":-1,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""ZVCambioStato"",""idOperazione"":1,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_bozza_7</incoming>
                      <outgoing>flow_bozza_8</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_bozza_8"" name=""Inizia compilazione"" sourceRef=""op_bozza_6"" targetRef=""stato_inserimentodatianagrafici""/>
                    <serviceTask completionQuantity=""1"" id=""op_pre_bozza_9"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""prereq"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":false,""letturaRichiedente"":true,""letturaResponsabileUORichiedente"":true,""letturaUORichiedente"":true,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":1,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_bozza_10</incoming>
                      <outgoing>flow_bozza_12</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_bozza_10"" name=""Elimina richiesta"" sourceRef=""gw_bozza"" targetRef=""op_pre_bozza_9""/>
                    <serviceTask completionQuantity=""1"" id=""op_bozza_11"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":false,""letturaRichiedente"":true,""letturaResponsabileUORichiedente"":true,""letturaUORichiedente"":true,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":1,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_bozza_12</incoming>
                      <outgoing>flow_bozza_14</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_bozza_12"" name=""Elimina richiesta"" sourceRef=""op_pre_bozza_9"" targetRef=""op_bozza_11""/>
                    <serviceTask completionQuantity=""1"" id=""op_bozza_13"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCambioStato"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_Engine.ZVCambioStato"",""__assembly"":""ZV_Engine"",""__name"":""ZVCambioStato"",""nomeStatoPartenza"":"""",""nomeStatoArrivo"":"""",""idStatoPartenza"":-1,""idStatoArrivo"":-1,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""ZVCambioStato"",""idOperazione"":1,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_bozza_14</incoming>
                      <outgoing>flow_bozza_15</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_bozza_14"" name=""Elimina richiesta"" sourceRef=""op_bozza_11"" targetRef=""op_bozza_13""/>
                    <sequenceFlow id=""flow_bozza_15"" name=""Elimina richiesta"" sourceRef=""op_bozza_13"" targetRef=""stato_richiestaeliminata""/>
                    <serviceTask completionQuantity=""1"" id=""op_pre_richiestacompletata_1"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""prereq"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":true,""letturaRichiedente"":false,""letturaResponsabileUORichiedente"":false,""letturaUORichiedente"":false,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":3,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_richiestacompletata_2</incoming>
                      <outgoing>flow_richiestacompletata_4</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_richiestacompletata_2"" name=""Modifica dati"" sourceRef=""stato_richiestacompletata"" targetRef=""op_pre_richiestacompletata_1""/>
                    <serviceTask completionQuantity=""1"" id=""op_richiestacompletata_3"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":true,""letturaRichiedente"":false,""letturaResponsabileUORichiedente"":false,""letturaUORichiedente"":false,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":3,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_richiestacompletata_4</incoming>
                      <outgoing>flow_richiestacompletata_6</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_richiestacompletata_4"" name=""Modifica dati"" sourceRef=""op_pre_richiestacompletata_1"" targetRef=""op_richiestacompletata_3""/>
                    <serviceTask completionQuantity=""1"" id=""op_richiestacompletata_5"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCambioStato"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_Engine.ZVCambioStato"",""__assembly"":""ZV_Engine"",""__name"":""ZVCambioStato"",""nomeStatoPartenza"":"""",""nomeStatoArrivo"":"""",""idStatoPartenza"":-1,""idStatoArrivo"":-1,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""ZVCambioStato"",""idOperazione"":1,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_richiestacompletata_6</incoming>
                      <outgoing>flow_richiestacompletata_7</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_richiestacompletata_6"" name=""Modifica dati"" sourceRef=""op_richiestacompletata_3"" targetRef=""op_richiestacompletata_5""/>
                    <sequenceFlow id=""flow_richiestacompletata_7"" name=""Modifica dati"" sourceRef=""op_richiestacompletata_5"" targetRef=""stato_inserimentodatianagrafici""/>
                    <serviceTask completionQuantity=""1"" id=""op_pre_inserimentodatianagrafici_1"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""prereq"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":false,""letturaRichiedente"":true,""letturaResponsabileUORichiedente"":true,""letturaUORichiedente"":true,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":1,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_inserimentodatianagrafici_2</incoming>
                      <outgoing>flow_inserimentodatianagrafici_4</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_inserimentodatianagrafici_2"" name=""Termina compilazione"" sourceRef=""stato_inserimentodatianagrafici"" targetRef=""op_pre_inserimentodatianagrafici_1""/>
                    <serviceTask completionQuantity=""1"" id=""op_inserimentodatianagrafici_3"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":false,""letturaRichiedente"":true,""letturaResponsabileUORichiedente"":true,""letturaUORichiedente"":true,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":1,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_inserimentodatianagrafici_4</incoming>
                      <outgoing>flow_inserimentodatianagrafici_6</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_inserimentodatianagrafici_4"" name=""Termina compilazione"" sourceRef=""op_pre_inserimentodatianagrafici_1"" targetRef=""op_inserimentodatianagrafici_3""/>
                    <serviceTask completionQuantity=""1"" id=""op_inserimentodatianagrafici_5"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCambioStato"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_Engine.ZVCambioStato"",""__assembly"":""ZV_Engine"",""__name"":""ZVCambioStato"",""nomeStatoPartenza"":"""",""nomeStatoArrivo"":"""",""idStatoPartenza"":-1,""idStatoArrivo"":-1,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""ZVCambioStato"",""idOperazione"":1,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_inserimentodatianagrafici_6</incoming>
                      <outgoing>flow_inserimentodatianagrafici_7</outgoing>
                    </serviceTask>
                    <sequenceFlow id=""flow_inserimentodatianagrafici_6"" name=""Termina compilazione"" sourceRef=""op_inserimentodatianagrafici_3"" targetRef=""op_inserimentodatianagrafici_5""/>
                    <sequenceFlow id=""flow_inserimentodatianagrafici_7"" name=""Termina compilazione"" sourceRef=""op_inserimentodatianagrafici_5"" targetRef=""stato_richiestacompletata""/>
                    <startEvent id=""start_stato_bozza"" isInterrupting=""true"" parallelMultiple=""false"">
                      <outgoing>flow_start_stato_bozza</outgoing>
                      <outputSet/>
                    </startEvent>
                    <sequenceFlow id=""flow_start_stato_bozza"" sourceRef=""start_stato_bozza"" targetRef=""stato_bozza""/>
                    <sequenceFlow id=""flow_stato_richiestacompletata_end"" sourceRef=""stato_richiestacompletata"" targetRef=""end_stato_richiestacompletata""/>
                    <endEvent id=""end_stato_richiestaeliminata"">
                      <incoming>flow_stato_richiestaeliminata_end</incoming>
                      <inputSet/>
                    </endEvent>
                    <sequenceFlow id=""flow_stato_richiestaeliminata_end"" sourceRef=""stato_richiestaeliminata"" targetRef=""end_stato_richiestaeliminata""/>
                    <endEvent id=""end_stato_richiestacompletata"">
                      <incoming>flow_stato_richiestacompletata_end</incoming>
                      <inputSet/>
                    </endEvent>
                    <sequenceFlow id=""flow_bozza_7"" name=""Inizia compilazione"" sourceRef=""op_bozza_4"" targetRef=""op_bozza_6""/>
                    <sequenceFlow id=""flow_bozza_5"" name=""Inizia compilazione"" sourceRef=""op_pre_bozza_2"" targetRef=""op_bozza_4""/>
                    <serviceTask completionQuantity=""1"" id=""op_bozza_4"" implementation=""##WebService"" isForCompensation=""false"" name=""ZVCompetenzaAzione"" startQuantity=""1"">
                      <extensionElements>
                        <zv:form version=""1.0"">
                          <form>
                            <zv:ui count=""1"" format=""json"" type=""task"">
                              <ui><![CDATA[[{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaAzione"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaAzione"",""competenzeDiStato"":true,""competenzeLetturaPEF"":[],""competenzeScritturaPEF"":[],""modalitaEsecuzione"":1,""sceltaTipoInformazioni"":1,""informazioniVisibiliA"":[""VISINFO_RICH"",""VISINFO_RESPUO"",""VISINFO_UO""],""esecuzioneCompetenzaDiStato"":true,""esecuzioneRichiedente"":false,""esecuzioneResponsabileUODelRichiedente"":false,""esecuzioneUODelRichiedente"":false,""esecuzioneListaCompetenzeAggiuntive"":[],""esecuzioneTuttiVisualizzatoriRichiesta"":false,""letturaVisibilitaStatiInseriti"":false,""letturaTipoDatiInseriti"":false,""letturaRichiedente"":true,""letturaResponsabileUORichiedente"":true,""letturaUORichiedente"":true,""letturaTuttiVisualizzatoriRichiesta"":true,""letturaListaCompetenzeAggiuntive"":[],""letturaUtenteCheHaEseguitoAzione"":false,""letturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""letturaCompetenzaDiModificaDelloStatoCorrente"":false,""letturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""scritturaDatiAzioneModificabili"":1,""scritturaUtenteCheHaEseguitoAzione"":false,""scritturaRichiedente"":false,""scritturaRespUORichiedente"":false,""scritturaUORichiedente"":false,""scritturaListaCompetenzeAggiuntive"":[],""scritturaCompetenzaDiModificaDelloStatoDiInserimento"":false,""scritturaCompetenzaDiModificaDelloStatoCorrente"":true,""scritturaTutteLeCompetenzeDiModificaIntervenuteECheInterverranno"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]}]]]></ui>
                            </zv:ui>
                          </form>
                        </zv:form>
                      </extensionElements>
                      <incoming>flow_bozza_5</incoming>
                      <outgoing>flow_bozza_7</outgoing>
                    </serviceTask>
                    <textAnnotation id=""note_ui_stato_bozza"" textFormat=""text/plain"">
                      <text>UI:
                 - ZVIntestazioneRichiesta
                 - ZVCompetenzaWorkflow
                 - ZVOggetto
                 - ZVCampoTesto
                 - ZVCampoTesto
                 - ZVCampoTesto
                 - ZVCampoData
                 - ZVCampoComboBox
                 - ZVCampoCheckBox</text>
                    </textAnnotation>
                    <association associationDirection=""None"" id=""assoc_note_ui_stato_bozza"" sourceRef=""stato_bozza"" targetRef=""note_ui_stato_bozza""/>
                  </process>
                  <bpmndi:BPMNDiagram id=""Yaoqiang_Diagram-WF_10268"" name=""Untitled Diagram"" resolution=""96.0"">
                    <bpmndi:BPMNPlane bpmnElement=""WF_10268"">
                      <bpmndi:BPMNShape bpmnElement=""note_ui_stato_bozza"" id=""Yaoqiang-note_ui_stato_bozza"">
                        <dc:Bounds height=""-1.0"" width=""-1.0"" x=""188.0"" y=""431.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""154.84"" width=""138.0"" x=""188.0"" y=""355.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""end_stato_richiestaeliminata"" id=""Yaoqiang-end_stato_richiestaeliminata"">
                        <dc:Bounds height=""32.0"" width=""32.0"" x=""939.0"" y=""228.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""952.0"" y=""268.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""start_stato_bozza"" id=""Yaoqiang-start_stato_bozza"">
                        <dc:Bounds height=""32.0"" width=""32.0"" x=""20.0"" y=""200.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""33.0"" y=""240.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_inserimentodatianagrafici_5"" id=""Yaoqiang-op_inserimentodatianagrafici_5"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""1237.0"" y=""56.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""1242.0"" y=""68.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_inserimentodatianagrafici_3"" id=""Yaoqiang-op_inserimentodatianagrafici_3"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""1084.0"" y=""56.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""1089.0"" y=""68.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_pre_inserimentodatianagrafici_1"" id=""Yaoqiang-op_pre_inserimentodatianagrafici_1"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""935.0"" y=""47.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""940.0"" y=""59.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_richiestacompletata_5"" id=""Yaoqiang-op_richiestacompletata_5"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""1395.0"" y=""392.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""1400.0"" y=""404.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_richiestacompletata_3"" id=""Yaoqiang-op_richiestacompletata_3"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""1394.0"" y=""296.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""1399.0"" y=""308.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_pre_richiestacompletata_1"" id=""Yaoqiang-op_pre_richiestacompletata_1"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""1394.0"" y=""203.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""1399.0"" y=""215.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_bozza_13"" id=""Yaoqiang-op_bozza_13"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""649.0"" y=""216.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""654.0"" y=""228.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_bozza_11"" id=""Yaoqiang-op_bozza_11"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""504.0"" y=""216.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""509.0"" y=""228.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_pre_bozza_9"" id=""Yaoqiang-op_pre_bozza_9"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""359.0"" y=""216.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""364.0"" y=""228.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_bozza_6"" id=""Yaoqiang-op_bozza_6"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""649.0"" y=""121.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""654.0"" y=""133.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_bozza_4"" id=""Yaoqiang-op_bozza_4"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""504.0"" y=""121.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""509.0"" y=""133.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""op_pre_bozza_2"" id=""Yaoqiang-op_pre_bozza_2"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""359.0"" y=""121.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""75.0"" x=""364.0"" y=""133.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""gw_bozza"" id=""Yaoqiang-gw_bozza"" isMarkerVisible=""true"">
                        <dc:Bounds height=""42.0"" width=""42.0"" x=""257.0"" y=""175.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""35.0"" x=""260.5"" y=""219.5""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""stato_inserimentodatianagrafici"" id=""Yaoqiang-stato_inserimentodatianagrafici"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""794.0"" y=""121.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""77.0"" x=""798.0"" y=""133.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""stato_richiestaeliminata"" id=""Yaoqiang-stato_richiestaeliminata"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""794.0"" y=""216.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""50.0"" x=""811.5"" y=""228.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""stato_richiestacompletata"" id=""Yaoqiang-stato_richiestacompletata"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""1382.0"" y=""55.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""34.84"" width=""59.0"" x=""1395.0"" y=""67.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""stato_bozza"" id=""Yaoqiang-stato_bozza"">
                        <dc:Bounds height=""55.0"" width=""85.0"" x=""112.0"" y=""188.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""37.0"" x=""136.0"" y=""208.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNShape bpmnElement=""end_stato_richiestacompletata"" id=""Yaoqiang-end_stato_richiestacompletata"">
                        <dc:Bounds height=""32.0"" width=""32.0"" x=""1535.0"" y=""67.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""1548.0"" y=""107.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNShape>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_15"" id=""Yaoqiang-flow_bozza_15"">
                        <di:waypoint x=""733.9705882352941"" y=""244.0""/>
                        <di:waypoint x=""793.9705882352941"" y=""244.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""82.0"" x=""722.97"" y=""234.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_14"" id=""Yaoqiang-flow_bozza_14"">
                        <di:waypoint x=""588.9705882352941"" y=""244.0""/>
                        <di:waypoint x=""648.9705882352941"" y=""244.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""82.0"" x=""577.97"" y=""234.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_12"" id=""Yaoqiang-flow_bozza_12"">
                        <di:waypoint x=""443.97058823529414"" y=""244.0""/>
                        <di:waypoint x=""503.97058823529414"" y=""244.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""82.0"" x=""432.97"" y=""234.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_10"" id=""Yaoqiang-flow_bozza_10"">
                        <di:waypoint x=""298.47058823529414"" y=""196.5""/>
                        <di:waypoint x=""358.97058823529414"" y=""244.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""82.0"" x=""288.0"" y=""210.05""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_inserimentodatianagrafici_7"" id=""Yaoqiang-flow_inserimentodatianagrafici_7"">
                        <di:waypoint x=""1321.9705882352941"" y=""84.0""/>
                        <di:waypoint x=""1381.9705882352941"" y=""83.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""109.0"" x=""1297.5"" y=""73.61""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_inserimentodatianagrafici_6"" id=""Yaoqiang-flow_inserimentodatianagrafici_6"">
                        <di:waypoint x=""1168.9705882352941"" y=""84.0""/>
                        <di:waypoint x=""1236.9705882352941"" y=""84.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""109.0"" x=""1148.47"" y=""74.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_inserimentodatianagrafici_4"" id=""Yaoqiang-flow_inserimentodatianagrafici_4"">
                        <di:waypoint x=""1019.9705882352941"" y=""75.0""/>
                        <di:waypoint x=""1083.9705882352941"" y=""84.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""109.0"" x=""997.5"" y=""69.55""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_inserimentodatianagrafici_2"" id=""Yaoqiang-flow_inserimentodatianagrafici_2"">
                        <di:waypoint x=""878.9705882352941"" y=""149.0""/>
                        <di:waypoint x=""934.9705882352941"" y=""75.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""109.0"" x=""852.5"" y=""102.11""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""assoc_note_ui_stato_bozza"" id=""Yaoqiang-assoc_note_ui_stato_bozza"">
                        <di:waypoint x=""158.70512820512823"" y=""244.0""/>
                        <di:waypoint x=""187.5"" y=""431.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""170.1"" y=""327.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_stato_richiestaeliminata_end"" id=""Yaoqiang-flow_stato_richiestaeliminata_end"">
                        <di:waypoint x=""878.9705882352941"" y=""244.0""/>
                        <di:waypoint x=""938.9705882352941"" y=""244.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""905.97"" y=""234.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_richiestacompletata_7"" id=""Yaoqiang-flow_richiestacompletata_7"">
                        <di:waypoint x=""1394.9705882352941"" y=""420.0""/>
                        <di:waypoint x=""878.9705882352941"" y=""149.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""66.0"" x=""1104.0"" y=""274.55""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_richiestacompletata_6"" id=""Yaoqiang-flow_richiestacompletata_6"">
                        <di:waypoint x=""1437.0"" y=""352.0""/>
                        <di:waypoint x=""1437.0"" y=""393.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""66.0"" x=""1404.0"" y=""362.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_8"" id=""Yaoqiang-flow_bozza_8"">
                        <di:waypoint x=""733.9705882352941"" y=""149.0""/>
                        <di:waypoint x=""793.9705882352941"" y=""149.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""95.0"" x=""716.47"" y=""139.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_7"" id=""Yaoqiang-flow_bozza_7"">
                        <di:waypoint x=""588.9705882352941"" y=""149.0""/>
                        <di:waypoint x=""648.9705882352941"" y=""149.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""95.0"" x=""571.47"" y=""139.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_richiestacompletata_4"" id=""Yaoqiang-flow_richiestacompletata_4"">
                        <di:waypoint x=""1436.5"" y=""259.0""/>
                        <di:waypoint x=""1436.5"" y=""297.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""66.0"" x=""1403.5"" y=""268.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_5"" id=""Yaoqiang-flow_bozza_5"">
                        <di:waypoint x=""443.97058823529414"" y=""149.0""/>
                        <di:waypoint x=""503.97058823529414"" y=""149.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""95.0"" x=""426.47"" y=""139.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_richiestacompletata_2"" id=""Yaoqiang-flow_richiestacompletata_2"">
                        <di:waypoint x=""1430.5"" y=""111.0""/>
                        <di:waypoint x=""1430.5"" y=""204.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""66.0"" x=""1397.5"" y=""147.58""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_3"" id=""Yaoqiang-flow_bozza_3"">
                        <di:waypoint x=""298.47058823529414"" y=""196.5""/>
                        <di:waypoint x=""358.97058823529414"" y=""149.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""95.0"" x=""281.5"" y=""163.11""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_bozza_1"" id=""Yaoqiang-flow_bozza_1"">
                        <di:waypoint x=""196.97058823529414"" y=""216.0""/>
                        <di:waypoint x=""257.47058823529414"" y=""196.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""224.0"" y=""196.11""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_stato_richiestacompletata_end"" id=""Yaoqiang-flow_stato_richiestacompletata_end"">
                        <di:waypoint x=""1466.9705882352941"" y=""83.0""/>
                        <di:waypoint x=""1534.9784026435746"" y=""83.5""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""1498.0"" y=""73.31""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                      <bpmndi:BPMNEdge bpmnElement=""flow_start_stato_bozza"" id=""Yaoqiang-flow_start_stato_bozza"">
                        <di:waypoint x=""51.970588235294144"" y=""216.0""/>
                        <di:waypoint x=""111.97058823529414"" y=""216.0""/>
                        <bpmndi:BPMNLabel>
                          <dc:Bounds height=""19.84"" width=""6.0"" x=""78.97"" y=""206.08""/>
                        </bpmndi:BPMNLabel>
                      </bpmndi:BPMNEdge>
                    </bpmndi:BPMNPlane>
                  </bpmndi:BPMNDiagram>
                </definitions>
            ";
            _xml = _xml.Replace("\r\n", "").Replace("\"",@"\""");
            #endregion

            _variables = new Dictionary<string, object?>() 
            {
                { "field1", "valorefield1" },
                { "variableName2", "VariableValue2" },
            };

            #endregion

            #region populate json
            var fieldsJson = @"[{""__type"":""ZV_OperazioniCSD.ZVIntestazioneRichiesta"",""__assembly"":""ZV_OperazioniCSD"",""__name"":""Intestazione richiesta"",""mostraNomeWorkflow"":true,""mostraDataRichiesta"":true,""mostraUtenteRichiedente"":true,""mostraEvidenza"":true,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""name"":""Intestazione richiesta"",""idOperazione"":0,""duplicata"":false,""ctrVisibile"":false,""interfacciaSingola"":false,""riepilogo"":false,""linfoaggiuntive"":[],""ewsMonitoraggioCapogruppo"":false,""mimetizzato"":false,""_helptooltip"":"""",""isCtrVisibile"":true,""isCtrEnabled"":true,""scriviLogSeEseguitoComeScadenza"":false},{""__type"":""ZV_OperazioniCompetenzeCSD.ZVCompetenzaWorkflow"",""__assembly"":""ZV_OperazioniCompetenzeCSD"",""__name"":""CompetenzaWorkflow"",""_usaGruppiPEF"":false,""listaCompetenzeAggiuntive"":[{""Nome"":""Amministratori"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati da esecutore"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati da esecutore 2"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Avvisati da esecutore 3"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Incaricato"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Incaricato 2"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Incaricato 3"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""InviaMailA"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriore approvazione"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriori incaricati"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriori incaricati 2"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Ulteriori incaricati 3"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true},{""Nome"":""Visibilità"",""VisibileInSezioneCompetenze"":true,""CodiciRS"":[],""DescrizioniRS"":[],""CreataInTemplate"":true}],""gestioneUORichiedente"":false,""trattaComeOrg"":false,""richiedenteAnonimo"":false,""tipologiaRichiesta"":2,""consideraAncheViceresponsabile"":true,""scalaLaGerarchia"":false,""livelliScalabili"":999,""richiedente"":"""",""uoRichiedente"":"""",""descrizioneUoRichiedente"":"""",""gestioneUOEvasione"":false,""statiAssegnamentoUOEvasione"":[],""chiAgiscePotraSempreVedere"":false,""visibileCliente"":false,""label"":"""",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""idOperazione"":1,""duplicata"":false,""ctrVisibile"":false,""interfacciaSingola"":false,""riepilogo"":false,""linfoaggiuntive"":[],""uoEvasione"":"""",""usaStruttureInterbanca"":false,""mimetizzato"":false,""_helptooltip"":"""",""isCtrVisibile"":true,""isCtrEnabled"":true,""scriviLogSeEseguitoComeScadenza"":false},{""__type"":""ZV_Operazioni.ZVOggetto"",""__assembly"":""ZV_Operazioni"",""__name"":""Oggetto"",""oggettoW"":""Inserimento nuovi dati anagrafici."",""valore"":"""",""reimpostaEsegui"":false,""_control"":false,""statiSelNonVis"":[],""statiSelEdit"":[""Bozza""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Oggetto"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""name"":""Oggetto"",""idOperazione"":2,""duplicata"":false,""ctrVisibile"":false,""interfacciaSingola"":false,""riepilogo"":false,""linfoaggiuntive"":[],""fontSizeW"":"""",""oggEvidenza"":false,""mimetizzato"":false,""_helptooltip"":"""",""isCtrVisibile"":true,""isCtrEnabled"":true,""scriviLogSeEseguitoComeScadenza"":false},{""__type"":""ZV_Operazioni.ZVCampoTesto"",""__assembly"":""ZV_Operazioni"",""__name"":""Note"",""numeroRighe"":1,""nMaxCaratteri"":"""",""valore"":"""",""statiSelNonVis"":[],""statiSelEdit"":[""Bozza""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":false,""operazioniPerInizializzazione"":[],""label"":""Note"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Note"",""idOperazione"":3,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoTesto"",""__assembly"":""ZV_Operazioni"",""__name"":""Nome"",""numeroRighe"":1,""nMaxCaratteri"":"""",""placeHolder"":"""",""valore"":"""",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Nome"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Nome"",""idOperazione"":4,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoTesto"",""__assembly"":""ZV_Operazioni"",""__name"":""Cognome"",""numeroRighe"":1,""nMaxCaratteri"":"""",""placeHolder"":"""",""valore"":"""",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Cognome"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Cognome"",""idOperazione"":5,""duplicata"":true,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoData"",""__assembly"":""ZV_Operazioni"",""__name"":""Data di nascita"",""nGiorni"":0,""dataDelGiorno"":false,""valore"":""0001-01-01T00:00:00"",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Data di nascita"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Data di nascita"",""idOperazione"":6,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoComboBox"",""__assembly"":""ZV_Operazioni"",""__name"":""Sesso"",""rgxEvidenza"":{""Pattern"":""&#9;&#9;&lt;(.*?)&#9;&#9;&gt;"",""Options"":0},""SemaforoParametriEsterni"":false,""listaParametriUtente"":[],""listaValori"":[""Maschio"",""Femmina"",""Preferisco non dichiararlo""],""listaCodici"":[""1"",""2"",""3""],""valoreDefault"":""3"",""_usaQueryPerCombo"":false,""_larghezzaEspansa"":false,""_UidQueryDatiEsterni"":"""",""valore"":""3"",""codice"":"""",""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":false,""operazioniPerInizializzazione"":[],""label"":""Sesso"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Sesso"",""idOperazione"":7,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{""__type"":""ZV_Operazioni.ZVCampoCheckBox"",""__assembly"":""ZV_Operazioni"",""__name"":""Confermo la correttezza dei dati"",""valore"":false,""valoreDef"":false,""statiSelNonVis"":[""Bozza"",""Richiesta Eliminata""],""statiSelEdit"":[""Inserimento Dati Anagrafici""],""ctrHeight"":30,""requisito"":"""",""colore"":""White"",""regExp"":"""",""codRegExp"":"""",""_forzaNonObbligatorio"":false,""obbligatorio"":true,""operazioniPerInizializzazione"":[],""label"":""Confermo la correttezza dei dati"",""attivo"":true,""ctrAttivo"":true,""ctrNONModRiep"":false,""criptato"":false,""mimetizzato"":false,""_helptooltip"":"""",""name"":""Confermo la correttezza dei dati"",""idOperazione"":8,""duplicata"":false,""ctrVisibile"":false,""isCtrVisibile"":true,""isCtrEnabled"":true,""interfacciaSingola"":false,""riepilogo"":false,""scriviLogSeEseguitoComeScadenza"":false,""linfoaggiuntive"":[]},{ ""type"": ""combobox"", ""name"": ""combobox1"", ""label"": ""label combobox1"", ""codici"":""V"", ""code"": ""if (BaseMethods().GetPropertyValue().Equals(&#9;&quot;V&#9;&quot;)) { if (BaseMethods().GetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;value&#9;&quot;).Contains(&#9;&quot;valorefield1&#9;&quot;)) { BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;visibile&#9;&quot;, true); } else { BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;visibile&#9;&quot;, false); } } else { BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;value&#9;&quot;, &#9;&quot;&#9;&quot;); BaseMethods().SetPropertyByName(&#9;&quot;field1&#9;&quot;, &#9;&quot;visibile&#9;&quot;, false); }""},{ ""type"": ""text"", ""name"": ""field1"", ""label"": ""label field1"", ""value"": ""valorefield1""},{ ""type"": ""text"", ""name"": ""field2"", ""label"": ""label field2""},{ ""type"": ""text"", ""name"": ""field3"", ""label"": ""label field3""},{ ""type"": ""text"", ""name"": ""ZVEwsAccantonamenti"", ""label"": ""label ZVEwsAccantonamenti"", ""code"":""CustomMethods().AlternativeViewControl(true); CustomMethods().SetIRRPrecedente(true);""},{ ""type"": ""text"", ""name"": ""ZVEwsStrategie"", ""label"": ""label ZVEwsStrategie""}]";

            var _workflowDefinitionJson = @"{""Id"":""WF_10268"",""Name"":""AnagraficaPoc"",""StartEventId"":""start_stato_bozza"",""GlobalForm"":{""Fields"":[],""Actions"":[]},""States"":[{""Actions"":[{""Id"":""7407a52d-d89b-4451-850f-10ff9eba3139"",""Label"":"""",""TargetNodeId"":""gw_bozza"",""Requirements"":[]}],""Id"":""stato_bozza"",""Name"":""Bozza"",""Fields"":[{""Json"":""" + fieldsJson.Replace("\"", @"\""") + @"""}]},{""Actions"":[{""Id"":""4f81b836-329b-4892-bcba-6d0b23359f0a"",""Label"":"""",""TargetNodeId"":""op_pre_richiestacompletata_1"",""Requirements"":[]},{""Id"":""5069ee7e-c12d-4325-bef3-8634f842f44b"",""Label"":"""",""TargetNodeId"":""end_stato_richiestacompletata"",""Requirements"":[]}],""Id"":""stato_richiestacompletata"",""Name"":""Richiesta Completata"",""Fields"":[]},{""Actions"":[{""Id"":""e82fb6fd-bcca-4b0d-8fe3-1286c7668e94"",""Label"":"""",""TargetNodeId"":""end_stato_richiestaeliminata"",""Requirements"":[]}],""Id"":""stato_richiestaeliminata"",""Name"":""Richiesta Eliminata"",""Fields"":[]},{""Actions"":[{""Id"":""5e9acd04-05f8-44c4-bbb7-f34e6b740388"",""Label"":"""",""TargetNodeId"":""op_pre_inserimentodatianagrafici_1"",""Requirements"":[]}],""Id"":""stato_inserimentodatianagrafici"",""Name"":""Inserimento Dati Anagrafici"",""Fields"":[]}],""ActionTasks"":[],""SystemTasks"":[{""Id"":""op_pre_bozza_2"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_bozza_4"",""Parameters"":{}},{""Id"":""op_bozza_6"",""Label"":""ZVCambioStato"",""Type"":""CambioStato"",""NextNodeId"":""stato_inserimentodatianagrafici"",""Parameters"":{}},{""Id"":""op_pre_bozza_9"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_bozza_11"",""Parameters"":{}},{""Id"":""op_bozza_11"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_bozza_13"",""Parameters"":{}},{""Id"":""op_bozza_13"",""Label"":""ZVCambioStato"",""Type"":""CambioStato"",""NextNodeId"":""stato_richiestaeliminata"",""Parameters"":{}},{""Id"":""op_pre_richiestacompletata_1"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_richiestacompletata_3"",""Parameters"":{}},{""Id"":""op_richiestacompletata_3"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_richiestacompletata_5"",""Parameters"":{}},{""Id"":""op_richiestacompletata_5"",""Label"":""ZVCambioStato"",""Type"":""CambioStato"",""NextNodeId"":""stato_inserimentodatianagrafici"",""Parameters"":{}},{""Id"":""op_pre_inserimentodatianagrafici_1"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_inserimentodatianagrafici_3"",""Parameters"":{}},{""Id"":""op_inserimentodatianagrafici_3"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_inserimentodatianagrafici_5"",""Parameters"":{}},{""Id"":""op_inserimentodatianagrafici_5"",""Label"":""ZVCambioStato"",""Type"":""CambioStato"",""NextNodeId"":""stato_richiestacompletata"",""Parameters"":{}},{""Id"":""op_bozza_4"",""Label"":""ZVCompetenzaAzione"",""Type"":""ZVCompetenzaAzione"",""NextNodeId"":""op_bozza_6"",""Parameters"":{}}],""Gateways"":[{""Id"":""gw_bozza"",""Label"":""scelta"",""Outgoing"":[{""Condition"":"""",""TargetNodeId"":""op_pre_bozza_2""},{""Condition"":"""",""TargetNodeId"":""op_pre_bozza_9""}]}]}";

            var _getProcessInstanceSqlResultJson = @"{""ProcessInstanceId"":4,""ProcessDefinitionId"":1,""Status"":""wait"",""StartedAt"":""2026-03-30T09:00:00"",""CurrentNodeId"":""stato_bozza"",""CurrentUserTaskId"":"""",""CompletedAt"":null,""LastUpdatedAt"":""2026-03-30T09:00:00"",""TenantId"":1}";

            var _getProcessDefinitionSqlResultJson = @"{""ProcessDefinitionId"":1,""Type"":""Inserimento Anagrafica"",""Category"":null,""Name"":""Inserimento Anagrafica"",""BpmnXml"":"""+ _xml + @""",""IsActive"":true,""CreatedAt"":""2026-03-23T16:00:25"",""CreatedBy"":"""",""TenantId"":1}";

            var _getVariableSqlResultJson = @"{""List"":[{""VariableId"":1,""TenantId"":1,""ProcessInstanceId"":4,""Type"":""field"",""Name"":""field1"",""ValueType"":""string"",""ValueString"":""valorefield1"",""ValueNumber"":null,""ValueDate"":null,""ValueBoolean"":null,""ValueJson"":""""},{""VariableId"":2,""TenantId"":1,""ProcessInstanceId"":4,""Type"":""variable"",""Name"":""variable1"",""ValueType"":""number"",""ValueString"":"""",""ValueNumber"":1.0000000000,""ValueDate"":null,""ValueBoolean"":null,""ValueJson"":""""},{""VariableId"":3,""TenantId"":1,""ProcessInstanceId"":4,""Type"":""warning"",""Name"":""warning1"",""ValueType"":""string"",""ValueString"":""valorewarning1"",""ValueNumber"":null,""ValueDate"":null,""ValueBoolean"":null,""ValueJson"":""""}]}";
            
            var _workflowDefinitionInfosJson = @"[{""ProcessType"":""Inserimento Anagrafica"",""Name"":""Inserimento Anagrafica"",""Category"":""POC""},{""ProcessType"":""Inserimento Anagrafica 2"",""Name"":""Inserimento Anagrafica 2"",""Category"":""POC""},{""ProcessType"":""Inserimento Anagrafica 3"",""Name"":""Inserimento Anagrafica 3"",""Category"":""POC""}]";
            
            #endregion

            #region populate Deserialize
            var workflowDefinition = JsonSerializer.Deserialize<WorkflowDefinition>(_workflowDefinitionJson);

            if (workflowDefinition != null)
                _workflowDefinition = workflowDefinition;

            var getProcessInstanceSqlResult = JsonSerializer.Deserialize<GetProcessInstanceSqlResult>(_getProcessInstanceSqlResultJson);

            if (getProcessInstanceSqlResult != null)
                _getProcessInstanceSqlResult = getProcessInstanceSqlResult;

            var getProcessDefinitionSqlResult = JsonSerializer.Deserialize<GetProcessDefinitionSqlResult>(_getProcessDefinitionSqlResultJson);

            if (getProcessDefinitionSqlResult != null)
                _getProcessDefinitionSqlResult = getProcessDefinitionSqlResult;

            var getVariableSqlResult = JsonSerializer.Deserialize<GetVariableSqlResult>(_getVariableSqlResultJson);

            if (getVariableSqlResult != null)
                _getVariableSqlResult = getVariableSqlResult;

            var workflowDefinitionInfos = JsonSerializer.Deserialize<List<WorkflowDefinitionInfo>>(_workflowDefinitionInfosJson);

            if (workflowDefinitionInfos != null)
                _workflowDefinitionInfos = workflowDefinitionInfos;

            var fields = JsonSerializer.Deserialize<List<JsonObject>>(fieldsJson);

            if (fields != null)
                _fields = fields;
            #endregion
        }
    }
}