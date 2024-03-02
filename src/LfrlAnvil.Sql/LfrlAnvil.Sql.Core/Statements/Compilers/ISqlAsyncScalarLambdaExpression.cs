using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements.Compilers;

public interface ISqlAsyncScalarLambdaExpression
{
    [Pure]
    Delegate Compile();
}

public interface ISqlAsyncScalarLambdaExpression<T> : ISqlAsyncScalarLambdaExpression
{
    [Pure]
    new Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult<T>>> Compile();
}
