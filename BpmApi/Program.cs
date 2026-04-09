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
using BpmDomain.Registries;
using BpmDomain.Registries.Interfaces;
using BpmDomain.Services;
using BpmInfrastructure.Context;
using BpmInfrastructure.Models;
using BpmInfrastructure.Repository;
using BpmInfrastructure.Repository.Interfaces;
using BpmInfrastructure.Services;
using BpmInfrastructure.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.OpenApi;
using System.Data;

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

// User context
builder.Services.AddScoped<UserContext>();

//scrutor per attivazione dinamica di un sistema a plug in
// per attivazione degli user task
// dll con classi che implementano l'interfaccia IServiceUserTaskHandler nella cartella bin
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IServiceUserTaskHandler>()   // assembly dove stanno gli handler
    .AddClasses(classes => classes.AssignableTo<IServiceUserTaskHandler>())
    .AsImplementedInterfaces()
    .WithTransientLifetime()
);
// dll con classi che implementano l'interfaccia IServiceTaskHandler nella cartella bin
builder.Services.Scan(scan => scan
        .FromAssemblyOf<IServiceTaskHandler>()   // assembly dove stanno gli handler
        .AddClasses(classes => classes.AssignableTo<IServiceTaskHandler>())
        .AsImplementedInterfaces()
        .WithTransientLifetime()
    );

//read the AppSettings and make it available to BpmInfrastructure
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

//Domain
builder.Services.AddSingleton<IServiceTaskRegistry, ServiceTaskRegistry>();
builder.Services.AddScoped<IBpmnParserService, BpmnParserService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();  // competenze 
builder.Services.AddScoped<IBpmEngine, BpmEngine>();

//Infrastructure
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
builder.Services.AddScoped<ICommandHandler<GetDefinitionsCommand, Result<IEnumerable<BpmApplication.DTO.WorkflowDefinitionDto>>>, GetDefinitionsHandler>();
builder.Services.AddScoped<StartProcessHandler>();
builder.Services.AddScoped<GetDefinitionsHandler>();
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