using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbCommand : System.Data.Common.DbCommand
{
    private readonly List<string> _audit = new List<string>();

    [AllowNull]
    public override string CommandText { get; set; }

    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public ResultSet[] ResultSets { get; init; } = Array.Empty<ResultSet>();
    public new DbDataParameterCollection Parameters { get; } = new DbDataParameterCollection();
    public IReadOnlyList<string> Audit => _audit;

    protected override DbConnection? DbConnection { get; set; }
    protected override DbTransaction? DbTransaction { get; set; }
    protected override DbParameterCollection DbParameterCollection => Parameters;

    public override void Cancel()
    {
        _audit.Add( nameof( Cancel ) );
    }

    [Pure]
    public new DbDataParameter CreateParameter()
    {
        _audit.Add( nameof( CreateParameter ) );
        return new DbDataParameter();
    }

    public override int ExecuteNonQuery()
    {
        _audit.Add( nameof( ExecuteNonQuery ) );
        return 0;
    }

    [Pure]
    public new DbDataReader ExecuteReader()
    {
        _audit.Add( nameof( ExecuteReader ) );
        return new DbDataReader( ResultSets ) { Command = this };
    }

    [Pure]
    public override object? ExecuteScalar()
    {
        _audit.Add( nameof( ExecuteScalar ) );
        using var reader = new DbDataReader( ResultSets ) { Command = this };
        return reader.Read() ? reader.GetValue( 0 ) : null;
    }

    public override void Prepare()
    {
        _audit.Add( nameof( Prepare ) );
    }

    public void ClearAudit()
    {
        _audit.Clear();
    }

    [Pure]
    protected override DbParameter CreateDbParameter()
    {
        return CreateParameter();
    }

    [Pure]
    protected override System.Data.Common.DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return ExecuteReader();
    }

    protected override void Dispose(bool disposing)
    {
        _audit.Add( $"{nameof( Dispose )}({disposing})" );
        base.Dispose( disposing );
    }
}
