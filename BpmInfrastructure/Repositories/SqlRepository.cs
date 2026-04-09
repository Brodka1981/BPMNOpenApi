using System.Data;

namespace BpmInfrastructure.Repository
{
    public abstract class SqlRepository
    {
        internal void AddParms(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
