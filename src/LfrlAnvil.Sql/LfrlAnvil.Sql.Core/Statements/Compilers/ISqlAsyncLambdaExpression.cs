using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlAsyncLambdaExpression
{
    [Pure]
    Delegate Compile();
}

public interface ISqlAsyncLambdaExpression<TRow> : ISqlAsyncLambdaExpression
    where TRow : notnull
{
    [Pure]
    new Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryReaderResult<TRow>>> Compile();
}
