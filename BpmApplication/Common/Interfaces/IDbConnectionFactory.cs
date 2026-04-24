using System.Data.Common;

namespace BpmApplication.Common.Interfaces;   
public interface IDbConnectionFactory
{
    DbConnection Create();
}
