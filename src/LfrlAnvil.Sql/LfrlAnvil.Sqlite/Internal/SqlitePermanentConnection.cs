using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using LfrlAnvil.Sqlite.Exceptions;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePermanentConnection : SqliteConnection
{
    private volatile int _closed;

    internal SqlitePermanentConnection(string connectionString)
        : base( connectionString )
    {
        _closed = 0;
        base.Open();
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
        if ( State != ConnectionState.Open )
            throw new InvalidOperationException( Resources.ConnectionForClosedPermanentDatabaseCannotBeReopened );
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
