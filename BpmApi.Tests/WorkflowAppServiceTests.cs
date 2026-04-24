using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Results;
using BpmApplication.Services;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository.Interfaces;
using Moq;
using System.Text.Json;

namespace BpmApi.Tests
{
    [TestFixture]
    public class WorkflowAppServiceTests
    {
        private WorkflowAppService? _workflowAppService;
        private Dictionary<string, object?> _variables;
        private string? _processType;
        private long _processInstanceId  = 0;
        private GetContextResult? _contextResult;
        private string _category;
        private List<WorkflowDefinitionDto>? _listDefinitionsResult;
        private StartProcessResult? _startProcessResult;
        private SearchProcessDto? _searchProcessDto;
        private List<WorkflowDefinitionInfo> _workflowDefinitionInfos = [];
        private readonly Mock<ICommandHandler<StartProcessCommand, Result<StartProcessResult>>> _startProcessHandler = new();
        private readonly Mock<ICommandHandler<GetContextCommand, Result<GetContextResult>>> _getContextHandler = new();
        private readonly Mock<ICommandHandler<GetDefinitionsCommand, Result<IEnumerable<WorkflowDefinitionDto>>>> _getDefinitionsHandler = new();
        private readonly Mock<ICommandHandler<SearchProcessCommand, Result<SearchProcessDto>>> _searchProcessHandler = new();
        private readonly Mock<IProcessDefinitionRepository> _processDefinitionRepo = new();
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
            _startProcessHandler.Setup(_ => _.HandleAsync(It.IsAny<StartProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<StartProcessResult>() { Success = true, Message = null, Value = _startProcessResult })
            .Verifiable();

            _getContextHandler.Setup(_ => _.HandleAsync(It.IsAny<GetContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<GetContextResult>() { Success = true, Message = null, Value = _contextResult })
            .Verifiable();

            _getDefinitionsHandler.Setup(_ => _.HandleAsync(It.IsAny<GetDefinitionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<IEnumerable<WorkflowDefinitionDto>>() { Success = true, Message = null, Value = _listDefinitionsResult })
            .Verifiable();

            _processDefinitionRepo.Setup(_ => _.GetProcessDefinitionsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_workflowDefinitionInfos)
            .Verifiable();

            _workflowAppService = new WorkflowAppService(_startProcessHandler.Object, _getContextHandler.Object, _getDefinitionsHandler.Object, _searchProcessHandler.Object);

            //execute test
            var result = await _workflowAppService.StartProcessAsync( new StartProcessCommand() { ProcessType = _processType, Variables = _variables, User = _user, Company = _company }, default);

            var response = result ?? new Result<StartProcessResult>() { Value = new StartProcessResult() { ProcessId = 0 } };

            //Assert
            Assert.That(_processInstanceId, Is.LessThan(response?.Value?.ProcessId));
        }

        [Test]
        public async Task GetContext_InputIsValid_ReturnTrue()
        {
            //arrange
            _startProcessHandler.Setup(_ => _.HandleAsync(It.IsAny<StartProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<StartProcessResult>() { Success = true, Message = null, Value = _startProcessResult })
            .Verifiable();

            _getContextHandler.Setup(_ => _.HandleAsync(It.IsAny<GetContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<GetContextResult>() { Success = true, Message = null, Value = _contextResult })
            .Verifiable();

            _getDefinitionsHandler.Setup(_ => _.HandleAsync(It.IsAny<GetDefinitionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<IEnumerable<WorkflowDefinitionDto>>() { Success = true, Message = null, Value = _listDefinitionsResult })
            .Verifiable();

            _workflowAppService = new WorkflowAppService(_startProcessHandler.Object, _getContextHandler.Object, _getDefinitionsHandler.Object, _searchProcessHandler.Object);

            //execute test
            var result = await _workflowAppService.GetContextAsync(new GetContextCommand() { ProcessInstanceId = _processInstanceId, User = _user, Company = _company }, default);

            var response = result ?? new Result<GetContextResult>() { Value = new GetContextResult() { ProcessId = 0 } };

            //Assert
            Assert.That(_processInstanceId, Is.EqualTo(response?.Value?.ProcessId));
        }

        [Test]
        public async Task ListDefinitions_InputIsValid_ReturnTrue()
        {
            //arrange
            _startProcessHandler.Setup(_ => _.HandleAsync(It.IsAny<StartProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<StartProcessResult>() { Success = true, Message = null, Value = _startProcessResult })
            .Verifiable();

            _getContextHandler.Setup(_ => _.HandleAsync(It.IsAny<GetContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<GetContextResult>() { Success = true, Message = null, Value = _contextResult })
            .Verifiable();

            _getDefinitionsHandler.Setup(_ => _.HandleAsync(It.IsAny<GetDefinitionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<IEnumerable<WorkflowDefinitionDto>>() { Success = true, Message = null, Value = _listDefinitionsResult })
            .Verifiable();

            _processDefinitionRepo.Setup(_ => _.GetProcessDefinitionsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_workflowDefinitionInfos)
            .Verifiable();

            _workflowAppService = new WorkflowAppService(_startProcessHandler.Object, _getContextHandler.Object, _getDefinitionsHandler.Object, _searchProcessHandler.Object);

            //execute test
            var result = await _workflowAppService.GetDefinitionsAsync( new GetDefinitionsCommand() { Category = _category, User = _user, Company = _company }, default);

            var response = result ?? new Result<IEnumerable<WorkflowDefinitionDto>>() { Value = null };

            //Assert
            Is.GreaterThan(response?.Value?.ToList().Count > 0);
            Assert.That(_category, Is.EqualTo(response?.Value?.FirstOrDefault()?.Category));
        }

        [Test]
        public async Task SearchProcessAsync_InputIsValid_ReturnTrue()
        {
            //arrange
            _startProcessHandler.Setup(_ => _.HandleAsync(It.IsAny<StartProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<StartProcessResult>() { Success = true, Message = null, Value = _startProcessResult })
            .Verifiable();

            _getContextHandler.Setup(_ => _.HandleAsync(It.IsAny<GetContextCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<GetContextResult>() { Success = true, Message = null, Value = _contextResult })
            .Verifiable();

            _getDefinitionsHandler.Setup(_ => _.HandleAsync(It.IsAny<GetDefinitionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<IEnumerable<WorkflowDefinitionDto>>() { Success = true, Message = null, Value = _listDefinitionsResult })
            .Verifiable();

            _processDefinitionRepo.Setup(_ => _.GetProcessDefinitionsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_workflowDefinitionInfos)
            .Verifiable();

            _searchProcessHandler.Setup(_ => _.HandleAsync(It.IsAny<SearchProcessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Result<SearchProcessDto>() { Success = true, Message = null, Value = _searchProcessDto })
            .Verifiable();


            _workflowAppService = new WorkflowAppService(_startProcessHandler.Object, _getContextHandler.Object, _getDefinitionsHandler.Object, _searchProcessHandler.Object);

            //execute test
            var result = await _workflowAppService.SearchProcessAsync(new SearchProcessCommand() { Category = [_category], Columns = ["column 1"] }, default);

            var response = result ?? new Result<SearchProcessDto>() { Value = null };

            //Assert
            Assert.That(_totalPages, Is.EqualTo(response?.Value?.TotalPages));
        }

        private void PopulateData()
        {
            #region populate variables
            _user = "user1";
            _company = "99900";
            _processType = "Inserimento Anagrafica 3";
            _processInstanceId = 4;
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
                    ""processId"": 7
                }
            ";

            var _workflowDefinitionInfosJson = @"[{""ProcessType"":""Inserimento Anagrafica"",""Name"":""Inserimento Anagrafica"",""Category"":""POC""},{""ProcessType"":""Inserimento Anagrafica 2"",""Name"":""Inserimento Anagrafica 2"",""Category"":""POC""},{""ProcessType"":""Inserimento Anagrafica 3"",""Name"":""Inserimento Anagrafica 3"",""Category"":""POC""}]";

            var _searchProcessDtoJson = @"{""Page"":1,""Size"":10,""TotalElements"":1,""TotalPages"":1}";
            #endregion

            #region populate Deserialize
            var contextResult = JsonSerializer.Deserialize<GetContextResult>(_contextResultJson);

            if (contextResult != null)
                _contextResult = contextResult;

            var listDefinitionsResult = JsonSerializer.Deserialize<List<WorkflowDefinitionDto>>(_listDefinitionsResultJson);

            if (listDefinitionsResult != null)
                _listDefinitionsResult = listDefinitionsResult;

            var startProcessResult = JsonSerializer.Deserialize<StartProcessResult>(_startProcessResultJson);

            if (startProcessResult != null)
                _startProcessResult = startProcessResult;

            var workflowDefinitionInfos = JsonSerializer.Deserialize<List<WorkflowDefinitionInfo>>(_workflowDefinitionInfosJson);

            if (workflowDefinitionInfos != null)
                _workflowDefinitionInfos = workflowDefinitionInfos;

            var searchProcessDto = JsonSerializer.Deserialize<SearchProcessDto>(_searchProcessDtoJson);

            if (searchProcessDto != null)
               _searchProcessDto = searchProcessDto;
            #endregion
        }
    }
}