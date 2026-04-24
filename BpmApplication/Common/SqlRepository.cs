using BpmApplication.Common.Interfaces;
using System.Data.Common;

namespace BpmApplication.Common;

public class SqlRepository(IDbConnectionFactory factory) : IRepository
{
    private readonly IDbConnectionFactory _factory = factory;

    public async Task<int> ExecuteAsync(string sql, object? parameters = null)
    {
        using DbConnection conn = _factory.Create();
        using DbCommand cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        AddParameters(cmd, parameters);

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<T>> QueryAsync<T>(string sql, Func<DbDataReader, T> map, object? parameters = null)
    {
        using DbConnection conn = _factory.Create();
        using DbCommand cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        AddParameters(cmd, parameters);

        await conn.OpenAsync();

        using DbDataReader reader = await cmd.ExecuteReaderAsync();
        var list = new List<T>();

        while (await reader.ReadAsync())
            list.Add(map(reader));

        return list;
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, Func<DbDataReader, T> map, object? parameters = null)
    {
        using DbConnection conn = _factory.Create();
        using DbCommand cmd = conn.CreateCommand();

        cmd.CommandText = sql;
        AddParameters(cmd, parameters);

        await conn.OpenAsync();

        using DbDataReader reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return map(reader);

        return default;
    }

    private static void AddParameters(DbCommand cmd, object? parameters)
    {
        if (parameters == null)
            return;

        foreach (var prop in parameters.GetType().GetProperties())
        {
            var p = cmd.CreateParameter();
            p.ParameterName = prop.Name;
            p.Value = prop.GetValue(parameters) ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }
    }
}