using LfrlAnvil.Sql.Statements.Compilers;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

public sealed class PostgreSqlQueryReaderFactory : SqlQueryReaderFactory<NpgsqlDataReader>
{
    internal PostgreSqlQueryReaderFactory(PostgreSqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( PostgreSqlDialect.Instance, columnTypeDefinitions ) { }
}
