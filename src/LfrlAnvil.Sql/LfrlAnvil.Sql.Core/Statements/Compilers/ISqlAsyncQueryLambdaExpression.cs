using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlAsyncQueryLambdaExpression
{
    [Pure]
    Delegate Compile();
}

public interface ISqlAsyncQueryLambdaExpression<TRow> : ISqlAsyncQueryLambdaExpression
    where TRow : notnull
{
    [Pure]
    new Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<TRow>>> Compile();
}
