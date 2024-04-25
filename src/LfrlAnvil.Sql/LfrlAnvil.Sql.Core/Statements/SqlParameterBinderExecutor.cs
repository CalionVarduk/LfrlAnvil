using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlParameterBinderExecutor(SqlParameterBinder Binder, IEnumerable<SqlParameter>? Source)
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Execute(IDbCommand command)
    {
        Binder.Bind( command, Source );
    }
}

public readonly record struct SqlParameterBinderExecutor<TSource>(SqlParameterBinder<TSource> Binder, TSource? Source)
    where TSource : notnull
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Execute(IDbCommand command)
    {
        Binder.Bind( command, Source );
    }
}
