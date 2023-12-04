using LfrlAnvil.Sql.Statements.Compilers;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteQueryReaderFactory : SqlQueryReaderFactory<SqliteDataReader>
{
    internal SqliteQueryReaderFactory(SqliteColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( SqliteDialect.Instance, columnTypeDefinitions ) { }
}
