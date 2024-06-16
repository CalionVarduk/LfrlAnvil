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
using System.Diagnostics.CodeAnalysis;
using LfrlAnvil.Async;
using LfrlAnvil.Sqlite.Exceptions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePermanentConnection : SqliteConnection
{
    private InterlockedBoolean _isDisposed;

    internal SqlitePermanentConnection(string connectionString)
        : base( connectionString )
    {
        _isDisposed = new InterlockedBoolean( false );
    }

    [AllowNull]
    public override string ConnectionString
    {
        get => base.ConnectionString;
        set
        {
            if ( base.ConnectionString is not null )
                throw new InvalidOperationException( Resources.ConnectionStringForPermanentDatabaseIsImmutable );

            base.ConnectionString = value;
        }
    }

    public override void Open()
    {
        if ( _isDisposed.Value )
            throw new InvalidOperationException( Resources.ConnectionForClosedPermanentDatabaseCannotBeReopened );

        base.Open();
    }

    public override void Close()
    {
        if ( ! _isDisposed.WriteTrue() )
        {
            base.Close();
            return;
        }

        base.Dispose( true );
    }

    [SuppressMessage( "Usage", "CA2215" )]
    protected override void Dispose(bool disposing) { }
}
