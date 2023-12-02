using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers;

public static class DataTypeMock
{
    [Pure]
    public static ISqlDataType Create(SqlDialect dialect, string name, DbType dbType = DbType.Object)
    {
        var result = Substitute.For<ISqlDataType>();
        result.Dialect.Returns( dialect );
        result.Name.Returns( name );
        result.DbType.Returns( dbType );
        return result;
    }
}
