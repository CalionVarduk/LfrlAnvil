using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlQueryReaderFactory
{
    bool SupportsAsync { get; }
    SqlDialect Dialect { get; }

    [Pure]
    SqlQueryReader Create(SqlQueryReaderCreationOptions? options = null);

    [Pure]
    SqlAsyncQueryReader CreateAsync(SqlQueryReaderCreationOptions? options = null);

    [Pure]
    SqlQueryReaderExpression CreateExpression(Type rowType, SqlQueryReaderCreationOptions? options = null);

    [Pure]
    SqlAsyncQueryReaderExpression CreateAsyncExpression(Type rowType, SqlQueryReaderCreationOptions? options = null);
}
