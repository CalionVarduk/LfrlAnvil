using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using LfrlAnvil.Sqlite.Exceptions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePermanentConnection : SqliteConnection
{
    private const int ActiveState = 0;
    private const int DisposedState = 1;
    private volatile int _state;

    internal SqlitePermanentConnection(string connectionString)
        : base( connectionString )
    {
        _state = ActiveState;
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
        switch ( _state )
        {
            case ActiveState:
                base.Open();
                break;
            case DisposedState:
                throw new InvalidOperationException( Resources.ConnectionForClosedPermanentDatabaseCannotBeReopened );
        }
    }

    public override void Close()
    {
        if ( Interlocked.Exchange( ref _state, DisposedState ) == DisposedState )
        {
            base.Close();
            return;
        }

        base.Dispose( true );
    }

    [SuppressMessage( "Usage", "CA2215" )]
    protected override void Dispose(bool disposing) { }
}
