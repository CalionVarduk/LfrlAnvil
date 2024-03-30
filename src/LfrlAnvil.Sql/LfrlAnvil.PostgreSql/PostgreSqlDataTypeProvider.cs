using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql;

public sealed class PostgreSqlDataTypeProvider : ISqlDataTypeProvider
{
    internal PostgreSqlDataTypeProvider() { }

    [Pure]
    public PostgreSqlDataType GetBool()
    {
        return PostgreSqlDataType.Boolean;
    }

    [Pure]
    public PostgreSqlDataType GetInt8()
    {
        return PostgreSqlDataType.Int2;
    }

    [Pure]
    public PostgreSqlDataType GetInt16()
    {
        return PostgreSqlDataType.Int2;
    }

    [Pure]
    public PostgreSqlDataType GetInt32()
    {
        return PostgreSqlDataType.Int4;
    }

    [Pure]
    public PostgreSqlDataType GetInt64()
    {
        return PostgreSqlDataType.Int8;
    }

    [Pure]
    public PostgreSqlDataType GetUInt8()
    {
        return PostgreSqlDataType.Int2;
    }

    [Pure]
    public PostgreSqlDataType GetUInt16()
    {
        return PostgreSqlDataType.Int4;
    }

    [Pure]
    public PostgreSqlDataType GetUInt32()
    {
        return PostgreSqlDataType.Int8;
    }

    [Pure]
    public PostgreSqlDataType GetUInt64()
    {
        return PostgreSqlDataType.Int8;
    }

    [Pure]
    public PostgreSqlDataType GetFloat()
    {
        return PostgreSqlDataType.Float4;
    }

    [Pure]
    public PostgreSqlDataType GetDouble()
    {
        return PostgreSqlDataType.Float8;
    }

    [Pure]
    public PostgreSqlDataType GetDecimal()
    {
        return PostgreSqlDataType.Decimal;
    }

    [Pure]
    public PostgreSqlDataType GetDecimal(int precision, int scale)
    {
        return PostgreSqlDataType.CreateDecimal( precision, scale );
    }

    [Pure]
    public PostgreSqlDataType GetGuid()
    {
        return PostgreSqlDataType.Uuid;
    }

    [Pure]
    public PostgreSqlDataType GetString()
    {
        return PostgreSqlDataType.VarChar;
    }

    [Pure]
    public PostgreSqlDataType GetString(int maxLength)
    {
        return PostgreSqlDataType.CreateVarChar( maxLength );
    }

    [Pure]
    public PostgreSqlDataType GetFixedString()
    {
        return PostgreSqlDataType.VarChar;
    }

    [Pure]
    public PostgreSqlDataType GetFixedString(int length)
    {
        return PostgreSqlDataType.CreateVarChar( length );
    }

    [Pure]
    public PostgreSqlDataType GetTimestamp()
    {
        return PostgreSqlDataType.Int8;
    }

    [Pure]
    public PostgreSqlDataType GetDateTime()
    {
        return PostgreSqlDataType.Timestamp;
    }

    [Pure]
    public PostgreSqlDataType GetTimeSpan()
    {
        return PostgreSqlDataType.Int8;
    }

    [Pure]
    public PostgreSqlDataType GetDate()
    {
        return PostgreSqlDataType.Date;
    }

    [Pure]
    public PostgreSqlDataType GetTime()
    {
        return PostgreSqlDataType.Time;
    }

    [Pure]
    public PostgreSqlDataType GetBinary()
    {
        return PostgreSqlDataType.Bytea;
    }

    [Pure]
    public PostgreSqlDataType GetBinary(int maxLength)
    {
        return PostgreSqlDataType.Bytea;
    }

    [Pure]
    public PostgreSqlDataType GetFixedBinary()
    {
        return PostgreSqlDataType.Bytea;
    }

    [Pure]
    public PostgreSqlDataType GetFixedBinary(int length)
    {
        return PostgreSqlDataType.Bytea;
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
