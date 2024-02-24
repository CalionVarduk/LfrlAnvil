using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

// TODO: remove when ready
public readonly struct SqlDatabaseConnectionChangeCallbacks
{
    private readonly List<Action<SqlDatabaseConnectionChangeEvent>> _callbacks;

    private SqlDatabaseConnectionChangeCallbacks(List<Action<SqlDatabaseConnectionChangeEvent>> callbacks)
    {
        FirstPendingCallbackIndex = callbacks.Count;
        _callbacks = callbacks;
    }

    public int FirstPendingCallbackIndex { get; }
    public IReadOnlyCollection<Action<SqlDatabaseConnectionChangeEvent>> Callbacks => _callbacks;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseConnectionChangeCallbacks Create()
    {
        return new SqlDatabaseConnectionChangeCallbacks( new List<Action<SqlDatabaseConnectionChangeEvent>>() );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void AddCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        _callbacks.Add( callback );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<Action<SqlDatabaseConnectionChangeEvent>> GetPendingCallbacks()
    {
        var span = CollectionsMarshal.AsSpan( _callbacks );
        return span.Slice( FirstPendingCallbackIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlDatabaseConnectionChangeCallbacks UpdateFirstPendingCallbackIndex()
    {
        return new SqlDatabaseConnectionChangeCallbacks( _callbacks );
    }

    [Pure]
    public Action<SqlDatabaseConnectionChangeEvent>[] GetCallbacksArray()
    {
        return _callbacks.ToArray();
    }
}
