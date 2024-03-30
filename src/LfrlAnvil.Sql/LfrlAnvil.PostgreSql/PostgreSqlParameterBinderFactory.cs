using LfrlAnvil.Sql.Statements.Compilers;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

public sealed class PostgreSqlParameterBinderFactory : SqlParameterBinderFactory<NpgsqlCommand>
{
    internal PostgreSqlParameterBinderFactory(PostgreSqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( PostgreSqlDialect.Instance, columnTypeDefinitions ) { }
}
