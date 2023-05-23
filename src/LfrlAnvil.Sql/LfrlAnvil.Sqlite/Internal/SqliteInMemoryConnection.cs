using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using LfrlAnvil.Sqlite.Exceptions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteInMemoryConnection : SqliteConnection
{
    private volatile int _closed;

    internal SqliteInMemoryConnection(string connectionString)
        : base( connectionString )
    {
        _closed = 0;
        Assume.Equals( DataSource, ":memory:", nameof( DataSource ) );
        base.Open();
    }

    [AllowNull]
    public override string ConnectionString
    {
        get => base.ConnectionString;
        set
        {
            if ( base.ConnectionString is not null )
                throw new InvalidOperationException( Resources.ConnectionStringForInMemoryDatabaseIsImmutable );

            base.ConnectionString = value;
        }
    }

    public override void Open()
    {
        if ( State != ConnectionState.Open )
            throw new InvalidOperationException( Resources.ConnectionForClosedInMemoryDatabaseCannotBeReopened );
    }

    public override void Close()
    {
        if ( Interlocked.Exchange( ref _closed, 1 ) == 1 )
        {
            base.Close();
            return;
        }

        base.Dispose( true );
    }

    [SuppressMessage( "Usage", "CA2215" )]
    protected override void Dispose(bool disposing) { }
}
