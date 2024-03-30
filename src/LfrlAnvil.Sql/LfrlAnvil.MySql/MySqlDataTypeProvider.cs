using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;

namespace LfrlAnvil.MySql;

public sealed class MySqlDataTypeProvider : ISqlDataTypeProvider
{
    internal static readonly MySqlDataType Guid = MySqlDataType.CreateBinary( 16 );

    internal MySqlDataTypeProvider() { }

    [Pure]
    public MySqlDataType GetBool()
    {
        return MySqlDataType.Bool;
    }

    [Pure]
    public MySqlDataType GetInt8()
    {
        return MySqlDataType.TinyInt;
    }

    [Pure]
    public MySqlDataType GetInt16()
    {
        return MySqlDataType.SmallInt;
    }

    [Pure]
    public MySqlDataType GetInt32()
    {
        return MySqlDataType.Int;
    }

    [Pure]
    public MySqlDataType GetInt64()
    {
        return MySqlDataType.BigInt;
    }

    [Pure]
    public MySqlDataType GetUInt8()
    {
        return MySqlDataType.UnsignedTinyInt;
    }

    [Pure]
    public MySqlDataType GetUInt16()
    {
        return MySqlDataType.UnsignedSmallInt;
    }

    [Pure]
    public MySqlDataType GetUInt32()
    {
        return MySqlDataType.UnsignedInt;
    }

    [Pure]
    public MySqlDataType GetUInt64()
    {
        return MySqlDataType.UnsignedBigInt;
    }

    [Pure]
    public MySqlDataType GetFloat()
    {
        return MySqlDataType.Float;
    }

    [Pure]
    public MySqlDataType GetDouble()
    {
        return MySqlDataType.Double;
    }

    [Pure]
    public MySqlDataType GetDecimal()
    {
        return MySqlDataType.Decimal;
    }

    [Pure]
    public MySqlDataType GetDecimal(int precision, int scale)
    {
        return MySqlDataType.CreateDecimal( precision, scale );
    }

    [Pure]
    public MySqlDataType GetGuid()
    {
        return Guid;
    }

    [Pure]
    public MySqlDataType GetString()
    {
        return MySqlDataType.VarChar;
    }

    [Pure]
    public MySqlDataType GetString(int maxLength)
    {
        return maxLength <= MySqlDataType.VarChar.ParameterDefinitions[0].Bounds.Max
            ? MySqlDataType.CreateVarChar( maxLength )
            : MySqlDataType.Text;
    }

    [Pure]
    public MySqlDataType GetFixedString()
    {
        return MySqlDataType.Char;
    }

    [Pure]
    public MySqlDataType GetFixedString(int length)
    {
        return length <= MySqlDataType.Char.ParameterDefinitions[0].Bounds.Max
            ? MySqlDataType.CreateChar( length )
            : GetString( length );
    }

    [Pure]
    public MySqlDataType GetTimestamp()
    {
        return MySqlDataType.BigInt;
    }

    [Pure]
    public MySqlDataType GetUtcDateTime()
    {
        return MySqlDataType.DateTime;
    }

    [Pure]
    public MySqlDataType GetDateTime()
    {
        return MySqlDataType.DateTime;
    }

    [Pure]
    public MySqlDataType GetTimeSpan()
    {
        return MySqlDataType.BigInt;
    }

    [Pure]
    public MySqlDataType GetDate()
    {
        return MySqlDataType.Date;
    }

    [Pure]
    public MySqlDataType GetTime()
    {
        return MySqlDataType.Time;
    }

    [Pure]
    public MySqlDataType GetBinary()
    {
        return MySqlDataType.VarBinary;
    }

    [Pure]
    public MySqlDataType GetBinary(int maxLength)
    {
        return maxLength <= MySqlDataType.VarBinary.ParameterDefinitions[0].Bounds.Max
            ? MySqlDataType.CreateVarBinary( maxLength )
            : MySqlDataType.Blob;
    }

    [Pure]
    public MySqlDataType GetFixedBinary()
    {
        return MySqlDataType.Binary;
    }

    [Pure]
    public MySqlDataType GetFixedBinary(int length)
    {
        return length <= MySqlDataType.Binary.ParameterDefinitions[0].Bounds.Max
            ? MySqlDataType.CreateBinary( length )
            : GetBinary( length );
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
