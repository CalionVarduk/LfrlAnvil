using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

internal sealed class SqlDatabaseConnectionStateChanges
{
    private readonly List<SqlDatabaseConnectionChangeEvent> _events;
    private int _invokedCallbackCount;
    private SqlDatabaseBuilder? _builder;

    internal SqlDatabaseConnectionStateChanges()
    {
        _events = new List<SqlDatabaseConnectionChangeEvent>();
        _invokedCallbackCount = 0;
        _builder = null;
    }

    internal void Add(object? sender, StateChangeEventArgs change)
    {
        if ( sender is not DbConnection connection )
            return;

        var @event = new SqlDatabaseConnectionChangeEvent( connection, change );
        if ( _builder is null )
        {
            _events.Add( @event );
            return;
        }

        var callbacks = _builder.ConnectionChangeCallbacks.Slice( 0, _invokedCallbackCount );
        foreach ( var callback in callbacks )
            callback( @event );
    }

    internal void SetBuilder(SqlDatabaseBuilder builder)
    {
        Assume.IsNull( _builder );
        _builder = builder;
        InvokePendingCallbacks();
    }

    internal void InvokePendingCallbacks()
    {
        Assume.IsNotNull( _builder );
        var callbacks = _builder.ConnectionChangeCallbacks.Slice( _invokedCallbackCount );
        if ( callbacks.Length == 0 )
            return;

        _invokedCallbackCount += callbacks.Length;
        foreach ( var @event in _events )
        {
            foreach ( var callback in callbacks )
                callback( @event );
        }
    }
}
