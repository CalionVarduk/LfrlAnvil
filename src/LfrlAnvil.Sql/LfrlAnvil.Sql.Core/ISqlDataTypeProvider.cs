using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

public interface ISqlDataTypeProvider
{
    [Pure]
    ISqlDataType GetBool();

    [Pure]
    ISqlDataType GetInt8();

    [Pure]
    ISqlDataType GetInt16();

    [Pure]
    ISqlDataType GetInt32();

    [Pure]
    ISqlDataType GetInt64();

    [Pure]
    ISqlDataType GetUInt8();

    [Pure]
    ISqlDataType GetUInt16();

    [Pure]
    ISqlDataType GetUInt32();

    [Pure]
    ISqlDataType GetUInt64();

    [Pure]
    ISqlDataType GetFloat();

    [Pure]
    ISqlDataType GetDouble();

    [Pure]
    ISqlDataType GetDecimal();

    [Pure]
    ISqlDataType GetDecimal(int precision, int scale);

    [Pure]
    ISqlDataType GetGuid();

    [Pure]
    ISqlDataType GetString();

    [Pure]
    ISqlDataType GetString(int maxLength);

    [Pure]
    ISqlDataType GetFixedString();

    [Pure]
    ISqlDataType GetFixedString(int length);

    [Pure]
    ISqlDataType GetTimestamp();

    [Pure]
    ISqlDataType GetDateTime();

    [Pure]
    ISqlDataType GetTimeSpan();

    [Pure]
    ISqlDataType GetDate();

    [Pure]
    ISqlDataType GetTime();

    [Pure]
    ISqlDataType GetBinary();

    [Pure]
    ISqlDataType GetBinary(int maxLength);

    [Pure]
    ISqlDataType GetFixedBinary();

    [Pure]
    ISqlDataType GetFixedBinary(int length);
}
