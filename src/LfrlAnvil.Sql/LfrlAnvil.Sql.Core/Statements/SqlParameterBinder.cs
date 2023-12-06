using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlParameterBinder(
    SqlDialect Dialect,
    Action<IDbCommand, IEnumerable<KeyValuePair<string, object?>>> Delegate)
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Bind(IDbCommand command, IEnumerable<KeyValuePair<string, object?>>? source = null)
    {
        if ( source is null )
            command.Parameters.Clear();
        else
            Delegate( command, source );
    }
}

public readonly record struct SqlParameterBinder<TSource>(SqlDialect Dialect, Action<IDbCommand, TSource> Delegate)
    where TSource : notnull
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Bind(IDbCommand command, TSource? source = default)
    {
        if ( Generic<TSource>.IsNull( source ) )
            command.Parameters.Clear();
        else
            Delegate( command, source );
    }
}
