using LfrlAnvil.Sql;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public static class SqlDialectMock
{
    public static readonly SqlDialect Instance = new SqlDialect( "SqlMock" );
}
