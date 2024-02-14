using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbConnection : System.Data.Common.DbConnection
{
    private ConnectionState _state = ConnectionState.Closed;

    [AllowNull]
    public override string ConnectionString { get; set; }

    public override string Database { get; } = string.Empty;
    public override ConnectionState State => _state;
    public override string DataSource { get; } = string.Empty;
    public override string ServerVersion { get; } = "0.0.0";

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

    protected override System.Data.Common.DbCommand CreateDbCommand()
    {
        return new DbCommand( this );
    }

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        throw new NotSupportedException();
    }

    private void ChangeState(ConnectionState next)
    {
        var prev = State;
        _state = next;
        OnStateChange( new StateChangeEventArgs( prev, next ) );
    }
}
