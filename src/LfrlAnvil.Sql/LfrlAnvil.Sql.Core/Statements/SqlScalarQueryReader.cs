using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlScalarQueryReader(SqlDialect Dialect, Func<IDataReader, SqlScalarQueryResult> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult Read(IDataReader reader)
    {
        return Delegate( reader );
    }
}

public readonly record struct SqlScalarQueryReader<T>(SqlDialect Dialect, Func<IDataReader, SqlScalarQueryResult<T>> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarQueryResult<T> Read(IDataReader reader)
    {
        return Delegate( reader );
    }
}
