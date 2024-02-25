using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Sql.Mocks.System;

public sealed class DbTransactionMock : DbTransaction
{
    private readonly List<string> _audit = new List<string>();

    internal DbTransactionMock(DbConnectionMock connection, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        Connection = connection;
        IsolationLevel = isolationLevel;
        Id = Connection.CreatedTransactions.Count;
    }

    public int Id { get; }
    public new DbConnectionMock Connection { get; }
    public bool Committed { get; private set; }
    public bool RolledBack { get; private set; }
    public bool ThrowOnCommit { get; init; }
    public override IsolationLevel IsolationLevel { get; }
    public IReadOnlyList<string> Audit => _audit;
    protected override DbConnection DbConnection => Connection;

    [Pure]
    public override string ToString()
    {
        return $"DbTransaction[{Id}]";
    }

    public override void Commit()
    {
        if ( ThrowOnCommit )
            throw new Exception( "db transaction mock error" );

        if ( Committed )
            throw new Exception( "db transaction was already committed" );

        Committed = true;
        AddAudit( nameof( Commit ) );
    }

    public override void Rollback()
    {
        if ( RolledBack )
            throw new Exception( "db transaction was already rolled back" );

        RolledBack = true;
        AddAudit( nameof( Rollback ) );
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose( disposing );
        AddAudit( $"{nameof( Dispose )}({disposing})" );
        if ( disposing )
            Connection.Transaction = null;
    }

    private void AddAudit(string value)
    {
        _audit.Add( value );
        Connection.AddAudit( this, value );
    }
}
