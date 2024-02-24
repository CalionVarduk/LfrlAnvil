using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbConnectionMock : DbConnection
{
    private readonly List<string> _audit = new List<string>();
    private readonly List<DbCommandMock> _createdCommands = new List<DbCommandMock>();
    private readonly List<DbTransactionMock> _createdTransactions = new List<DbTransactionMock>();
    private readonly Queue<ResultSet[]> _resultSets = new Queue<ResultSet[]>();
    private ConnectionState _state = ConnectionState.Closed;

    public DbConnectionMock(string serverVersion = "0.0.0")
    {
        ServerVersion = serverVersion;
    }

    public IReadOnlyList<DbCommandMock> CreatedCommands => _createdCommands;
    public IReadOnlyList<DbTransactionMock> CreatedTransactions => _createdTransactions;

    [AllowNull]
    public override string ConnectionString { get; set; }

    public override string Database { get; } = string.Empty;
    public override ConnectionState State => _state;
    public override string DataSource { get; } = string.Empty;
    public override string ServerVersion { get; }
    public DbTransactionMock? Transaction { get; internal set; }
    public IReadOnlyList<string> Audit => _audit;

    public void EnqueueResultSets(ResultSet[] sets)
    {
        _resultSets.Enqueue( sets );
    }

    public override void Close()
    {
        if ( State != ConnectionState.Closed )
            ChangeState( ConnectionState.Closed );
    }

    public override void Open()
    {
        if ( State != ConnectionState.Open )
            ChangeState( ConnectionState.Open );
    }

    public override void ChangeDatabase(string databaseName)
    {
        throw new NotSupportedException();
    }

    protected override DbCommand CreateDbCommand()
    {
        var result = new DbCommandMock( this ) { ResultSets = _resultSets.TryDequeue( out var sets ) ? sets : Array.Empty<ResultSet>() };
        AddAudit( $"{nameof( CreateDbCommand )}({result})" );
        _createdCommands.Add( result );
        return result;
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        if ( Transaction is not null )
            throw new Exception( "active db transaction exists" );

        Transaction = new DbTransactionMock( this, isolationLevel );
        AddAudit( $"{nameof( BeginDbTransaction )}({Transaction}.{isolationLevel})" );
        _createdTransactions.Add( Transaction );
        return Transaction;
    }

    protected override void Dispose(bool disposing)
    {
        if ( disposing )
            Close();

        AddAudit( $"{nameof( Dispose )}({disposing})" );
        base.Dispose( disposing );
    }

    internal void AddAudit(DbCommandMock command, string value)
    {
        AddAudit( $"{command}.{value}" );
    }

    internal void AddAudit(DbTransactionMock transaction, string value)
    {
        AddAudit( $"{transaction}.{value}" );
    }

    private void AddAudit(string value)
    {
        _audit.Add( value );
    }

    private void ChangeState(ConnectionState next)
    {
        AddAudit( $"{nameof( ChangeState )}({State} => {next})" );
        var prev = State;
        _state = next;
        OnStateChange( new StateChangeEventArgs( prev, next ) );
    }
}
