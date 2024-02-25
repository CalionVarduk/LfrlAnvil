using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDataTypeProviderMock : ISqlDataTypeProvider
{
    [Pure]
    public SqlDataTypeMock GetBool()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetInt8()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetInt16()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetInt32()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetInt64()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetUInt8()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetUInt16()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetUInt32()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetUInt64()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetFloat()
    {
        return SqlDataTypeMock.Real;
    }

    [Pure]
    public SqlDataTypeMock GetDouble()
    {
        return SqlDataTypeMock.Real;
    }

    [Pure]
    public SqlDataTypeMock GetDecimal()
    {
        return SqlDataTypeMock.Real;
    }

    [Pure]
    public SqlDataTypeMock GetDecimal(int precision, int scale)
    {
        return SqlDataTypeMock.Real;
    }

    [Pure]
    public SqlDataTypeMock GetGuid()
    {
        return SqlDataTypeMock.Binary;
    }

    [Pure]
    public SqlDataTypeMock GetString()
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetString(int maxLength)
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetFixedString()
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetFixedString(int length)
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetTimestamp()
    {
        return SqlDataTypeMock.Integer;
    }

    [Pure]
    public SqlDataTypeMock GetDateTime()
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetTimeSpan()
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetDate()
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetTime()
    {
        return SqlDataTypeMock.Text;
    }

    [Pure]
    public SqlDataTypeMock GetBinary()
    {
        return SqlDataTypeMock.Binary;
    }

    [Pure]
    public SqlDataTypeMock GetBinary(int maxLength)
    {
        return SqlDataTypeMock.Binary;
    }

    [Pure]
    public SqlDataTypeMock GetFixedBinary()
    {
        return SqlDataTypeMock.Binary;
    }

    [Pure]
    public SqlDataTypeMock GetFixedBinary(int length)
    {
        return SqlDataTypeMock.Binary;
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
