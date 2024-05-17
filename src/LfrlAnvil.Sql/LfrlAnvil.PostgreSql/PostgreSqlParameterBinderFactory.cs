using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

/// <summary>
/// Represents a factory of delegates used by <see cref="SqlParameterBinderExpression{TSource}"/> instances.
/// </summary>
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlParameterBinderFactory : SqlParameterBinderFactory<NpgsqlCommand>
{
    internal PostgreSqlParameterBinderFactory(PostgreSqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( PostgreSqlDialect.Instance, columnTypeDefinitions, supportsPositionalParameters: true ) { }
}
