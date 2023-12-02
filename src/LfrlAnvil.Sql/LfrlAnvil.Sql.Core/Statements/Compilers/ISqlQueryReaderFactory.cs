using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlQueryReaderFactory
{
    SqlDialect Dialect { get; }

    [Pure]
    SqlQueryReader Create(SqlQueryReaderCreationOptions? options = null);

    [Pure]
    SqlQueryReaderExpression CreateExpression(Type rowType, SqlQueryReaderCreationOptions? options = null);
}
