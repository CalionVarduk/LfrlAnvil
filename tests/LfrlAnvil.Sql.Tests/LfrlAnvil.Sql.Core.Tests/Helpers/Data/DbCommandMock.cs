using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbCommandMock : DbCommand
{
    private readonly List<string> _audit = new List<string>();
    private readonly DbConnectionMock? _connection;
    private int _createdReaderCount;
    private int _createdParameterCount;

    public DbCommandMock(DbConnectionMock? connection = null)
    {
        _connection = connection;
        Transaction = _connection?.Transaction;
        Id = _connection?.CreatedCommands.Count ?? -1;
        Parameters = new DbDataParameterCollectionMock();
    }

    [AllowNull]
    public override string CommandText { get; set; }

    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public ResultSet[] ResultSets { get; init; } = Array.Empty<ResultSet>();
    public int NonQueryResult { get; init; }
    public int Id { get; }
    public new DbDataParameterCollectionMock Parameters { get; }
    public IReadOnlyList<string> Audit => _audit;

    protected override DbConnection? DbConnection
    {
        get => _connection;
        set => throw new NotSupportedException();
    }

    protected override DbTransaction? DbTransaction { get; set; }
    protected override DbParameterCollection DbParameterCollection => Parameters;

    [Pure]
    public override string ToString()
    {
        return Transaction is null ? $"DbCommand[{Id}]" : $"{Transaction}:DbCommand[{Id}]";
    }

    public override void Cancel()
    {
        AddAudit( nameof( Cancel ) );
    }

    [Pure]
    public new DbDataParameterMock CreateParameter()
    {
        var result = new DbDataParameterMock { Command = this, Id = _createdParameterCount++ };
        AddAudit( $"{nameof( CreateParameter )}({result})" );
        return result;
    }

    public override int ExecuteNonQuery()
    {
        AddAudit( $"{nameof( ExecuteNonQuery )}({CommandText})" );
        return NonQueryResult;
    }

    [Pure]
    public new DbDataReaderMock ExecuteReader()
    {
        var result = new DbDataReaderMock( ResultSets ) { Command = this, Id = _createdReaderCount++ };
        AddAudit( $"{nameof( ExecuteReader )}({result} => {CommandText})" );
        return result;
    }

    [Pure]
    public override object? ExecuteScalar()
    {
        using var reader = new DbDataReaderMock( ResultSets ) { Command = this, Id = _createdReaderCount++ };
        AddAudit( $"{nameof( ExecuteScalar )}({reader} => {CommandText})" );
        return reader.Read() ? reader.GetValue( 0 ) : null;
    }

    public override void Prepare()
    {
        AddAudit( nameof( Prepare ) );
    }

    [Pure]
    protected override DbParameter CreateDbParameter()
    {
        return CreateParameter();
    }

    [Pure]
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return ExecuteReader();
    }

    protected override void Dispose(bool disposing)
    {
        AddAudit( $"{nameof( Dispose )}({disposing})" );
        base.Dispose( disposing );
    }

    internal void AddAudit(DbDataReaderMock reader, string value)
    {
        AddAudit( $"{reader}.{value}" );
    }

    internal void AddAudit(DbDataParameterMock parameter, string value)
    {
        AddAudit( $"{parameter}.{value}" );
    }

    private void AddAudit(string value)
    {
        _audit.Add( value );
        _connection?.AddAudit( this, value );
    }
}
