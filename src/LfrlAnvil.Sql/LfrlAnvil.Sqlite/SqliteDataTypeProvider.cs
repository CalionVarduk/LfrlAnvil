using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteDataTypeProvider : ISqlDataTypeProvider
{
    internal SqliteDataTypeProvider() { }

    [Pure]
    public SqliteDataType GetBool()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetInt8()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetInt16()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetInt32()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetInt64()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetUInt8()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetUInt16()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetUInt32()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetUInt64()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetFloat()
    {
        return SqliteDataType.Real;
    }

    [Pure]
    public SqliteDataType GetDouble()
    {
        return SqliteDataType.Real;
    }

    [Pure]
    public SqliteDataType GetDecimal()
    {
        return SqliteDataType.Text;
    }

    [Pure]
    public SqliteDataType GetDecimal(int precision, int scale)
    {
        Ensure.IsGreaterThan( precision, 0, nameof( precision ) );
        Ensure.IsInRange( scale, 0, precision, nameof( scale ) );
        return SqliteDataType.Text;
    }

    [Pure]
    public SqliteDataType GetGuid()
    {
        return SqliteDataType.Blob;
    }

    [Pure]
    public SqliteDataType GetString()
    {
        return SqliteDataType.Text;
    }

    [Pure]
    public SqliteDataType GetString(int length)
    {
        Ensure.IsGreaterThan( length, 0, nameof( length ) );
        return SqliteDataType.Text;
    }

    [Pure]
    public SqliteDataType GetTimestamp()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetDateTime()
    {
        return SqliteDataType.Text;
    }

    [Pure]
    public SqliteDataType GetTimeSpan()
    {
        return SqliteDataType.Integer;
    }

    [Pure]
    public SqliteDataType GetDate()
    {
        return SqliteDataType.Text;
    }

    [Pure]
    public SqliteDataType GetTime()
    {
        return SqliteDataType.Text;
    }

    [Pure]
    public SqliteDataType GetBinary()
    {
        return SqliteDataType.Blob;
    }

    [Pure]
    public SqliteDataType GetBinary(int length)
    {
        Ensure.IsGreaterThan( length, 0, nameof( length ) );
        return SqliteDataType.Blob;
    }

    [Pure]
    public SqliteDataType GetAny()
    {
        return SqliteDataType.Any;
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetBool()
    {
        return GetBool();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetInt8()
    {
        return GetInt8();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetInt16()
    {
        return GetInt16();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetInt32()
    {
        return GetInt32();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetInt64()
    {
        return GetInt64();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetUInt8()
    {
        return GetUInt8();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetUInt16()
    {
        return GetUInt16();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetUInt32()
    {
        return GetUInt32();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetUInt64()
    {
        return GetUInt64();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetFloat()
    {
        return GetFloat();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetDouble()
    {
        return GetDouble();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetDecimal()
    {
        return GetDecimal();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetDecimal(int precision, int scale)
    {
        return GetDecimal( precision, scale );
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetGuid()
    {
        return GetGuid();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetString()
    {
        return GetString();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetString(int length)
    {
        return GetString( length );
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetTimestamp()
    {
        return GetTimestamp();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetDateTime()
    {
        return GetDateTime();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetTimeSpan()
    {
        return GetTimeSpan();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetDate()
    {
        return GetDate();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetTime()
    {
        return GetTime();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetBinary()
    {
        return GetBinary();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetBinary(int length)
    {
        return GetBinary( length );
    }
}
