using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbTransactionMock : DbTransaction
{
    private readonly List<string> _audit = new List<string>();
    private readonly DbConnectionMock? _connection;

    public DbTransactionMock(DbConnectionMock? connection = null, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        _connection = connection;
        IsolationLevel = isolationLevel;
        Id = _connection?.CreatedTransactions.Count ?? -1;
    }

    public bool Committed { get; private set; }
    public bool RolledBack { get; private set; }
    public bool ThrowOnCommit { get; init; }
    public int Id { get; }
    public override IsolationLevel IsolationLevel { get; }
    protected override DbConnection? DbConnection => _connection;
    public IReadOnlyList<string> Audit => _audit;

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
        if ( disposing && _connection is not null )
            _connection.Transaction = null;
    }

    private void AddAudit(string value)
    {
        _audit.Add( value );
        _connection?.AddAudit( this, value );
    }
}
