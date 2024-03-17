using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

public sealed class DbConnectionEventHandler
{
    private readonly StateChangeEventHandler? _stateChangeHandler;
    private readonly EventHandler? _disposedHandler;

    public DbConnectionEventHandler(ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> callbacks)
    {
        Callbacks = callbacks;
        if ( Callbacks.Count > 0 )
        {
            _stateChangeHandler = OnStateChange;
            _disposedHandler = OnDisposed;
        }
    }

    public ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> Callbacks { get; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Attach(DbConnection connection)
    {
        if ( Callbacks.Count > 0 )
        {
            connection.StateChange += _stateChangeHandler;
            connection.Disposed += _disposedHandler;
        }
    }

    private void OnStateChange(object? sender, StateChangeEventArgs args)
    {
        if ( sender is DbConnection connection )
        {
            foreach ( var callback in Callbacks )
                callback( new SqlDatabaseConnectionChangeEvent( connection, args ) );
        }
    }

    private void OnDisposed(object? sender, EventArgs args)
    {
        if ( sender is DbConnection connection )
        {
            connection.StateChange -= _stateChangeHandler;
            connection.Disposed -= _disposedHandler;
        }
    }
}
