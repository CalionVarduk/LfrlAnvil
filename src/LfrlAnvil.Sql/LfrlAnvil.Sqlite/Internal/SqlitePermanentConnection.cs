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
