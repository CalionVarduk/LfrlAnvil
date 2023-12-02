using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbCommand : IDbCommand
{
    private readonly List<string> _audit = new List<string>();

    [AllowNull]
    public string CommandText { get; set; }

    public int CommandTimeout { get; set; }
    public CommandType CommandType { get; set; }
    public IDbConnection? Connection { get; set; }
    public IDbTransaction? Transaction { get; set; }
    public UpdateRowSource UpdatedRowSource { get; set; }
    public ResultSet[] ResultSets { get; set; } = Array.Empty<ResultSet>();
    public DbDataParameterCollection Parameters { get; } = new DbDataParameterCollection();
    public IReadOnlyList<string> Audit => _audit;

    IDataParameterCollection IDbCommand.Parameters => Parameters;

    public void Dispose()
    {
        _audit.Add( nameof( Dispose ) );
    }

    public void Cancel()
    {
        _audit.Add( nameof( Cancel ) );
    }

    [Pure]
    public DbDataParameter CreateParameter()
    {
        _audit.Add( nameof( CreateParameter ) );
        return new DbDataParameter();
    }

    public int ExecuteNonQuery()
    {
        _audit.Add( nameof( ExecuteNonQuery ) );
        return 0;
    }

    [Pure]
    public DbDataReader ExecuteReader()
    {
        _audit.Add( nameof( ExecuteReader ) );
        return new DbDataReader( ResultSets );
    }

    [Pure]
    public object? ExecuteScalar()
    {
        _audit.Add( nameof( ExecuteScalar ) );
        using var reader = new DbDataReader( ResultSets );
        return reader.Read() ? reader.GetValue( 0 ) : null;
    }

    public void Prepare()
    {
        _audit.Add( nameof( Prepare ) );
    }

    public void ClearAudit()
    {
        _audit.Clear();
    }

    [Pure]
    IDbDataParameter IDbCommand.CreateParameter()
    {
        _audit.Add( "[explicit]" );
        return CreateParameter();
    }

    [Pure]
    IDataReader IDbCommand.ExecuteReader()
    {
        _audit.Add( "[explicit]" );
        return ExecuteReader();
    }

    [Pure]
    IDataReader IDbCommand.ExecuteReader(CommandBehavior behavior)
    {
        _audit.Add( $"[explicit]({behavior})" );
        return ExecuteReader();
    }
}
