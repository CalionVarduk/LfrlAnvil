using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteDataTypeProvider : ISqlDataTypeProvider
{
    internal SqliteDataTypeProvider() { }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBool()" />
    [Pure]
    public SqliteDataType GetBool()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt8()" />
    [Pure]
    public SqliteDataType GetInt8()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt16()" />
    [Pure]
    public SqliteDataType GetInt16()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt32()" />
    [Pure]
    public SqliteDataType GetInt32()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt64()" />
    [Pure]
    public SqliteDataType GetInt64()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt8()" />
    [Pure]
    public SqliteDataType GetUInt8()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt16()" />
    [Pure]
    public SqliteDataType GetUInt16()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt32()" />
    [Pure]
    public SqliteDataType GetUInt32()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt64()" />
    [Pure]
    public SqliteDataType GetUInt64()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFloat()" />
    [Pure]
    public SqliteDataType GetFloat()
    {
        return SqliteDataType.Real;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDouble()" />
    [Pure]
    public SqliteDataType GetDouble()
    {
        return SqliteDataType.Real;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDecimal()" />
    [Pure]
    public SqliteDataType GetDecimal()
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDecimal(int,int)" />
    [Pure]
    public SqliteDataType GetDecimal(int precision, int scale)
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetGuid()" />
    [Pure]
    public SqliteDataType GetGuid()
    {
        return SqliteDataType.Blob;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetString()" />
    [Pure]
    public SqliteDataType GetString()
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetString(int)" />
    [Pure]
    public SqliteDataType GetString(int maxLength)
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedString()" />
    [Pure]
    public SqliteDataType GetFixedString()
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedString(int)" />
    [Pure]
    public SqliteDataType GetFixedString(int length)
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTimestamp()" />
    [Pure]
    public SqliteDataType GetTimestamp()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUtcDateTime()" />
    [Pure]
    public SqliteDataType GetUtcDateTime()
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDateTime()" />
    [Pure]
    public SqliteDataType GetDateTime()
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTimeSpan()" />
    [Pure]
    public SqliteDataType GetTimeSpan()
    {
        return SqliteDataType.Integer;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDate()" />
    [Pure]
    public SqliteDataType GetDate()
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTime()" />
    [Pure]
    public SqliteDataType GetTime()
    {
        return SqliteDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBinary()" />
    [Pure]
    public SqliteDataType GetBinary()
    {
        return SqliteDataType.Blob;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBinary(int)" />
    [Pure]
    public SqliteDataType GetBinary(int maxLength)
    {
        return SqliteDataType.Blob;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedBinary()" />
    [Pure]
    public SqliteDataType GetFixedBinary()
    {
        return SqliteDataType.Blob;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedBinary(int)" />
    [Pure]
    public SqliteDataType GetFixedBinary(int length)
    {
        return SqliteDataType.Blob;
    }

    /// <summary>
    /// Returns <see cref="SqliteDataType.Any"/> type.
    /// </summary>
    /// <returns><see cref="SqliteDataType.Any"/>.</returns>
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
    ISqlDataType ISqlDataTypeProvider.GetString(int maxLength)
    {
        return GetString( maxLength );
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetFixedString()
    {
        return GetFixedString();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetFixedString(int length)
    {
        return GetFixedString( length );
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetTimestamp()
    {
        return GetTimestamp();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetUtcDateTime()
    {
        return GetUtcDateTime();
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
    ISqlDataType ISqlDataTypeProvider.GetBinary(int maxLength)
    {
        return GetBinary( maxLength );
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetFixedBinary()
    {
        return GetFixedBinary();
    }

    [Pure]
    ISqlDataType ISqlDataTypeProvider.GetFixedBinary(int length)
    {
        return GetFixedBinary( length );
    }
}
