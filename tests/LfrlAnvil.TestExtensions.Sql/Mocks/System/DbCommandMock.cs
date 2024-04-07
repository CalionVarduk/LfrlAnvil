using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.TestExtensions.Sql.Mocks.System;

public sealed class DbCommandMock : DbCommand
{
    private readonly List<string> _audit = new List<string>();
    private readonly ResultSet[] _resultSets = Array.Empty<ResultSet>();
    private int _nextReaderId;
    private int _nextParameterId;

    public DbCommandMock(params ResultSet[] resultSets)
    {
        Id = -1;
        _resultSets = resultSets;
    }

    public DbCommandMock(DbConnectionMock connection)
    {
        Connection = connection;
        Transaction = Connection.Transaction;
        Id = Connection.CreatedCommands.Count;
    }

    public int Id { get; }

    [AllowNull]
    public override string CommandText { get; set; }

    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public int NonQueryResult { get; init; }
    public new DbConnectionMock? Connection { get; }
    public new DbTransactionMock? Transaction { get; private set; }
    public new DbParameterCollectionMock Parameters { get; } = new DbParameterCollectionMock();
    public IReadOnlyList<string> Audit => _audit;

    protected override DbConnection? DbConnection
    {
        get => Connection;
        set => throw new NotSupportedException();
    }

    protected override DbTransaction? DbTransaction
    {
        get => Transaction;
        set => Transaction = ( DbTransactionMock? )value;
    }

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
    public new DbParameterMock CreateParameter()
    {
        var result = new DbParameterMock( this );
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
        var result = new DbDataReaderMock( this );
        AddAudit( $"{nameof( ExecuteReader )}({result} => {CommandText})" );
        return result;
    }

    [Pure]
    public override object? ExecuteScalar()
    {
        using var reader = new DbDataReaderMock( this );
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

    internal int GetNextReaderId()
    {
        return _nextReaderId++;
    }

    internal int GetNextParameterId()
    {
        return _nextParameterId++;
    }

    internal ResultSet[] GetNextResultSets()
    {
        return Connection?.GetNextResultSets() ?? _resultSets;
    }

    internal void AddAudit(DbDataReaderMock reader, string value)
    {
        AddAudit( $"{reader}.{value}" );
    }

    internal void AddAudit(DbParameterMock parameter, string value)
    {
        AddAudit( $"{parameter}.{value}" );
    }

    private void AddAudit(string value)
    {
        _audit.Add( value );
        Connection?.AddAudit( this, value );
    }
}
