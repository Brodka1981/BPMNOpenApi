using BpmApi.Controllers;
using BpmApplication.Commands;
using BpmApplication.DTO;
using BpmApplication.Results;
using BpmWebApi.Contracts;
using BpmApplication.Services.Interfaces;
using BpmInfrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;

namespace BpmApi.Tests
{
    [TestFixture]
    public class WorkflowControllerTests
    {
        private WorkflowController? _workflowController;
        private readonly Mock<IWorkflowAppService> _workflowAppService = new();
        private UserContext _userContext;
        private Dictionary<string, object?> _variables;
        private string? _processType;
        private long _processInstanceId  = 0;
        private GetContextResult? _contextResult;
        private string _category;
        private List<WorkflowDefinitionDto>? _listDefinitionsResult;
        private StartProcessResult? _startProcessResult;
        private int _totalPages = 0;
        private SearchProcessDto? _searchProcessDto;

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
            _workflowAppService.Setup(_ => _.StartProcessAsync(It.IsAny<StartProcessCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<StartProcessResult>() { Success = true, Message = null, Value = _startProcessResult })
                .Verifiable();

            _workflowController = new WorkflowController(_workflowAppService.Object, _userContext);

            //execute test
            var result = await _workflowController.StartProcess( new BpmWebApi.Contracts.StartProcessRequest() { ProcessType = _processType, Variables = _variables });

            var okResult = (OkObjectResult)result;
            var response = (Result<StartProcessResult>?) okResult.Value ?? new Result<StartProcessResult>() { Value = new StartProcessResult() { ProcessId = 0 } };

            //Assert
            Assert.That(_processInstanceId, Is.LessThan(response?.Value?.ProcessId));
        }

        [Test]
        public async Task GetContext_InputIsValid_ReturnTrue()
        {
            //arrange
            _workflowAppService.Setup(_ => _.GetContextAsync(It.IsAny<GetContextCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<GetContextResult>() { Success = true, Message = null, Value = _contextResult })
                .Verifiable();

            _workflowController = new WorkflowController(_workflowAppService.Object, _userContext);

            //execute test
            var result = await _workflowController.GetContext(_processInstanceId);

            var okResult = (OkObjectResult)result;
            var response = (Result<GetContextResult>?)okResult.Value ?? new Result<GetContextResult>() { Value = new GetContextResult() { ProcessId = 0 } };

            //Assert
            Assert.That(_processInstanceId, Is.EqualTo(response?.Value?.ProcessId));
        }

        [Test]
        public async Task ListDefinitions_InputIsValid_ReturnTrue()
        {
            //arrange
            _workflowAppService.Setup(_ => _.GetDefinitionsAsync(It.IsAny<BpmApplication.Commands.GetDefinitionsCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<IEnumerable<WorkflowDefinitionDto>>() { Success = true, Message = null, Value = _listDefinitionsResult })
                .Verifiable();

            _workflowController = new WorkflowController(_workflowAppService.Object, _userContext);

            //execute test
            var result = await _workflowController.ListDefinitions(new ListDefinitionsRequest
            {
                Filters =
                [
                    new Dictionary<string, string>
                    {
                        ["CATEGORY"] = _category
                    }
                ]
            });


            var okResult = (OkObjectResult)result;
            var response = (Result<ListDefinitionsValueResponse>?)okResult.Value ?? new Result<ListDefinitionsValueResponse>() { Value = new ListDefinitionsValueResponse() };

            //Assert
            Assert.That(response?.Value?.Items.Count > 0, Is.True);
            Assert.That(_category, Is.EqualTo(response?.Value?.Items.FirstOrDefault()?.Category));
        }

        [Test]
        public async Task SearchProcess_InputIsValid_ReturnTrue()
        {
            //arrange
            _workflowAppService.Setup(_ => _.SearchProcessAsync(It.IsAny<BpmApplication.Commands.SearchProcessCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result<SearchProcessDto>() { Success = true, Message = null, Value = _searchProcessDto })
                .Verifiable();

            _workflowController = new WorkflowController(_workflowAppService.Object, _userContext);

            //execute test
            var result = await _workflowController.SearchProcess(new BpmWebApi.Contracts.SearchProcessRequest() { Category = [_category], Columns = ["column 1"] });

            var okResult = (OkObjectResult)result;
            var response = (Result<SearchProcessDto>?)okResult.Value ?? new Result<SearchProcessDto>() { Value = null };

            //Assert
            Assert.That(response?.Value, Is.Not.Null);
            Assert.That(_totalPages, Is.EqualTo(response?.Value?.TotalPages));

        }

        private void PopulateData()
        {
            #region populate variables
            _processType = "Inserimento Anagrafica 3";
            _processInstanceId = 4;
            _variables = new()
            {
                { "variableName1", "VariableValue1" },
                { "variableName2", "VariableValue2" }
            };
            _userContext = new UserContext() { User = "userTest", Company = "companyTest" };
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

            var searchProcessDto = JsonSerializer.Deserialize<SearchProcessDto>(_searchProcessDtoJson);

            if (searchProcessDto != null)
                _searchProcessDto = searchProcessDto;
            #endregion
        }
    }
}