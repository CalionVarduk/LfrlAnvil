using LfrlAnvil.Sql.Statements.Compilers;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

/// <summary>
/// Represents a factory of delegates used by query reader expression instances.
/// </summary>
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlQueryReaderFactory : SqlQueryReaderFactory<NpgsqlDataReader>
{
    internal PostgreSqlQueryReaderFactory(PostgreSqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( PostgreSqlDialect.Instance, columnTypeDefinitions ) { }
}
