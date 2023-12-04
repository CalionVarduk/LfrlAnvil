using LfrlAnvil.Sql.Statements.Compilers;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteParameterBinderFactory : SqlParameterBinderFactory<SqliteCommand>
{
    internal SqliteParameterBinderFactory(SqliteColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( SqliteDialect.Instance, columnTypeDefinitions ) { }
}
