using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.TestExtensions.Sql.Mocks.System;

public sealed class DbDataReaderMock : DbDataReader
{
    private int _setIndex;
    private int _rowIndex = -1;

    public DbDataReaderMock(params ResultSet[] sets)
    {
        Id = -1;
        Sets = sets;
    }

    public DbDataReaderMock(DbCommandMock command)
    {
        Command = command;
        Id = Command.GetNextReaderId();
        Sets = Command.GetNextResultSets();
    }

    public int Id { get; }
    public DbCommandMock? Command { get; }
    public ResultSet[] Sets { get; }
    public bool ThrowOnDispose { get; init; }
    public override bool HasRows => _rowIndex < FieldCount;
    public override bool IsClosed => _setIndex >= Sets.Length;
    public override int Depth => 0;
    public override int RecordsAffected => 0;
    public override int FieldCount => IsClosed ? 0 : Sets[_setIndex].FieldNames.Length;
    public override object this[int i] => GetValue( i );
    public override object this[string name] => GetValue( GetOrdinal( name ) );

    [Pure]
    public override string ToString()
    {
        return $"DbDataReader[{Id}]";
    }

    [Pure]
    public override bool GetBoolean(int ordinal)
    {
        return Convert.ToBoolean( GetValue( ordinal ) );
    }

    [Pure]
    public override byte GetByte(int ordinal)
    {
        return Convert.ToByte( GetValue( ordinal ) );
    }

    [Pure]
    public byte[] GetBytes(int ordinal)
    {
        return ( byte[] )GetValue( ordinal );
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        var source = GetBytes( ordinal ).AsSpan( ( int )dataOffset, length );
        if ( buffer is not null )
            source.CopyTo( buffer.AsSpan( bufferOffset, length ) );

        return source.Length;
    }

    [Pure]
    public override char GetChar(int ordinal)
    {
        return Convert.ToChar( GetValue( ordinal ) );
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        var source = (( IEnumerable<char> )GetValue( ordinal )).ToArray().AsSpan( ( int )dataOffset, length );
        if ( buffer is not null )
            source.CopyTo( buffer.AsSpan( bufferOffset, length ) );

        return source.Length;
    }

    [Pure]
    public override string GetDataTypeName(int ordinal)
    {
        return GetFieldType( ordinal ).Name.ToLowerInvariant();
    }

    [Pure]
    public override DateTime GetDateTime(int ordinal)
    {
        return ( DateTime )GetValue( ordinal );
    }

    [Pure]
    public override decimal GetDecimal(int ordinal)
    {
        return Convert.ToDecimal( GetValue( ordinal ) );
    }

    [Pure]
    public override double GetDouble(int ordinal)
    {
        return Convert.ToDouble( GetValue( ordinal ) );
    }

    [Pure]
    public override Type GetFieldType(int ordinal)
    {
        return GetValue( ordinal ).GetType();
    }

    [Pure]
    public override float GetFloat(int ordinal)
    {
        return Convert.ToSingle( GetValue( ordinal ) );
    }

    [Pure]
    public override Guid GetGuid(int ordinal)
    {
        return ( Guid )GetValue( ordinal );
    }

    [Pure]
    public override short GetInt16(int ordinal)
    {
        return Convert.ToInt16( GetValue( ordinal ) );
    }

    [Pure]
    public override int GetInt32(int ordinal)
    {
        return Convert.ToInt32( GetValue( ordinal ) );
    }

    [Pure]
    public override long GetInt64(int ordinal)
    {
        return Convert.ToInt64( GetValue( ordinal ) );
    }

    [Pure]
    public override string GetName(int ordinal)
    {
        return Sets[_setIndex].FieldNames[ordinal];
    }

    [Pure]
    public override int GetOrdinal(string name)
    {
        return Array.FindIndex( Sets[_setIndex].FieldNames, n => n.Equals( name, StringComparison.OrdinalIgnoreCase ) );
    }

    [Pure]
    public override string GetString(int ordinal)
    {
        return ( string )GetValue( ordinal );
    }

    [Pure]
    public override object GetValue(int ordinal)
    {
        return Sets[_setIndex].Rows[_rowIndex][ordinal] ?? DBNull.Value;
    }

    [Pure]
    public override int GetValues(object[] values)
    {
        var row = Sets[_setIndex].Rows[_rowIndex];
        var length = Math.Min( row.Length, values.Length );
        Array.Copy( row, values, length );
        return length;
    }

    [Pure]
    public override bool IsDBNull(int ordinal)
    {
        return GetValue( ordinal ) is DBNull;
    }

    public override void Close()
    {
        if ( ThrowOnDispose )
            throw new Exception();

        _setIndex = Sets.Length;
        _rowIndex = -1;
        Command?.AddAudit( this, nameof( Close ) );
    }

    [Pure]
    public override DataTable? GetSchemaTable()
    {
        return null;
    }

    public override bool NextResult()
    {
        if ( IsClosed )
            return false;

        _rowIndex = -1;
        return ++_setIndex < Sets.Length;
    }

    public override bool Read()
    {
        if ( IsClosed )
            return false;

        var set = Sets[_setIndex];
        if ( _rowIndex >= set.Rows.Length )
            return false;

        return ++_rowIndex < set.Rows.Length;
    }

    [Pure]
    public override IEnumerator GetEnumerator()
    {
        var set = Sets[_setIndex];
        return set.Rows.Skip( _rowIndex ).GetEnumerator();
    }
}
