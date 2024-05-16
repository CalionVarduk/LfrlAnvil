using LfrlAnvil.Sql.Statements.Compilers;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Represents a factory of delegates used by query reader expression instances.
/// </summary>
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteQueryReaderFactory : SqlQueryReaderFactory<SqliteDataReader>
{
    internal SqliteQueryReaderFactory(SqliteColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( SqliteDialect.Instance, columnTypeDefinitions ) { }
}
