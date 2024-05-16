using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using MySqlConnector;

namespace LfrlAnvil.MySql;

/// <summary>
/// Represents a factory of delegates used by <see cref="SqlParameterBinderExpression{TSource}"/> instances.
/// </summary>
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlParameterBinderFactory : SqlParameterBinderFactory<MySqlCommand>
{
    internal MySqlParameterBinderFactory(MySqlColumnTypeDefinitionProvider columnTypeDefinitions)
        : base( MySqlDialect.Instance, columnTypeDefinitions, supportsPositionalParameters: false ) { }
}
