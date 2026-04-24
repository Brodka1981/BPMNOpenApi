using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Handlers;
using BpmApplication.Results;

using Moq;
using System.Text.Json;

namespace BpmApi.Tests
{
    [TestFixture]
    public class CommandHandlerTests
    {
        private Dictionary<string, object?> _variables;
        private string? _processType;
        private long _processInstanceId  = 0;
        private long _processId = 0;
        private BpmDomain.Results.GetContextResult _contextResult = new();
        private string _category;
        private IEnumerable<BpmDomain.Results.GetDefinitionsResult> _listDefinitionsResult = [];
        private BpmDomain.Results.StartProcessResult _startProcessResult = new();
        private StartProcessHandler? _startProcessHandler;
        private GetContextHandler? _getContextHandler;
        private GetDefinitionsHandler? _getDefinitionsHandler;
        private SearchProcessHandler? _searchProcessHandler;
        private readonly Mock<BpmDomain.Engine.Interfaces.IBpmEngine> _engine = new();
        private string _user;
        private string _company;
        private BpmDomain.Results.SearchProcessResult? _searchProcessDto;
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
            _engine.Setup(_ => _.StartProcessAsync(It.IsAny<BpmDomain.Commands.StartProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_startProcessResult)
            .Verifiable();

            _startProcessHandler = new StartProcessHandler(_engine.Object);

            //execute test
            var result = await _startProcessHandler.HandleAsync( new StartProcessCommand() { ProcessType = _processType, Variables = _variables, User = _user, Company = _company }, default);

            var response = result ?? new Result<StartProcessResult>() { Value = new StartProcessResult() { ProcessId = 0 } };

            //Assert
            Assert.That(_processInstanceId < response?.Value?.ProcessId);
        }

        [Test]
        public async Task GetContext_InputIsValid_ReturnTrue()
        {
            //arrange
            _engine.Setup(_ => _.GetContextAsync(It.IsAny<BpmDomain.Commands.GetContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_contextResult)
            .Verifiable();

            _getContextHandler = new GetContextHandler(_engine.Object);

            //execute test
            var result = await _getContextHandler.HandleAsync(new GetContextCommand() { ProcessInstanceId = _processInstanceId, User = _user, Company = _company }, default);

            var response = result ?? new Result<GetContextResult>() { Value = new GetContextResult() { ProcessId = 0 } };

            //Assert
            Assert.That(_processInstanceId, Is.EqualTo(response?.Value?.ProcessId));
        }

        [Test]
        public async Task ListDefinitions_InputIsValid_ReturnTrue()
        {
            //arrange
            _engine.Setup(_ => _.GetDefinitionsAsync(It.IsAny<BpmDomain.Commands.GetDefinitionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_listDefinitionsResult)
            .Verifiable();

            _getDefinitionsHandler = new GetDefinitionsHandler(_engine.Object);

            //execute test
            var result = await _getDefinitionsHandler.HandleAsync( new GetDefinitionsCommand() { Category = _category, User = _user, Company = _company }, default);

            var response = result ?? new Result<IEnumerable<WorkflowDefinitionDto>>() { Value = null };

            //Assert
            Is.GreaterThan(response?.Value?.ToList().Count > 0);
            Assert.That(_category, Is.EqualTo(response?.Value?.FirstOrDefault()?.Category));
        }

        [Test]
        public async Task SearchProcess_InputIsValid_ReturnTrue()
        {
            //arrange
            _engine.Setup(_ => _.SearchProcessAsync(It.IsAny<BpmDomain.Commands.SearchProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_searchProcessDto)
            .Verifiable();

            _searchProcessHandler = new SearchProcessHandler(_engine.Object);

            //execute test
            var result = await _searchProcessHandler.HandleAsync(new SearchProcessCommand() {  DefinitionType = [_category], Columns = ["col1"] }, default);

            var response = result ?? new Result<SearchProcessDto>() { Value = null };

            //Assert
            Assert.That(response?.Value, Is.Not.Null);
            Assert.That(_totalPages, Is.EqualTo(response?.Value?.TotalPages));

        }

        private void PopulateData()
        {
            #region populate variables
            _user = "user1";
            _company = "99900";
            _processType = "Inserimento Anagrafica 3";
            _processInstanceId = 4;
            _processId = 7;
            _variables = new()
            {
                { "variableName1", "VariableValue1" },
                { "variableName2", "VariableValue2" }
            };

            _category = "POC";
            _totalPages = 1;
            #endregion

            #region populate Json
            var _contextResultJson = @"
                {
                    ""processId"": " + _processInstanceId.ToString() + @",
                    ""processType"": ""Inserimento Anagrafica"",
                    ""name"": ""Inserimento Anagrafica"",
                    ""contextMode"": ""NORMAL"",
                    ""state"": {
                        ""idState"": ""stato_bozza"",
                        ""description"": ""Bozza""
                    },
                    ""actions"": [
                        {
                        ""idAction"": ""dd16a4e5-2808-4501-a88a-d81aa3599ca3"",
                        ""description"": """"
                        }
                    ],
                    ""variables"": [
                        {
                        ""variable1"": 1
                        }
                    ],
                    ""form"": {
                        ""sections"": [
                        {
                            ""type"": ""Fields"",
                            ""title"": ""Fields"",
                            ""fields"": [
                            {
                                ""type"": ""text"",
                                ""name"": ""field1"",
                                ""label"": ""label field1"",
                                ""value"": ""valorefield1""
                            }
                            ]
                        },
                        {
                            ""type"": ""Warnings"",
                            ""title"": ""Warnings"",
                            ""fields"": [
                            {
                                ""warning1"": ""valorewarning1""
                            }
                            ]
                        }
                        ]
                    }
                }            
            ";

            var _listDefinitionsResultJson = @"
                  [
                    {
                      ""processType"": ""Inserimento Anagrafica"",
                      ""name"": ""Inserimento Anagrafica"",
                      ""category"": ""POC""
                    },
                    {
                      ""processType"": ""Inserimento Anagrafica 2"",
                      ""name"": ""Inserimento Anagrafica 2"",
                      ""category"": ""POC""
                    }
                  ]
            ";

            var _startProcessResultJson = @"
                {
                    ""processId"": " + _processId.ToString() + @"
                }
            ";

            var _searchProcessDtoJson = @"{""page"":1,""size"":10,""totalElements"":1,""totalPages"":1}";
            #endregion

            #region populate Deserialize
            var contextResult = JsonSerializer.Deserialize<BpmDomain.Results.GetContextResult>(_contextResultJson);

            if (contextResult != null)
                _contextResult = contextResult;

            var listDefinitionsResult = JsonSerializer.Deserialize<List<BpmDomain.Results.GetDefinitionsResult>>(_listDefinitionsResultJson);

            if (listDefinitionsResult != null)
                _listDefinitionsResult = listDefinitionsResult;

            var startProcessResult = JsonSerializer.Deserialize<BpmDomain.Results.StartProcessResult>(_startProcessResultJson);

            if (startProcessResult != null)
                _startProcessResult = startProcessResult;

            var searchProcessDto = JsonSerializer.Deserialize<BpmDomain.Results.SearchProcessResult>(_searchProcessDtoJson);

            if (searchProcessDto != null)
                _searchProcessDto = searchProcessDto;
            #endregion
        }
    }
}