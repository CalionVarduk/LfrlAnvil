using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlAsyncScalarQueryLambdaExpression
{
    [Pure]
    Delegate Compile();
}

public interface ISqlAsyncScalarQueryLambdaExpression<T> : ISqlAsyncScalarQueryLambdaExpression
{
    [Pure]
    new Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<T>>> Compile();
}
