using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Sql.Tests.Helpers.Data;

public sealed class DbDataReader : IDataReader
{
    private int _setIndex;
    private int _rowIndex;

    public DbDataReader(params ResultSet[] sets)
    {
        Sets = sets;
        _setIndex = 0;
        _rowIndex = -1;
    }

    public DbCommand? Command { get; set; }
    public bool ThrowOnDispose { get; set; } = false;
    public ResultSet[] Sets { get; }
    public bool IsClosed => _setIndex >= Sets.Length;
    public int Depth => 0;
    public int RecordsAffected => 0;
    public int FieldCount => IsClosed ? 0 : Sets[_setIndex].FieldNames.Length;
    public object this[int i] => GetValue( i );
    public object this[string name] => GetValue( GetOrdinal( name ) );

    [Pure]
    public bool GetBoolean(int i)
    {
        return Convert.ToBoolean( GetValue( i ) );
    }

    [Pure]
    public byte GetByte(int i)
    {
        return Convert.ToByte( GetValue( i ) );
    }

    [Pure]
    public byte[] GetBytes(int i)
    {
        return (byte[])GetValue( i );
    }

    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
    {
        var source = GetBytes( i ).AsSpan( (int)fieldOffset, length );
        if ( buffer is not null )
            source.CopyTo( buffer.AsSpan( bufferoffset, length ) );

        return source.Length;
    }

    [Pure]
    public char GetChar(int i)
    {
        return Convert.ToChar( GetValue( i ) );
    }

    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
    {
        var source = ((IEnumerable<char>)GetValue( i )).ToArray().AsSpan( (int)fieldoffset, length );
        if ( buffer is not null )
            source.CopyTo( buffer.AsSpan( bufferoffset, length ) );

        return source.Length;
    }

    [Pure]
    [DoesNotReturn]
    public IDataReader GetData(int i)
    {
        throw new NotSupportedException();
    }

    [Pure]
    public string GetDataTypeName(int i)
    {
        return GetFieldType( i ).Name.ToLowerInvariant();
    }

    [Pure]
    public DateTime GetDateTime(int i)
    {
        return (DateTime)GetValue( i );
    }

    [Pure]
    public decimal GetDecimal(int i)
    {
        return Convert.ToDecimal( GetValue( i ) );
    }

    [Pure]
    public double GetDouble(int i)
    {
        return Convert.ToDouble( GetValue( i ) );
    }

    [Pure]
    public Type GetFieldType(int i)
    {
        return GetValue( i ).GetType();
    }

    [Pure]
    public float GetFloat(int i)
    {
        return Convert.ToSingle( GetValue( i ) );
    }

    [Pure]
    public Guid GetGuid(int i)
    {
        return (Guid)GetValue( i );
    }

    [Pure]
    public short GetInt16(int i)
    {
        return Convert.ToInt16( GetValue( i ) );
    }

    [Pure]
    public int GetInt32(int i)
    {
        return Convert.ToInt32( GetValue( i ) );
    }

    [Pure]
    public long GetInt64(int i)
    {
        return Convert.ToInt64( GetValue( i ) );
    }

    [Pure]
    public string GetName(int i)
    {
        return Sets[_setIndex].FieldNames[i];
    }

    [Pure]
    public int GetOrdinal(string name)
    {
        return Array.FindIndex( Sets[_setIndex].FieldNames, n => n.Equals( name, StringComparison.OrdinalIgnoreCase ) );
    }

    [Pure]
    public string GetString(int i)
    {
        return (string)GetValue( i );
    }

    [Pure]
    public object GetValue(int i)
    {
        return Sets[_setIndex].Rows[_rowIndex][i] ?? DBNull.Value;
    }

    [Pure]
    public int GetValues(object[] values)
    {
        var row = Sets[_setIndex].Rows[_rowIndex];
        var length = Math.Min( row.Length, values.Length );
        Array.Copy( row, values, length );
        return length;
    }

    [Pure]
    public bool IsDBNull(int i)
    {
        return GetValue( i ) is DBNull;
    }

    public void Dispose()
    {
        if ( ThrowOnDispose )
            throw new Exception();

        Close();
    }

    public void Close()
    {
        _setIndex = Sets.Length;
        _rowIndex = -1;
        ((IList<string>?)Command?.Audit)?.Add( "DbDataReader.Close" );
    }

    [Pure]
    public DataTable? GetSchemaTable()
    {
        return null;
    }

    public bool NextResult()
    {
        if ( IsClosed )
            return false;

        _rowIndex = -1;
        return ++_setIndex < Sets.Length;
    }

    public bool Read()
    {
        if ( IsClosed )
            return false;

        var set = Sets[_setIndex];
        if ( _rowIndex >= set.Rows.Length )
            return false;

        return ++_rowIndex < set.Rows.Length;
    }
}
