using System.Data.Common;

namespace BpmApplication.Common.Interfaces;

public interface IRepository
{
    Task<int> ExecuteAsync(string sql, object? parameters = null);
    Task<List<T>> QueryAsync<T>(string sql, Func<DbDataReader, T> map, object? parameters = null);
    Task<T?> QuerySingleAsync<T>(string sql, Func<DbDataReader, T> map, object? parameters = null);
}
