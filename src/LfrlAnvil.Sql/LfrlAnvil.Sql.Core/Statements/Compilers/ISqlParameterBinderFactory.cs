using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlParameterBinderFactory
{
    SqlDialect Dialect { get; }
    bool SupportsPositionalParameters { get; }

    [Pure]
    SqlParameterBinder Create(SqlParameterBinderCreationOptions? options = null);

    [Pure]
    SqlParameterBinderExpression CreateExpression(Type sourceType, SqlParameterBinderCreationOptions? options = null);
}
