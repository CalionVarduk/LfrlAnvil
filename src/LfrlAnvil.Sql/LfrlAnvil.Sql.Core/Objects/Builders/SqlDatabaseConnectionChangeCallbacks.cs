using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlDatabaseConnectionChangeCallbacks
{
    private SqlDatabaseConnectionChangeCallbacks(List<Action<SqlDatabaseConnectionChangeEvent>> callbacks)
    {
        FirstPendingCallbackIndex = callbacks.Count;
        Callbacks = callbacks;
    }

    public int FirstPendingCallbackIndex { get; }
    public List<Action<SqlDatabaseConnectionChangeEvent>> Callbacks { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseConnectionChangeCallbacks Create()
    {
        return new SqlDatabaseConnectionChangeCallbacks( new List<Action<SqlDatabaseConnectionChangeEvent>>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<Action<SqlDatabaseConnectionChangeEvent>> GetPendingCallbacks()
    {
        var span = CollectionsMarshal.AsSpan( Callbacks );
        return span.Slice( FirstPendingCallbackIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlDatabaseConnectionChangeCallbacks UpdateFirstPendingCallbackIndex()
    {
        return new SqlDatabaseConnectionChangeCallbacks( Callbacks );
    }
}
