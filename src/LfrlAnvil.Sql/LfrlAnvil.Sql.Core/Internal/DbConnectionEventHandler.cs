// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object capable of attaching a collection of <see cref="SqlDatabaseConnectionChangeEvent"/> callbacks
/// to <see cref="DbConnection"/> event handlers.
/// </summary>
public sealed class DbConnectionEventHandler
{
    private readonly StateChangeEventHandler? _stateChangeHandler;
    private readonly EventHandler? _disposedHandler;

    /// <summary>
    /// Creates a new <see cref="DbConnectionEventHandler"/> instance.
    /// </summary>
    /// <param name="callbacks">Collection of <see cref="SqlDatabaseConnectionChangeEvent"/> callbacks.</param>
    public DbConnectionEventHandler(ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> callbacks)
    {
        Callbacks = callbacks;
        if ( Callbacks.Count > 0 )
        {
            _stateChangeHandler = OnStateChange;
            _disposedHandler = OnDisposed;
        }
    }

    /// <summary>
    /// Collection of <see cref="SqlDatabaseConnectionChangeEvent"/> callbacks.
    /// </summary>
    public ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> Callbacks { get; }

    /// <summary>
    /// Attaches the <see cref="Callbacks"/> to the provided <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection">DB connection.</param>
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
