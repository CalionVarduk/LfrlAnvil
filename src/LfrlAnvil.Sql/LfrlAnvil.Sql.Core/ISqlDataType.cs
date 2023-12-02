using System.Data;

namespace LfrlAnvil.Sql;

public interface ISqlDataType
{
    SqlDialect Dialect { get; }
    string Name { get; }
    DbType DbType { get; }
}
