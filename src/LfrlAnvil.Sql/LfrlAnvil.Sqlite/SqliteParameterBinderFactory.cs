using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Represents a factory of delegates used by <see cref="SqlParameterBinderExpression"/> instances.
/// </summary>
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteParameterBinderFactory : SqlParameterBinderFactory<SqliteCommand>
{
    internal SqliteParameterBinderFactory(SqliteColumnTypeDefinitionProvider columnTypeDefinitions, bool supportsPositionalParameters)
        : base( SqliteDialect.Instance, columnTypeDefinitions, supportsPositionalParameters ) { }
}
