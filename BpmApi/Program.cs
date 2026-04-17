using BpmApi.Middleware;
using BpmApplication.Commands;
using BpmApplication.Handlers;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Queries;
using BpmApplication.Queries.Interfaces;
using BpmApplication.Results;
using BpmApplication.Services;
using BpmApplication.Services.Interfaces;
using BpmDomain.Engine;
using BpmDomain.Engine.Interfaces;
using BpmDomain.Factories;
using BpmDomain.Factories.Interfaces;
//using BpmDomain.Registries;
//using BpmDomain.Registries.Interfaces;
using BpmDomain.Services;
using BpmInfrastructure.Context;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repositories;
using BpmInfrastructure.Repository;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Services;
using BpmInfrastructure.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi;
using System.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BPM Workflow API",
        Version = "v1"
    });
});

// ---------------------------------------------------------
// 2) SQL CONNECTION 
// ---------------------------------------------------------
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var conn = new SqlConnection(builder.Configuration.GetConnectionString("Default"));
    return conn;
});

//search all BpmService... and add to Assembly list for add into scan of scrutor
foreach (string assemblyPath in Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Add-ins"), "*.dll", SearchOption.AllDirectories))
{
    var assembly = Assembly.LoadFile(assemblyPath);

    //scrutor per attivazione dinamica di un sistema a plug in for a custom namespace
    builder.Services.Scan(scan => scan
       .FromAssemblies(assembly)
       .AddClasses(c => c.InNamespaces(assembly.GetName().Name + ".Handlers")) 
       .AsImplementedInterfaces()
       .WithTransientLifetime());
}

// User context
builder.Services.AddScoped<UserContext>();

//read the AppSettings and make it available to BpmInfrastructure
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

//Factories
builder.Services.AddTransient<IServiceFactory, ServiceFactory>();
builder.Services.AddTransient<ServiceFactory>();

//Domain
builder.Services.AddScoped<IBpmnParserService, BpmnParserService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();  // competenze 
builder.Services.AddScoped<IBpmEngine, BpmEngine>();

//Infrastructure
builder.Services.AddScoped<IDefinitionRepository, SqlDefinitionRepository>();
builder.Services.AddHttpClient<IAuthorizationDataProvider, AuthorizationDataProvider>(client =>
{
    var authorizationApiBaseUrl = builder.Configuration["AuthorizationApi:BaseUrl"]
                                  ?? throw new InvalidOperationException("Missing configuration: AuthorizationApi:BaseUrl");
    client.BaseAddress = new Uri(authorizationApiBaseUrl);
});
builder.Services.AddScoped<IProcessDefinitionRepository, SqlProcessDefinitionRepository>();
builder.Services.AddScoped<ICompetenceDefinitionRepository, SqlCompetenceDefinitionRepository>();
builder.Services.AddScoped<IProcessInstanceRepository, SqlProcessInstanceRepository>();
builder.Services.AddScoped<ISqlCommonRepository, SqlCommonRepository>();

//Application
builder.Services.AddScoped<ICommandHandler<StartProcessCommand, Result<StartProcessResult>>, StartProcessHandler>();
builder.Services.AddScoped<ICommandHandler<GetContextCommand, Result<GetContextResult>>, GetContextHandler>();
builder.Services.AddScoped<ICommandHandler<GetDefinitionsCommand, Result<IEnumerable<BpmApplication.DTO.WorkflowDefinitionDto>>>, GetDefinitionsHandler>();
builder.Services.AddScoped<ICommandHandler<SearchProcessCommand, Result<IEnumerable<BpmApplication.DTO.SearchProcessDto>>>, SearchProcessHandler>();
builder.Services.AddScoped<StartProcessHandler>();
builder.Services.AddScoped<GetDefinitionsHandler>();
builder.Services.AddScoped<SearchProcessHandler>();
builder.Services.AddScoped<IWorkflowAppService, WorkflowAppService>();
builder.Services.AddScoped<GetContextHandler>();

builder.Services.AddControllers();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseMiddleware<UserContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();