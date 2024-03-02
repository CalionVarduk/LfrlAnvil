using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlScalarReader(SqlDialect Dialect, Func<IDataReader, SqlScalarResult> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarResult Read(IDataReader reader)
    {
        return Delegate( reader );
    }
}

public readonly record struct SqlScalarReader<T>(SqlDialect Dialect, Func<IDataReader, SqlScalarResult<T>> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlScalarResult<T> Read(IDataReader reader)
    {
        return Delegate( reader );
    }
}
