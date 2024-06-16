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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
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

    [Pure]
    internal DbConnectionEventHandler CreateEventHandler()
    {
        var callbacks = _builder?.ConnectionChangeCallbacks.ToArray() ?? Array.Empty<Action<SqlDatabaseConnectionChangeEvent>>();
        return new DbConnectionEventHandler( callbacks );
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
