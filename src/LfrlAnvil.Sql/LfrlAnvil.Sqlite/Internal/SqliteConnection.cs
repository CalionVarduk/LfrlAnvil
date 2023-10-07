using System;
using System.Data;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal class SqliteConnection : Microsoft.Data.Sqlite.SqliteConnection
{
    internal SqliteConnection(string connectionString)
        : base( connectionString )
    {
        ChangeCallbacks = Array.Empty<Action<SqlDatabaseConnectionChangeEvent>>();
    }

    protected internal Action<SqlDatabaseConnectionChangeEvent>[] ChangeCallbacks { get; set; }

    protected override void OnStateChange(StateChangeEventArgs stateChange)
    {
        base.OnStateChange( stateChange );

        var @event = new SqlDatabaseConnectionChangeEvent( this, stateChange );
        foreach ( var callback in ChangeCallbacks )
            callback( @event );
    }
}
