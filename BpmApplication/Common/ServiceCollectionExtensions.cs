using BpmApplication.Common.Interfaces;
using BpmApplication.Handlers.Interfaces;
using BpmApplication.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BpmApplication.Common;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowCqrs(
        this IServiceCollection services,
        DbConfig config)
    {
        services.AddSingleton(config);
        services.AddSingleton<IDbConnectionFactory, SqlDbConnectionFactory>();
        services.AddScoped<IRepository, SqlRepository>();

        RegisterHandlers(services, Assembly.GetExecutingAssembly());

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var types = assembly.GetTypes();

        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(BpmApplication.Handlers.Interfaces.ICommandHandler<,>))
                {
                    services.AddScoped(iface, type);
                }

                if (iface.IsGenericType &&
                    iface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                {
                    services.AddScoped(iface, type);
                }
            }
        }
    }
}
