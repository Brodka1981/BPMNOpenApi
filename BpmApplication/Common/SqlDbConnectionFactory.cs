using BpmApplication.Common.Interfaces;
using System.Data;
using System.Data.Common;

namespace BpmApplication.Common;

public class SqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly DbConfig _config;
    private readonly DbProviderFactory _factory;

    public SqlDbConnectionFactory(DbConfig config)
    {
        _config = config;
        _factory = DbProviderFactories.GetFactory(config.ProviderName);
    }

    public DbConnection Create()
    {
        var conn = _factory.CreateConnection();
        if (conn == null)
            throw new InvalidOperationException($"Cannot create connection for provider '{_config.ProviderName}'.");

        conn.ConnectionString = _config.ConnectionString;
        return conn;
    }
}
