// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc cref="ISqlDataTypeProvider" />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlDataTypeProvider : ISqlDataTypeProvider
{
    internal PostgreSqlDataTypeProvider() { }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBool()" />
    [Pure]
    public PostgreSqlDataType GetBool()
    {
        return PostgreSqlDataType.Boolean;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt8()" />
    [Pure]
    public PostgreSqlDataType GetInt8()
    {
        return PostgreSqlDataType.Int2;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt16()" />
    [Pure]
    public PostgreSqlDataType GetInt16()
    {
        return PostgreSqlDataType.Int2;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt32()" />
    [Pure]
    public PostgreSqlDataType GetInt32()
    {
        return PostgreSqlDataType.Int4;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt64()" />
    [Pure]
    public PostgreSqlDataType GetInt64()
    {
        return PostgreSqlDataType.Int8;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt8()" />
    [Pure]
    public PostgreSqlDataType GetUInt8()
    {
        return PostgreSqlDataType.Int2;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt16()" />
    [Pure]
    public PostgreSqlDataType GetUInt16()
    {
        return PostgreSqlDataType.Int4;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt32()" />
    [Pure]
    public PostgreSqlDataType GetUInt32()
    {
        return PostgreSqlDataType.Int8;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt64()" />
    [Pure]
    public PostgreSqlDataType GetUInt64()
    {
        return PostgreSqlDataType.Int8;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFloat()" />
    [Pure]
    public PostgreSqlDataType GetFloat()
    {
        return PostgreSqlDataType.Float4;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDouble()" />
    [Pure]
    public PostgreSqlDataType GetDouble()
    {
        return PostgreSqlDataType.Float8;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDecimal()" />
    [Pure]
    public PostgreSqlDataType GetDecimal()
    {
        return PostgreSqlDataType.Decimal;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDecimal(int,int)" />
    [Pure]
    public PostgreSqlDataType GetDecimal(int precision, int scale)
    {
        return PostgreSqlDataType.CreateDecimal( precision, scale );
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetGuid()" />
    [Pure]
    public PostgreSqlDataType GetGuid()
    {
        return PostgreSqlDataType.Uuid;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetString()" />
    [Pure]
    public PostgreSqlDataType GetString()
    {
        return PostgreSqlDataType.VarChar;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetString(int)" />
    [Pure]
    public PostgreSqlDataType GetString(int maxLength)
    {
        return PostgreSqlDataType.CreateVarChar( maxLength );
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedString()" />
    [Pure]
    public PostgreSqlDataType GetFixedString()
    {
        return PostgreSqlDataType.VarChar;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedString(int)" />
    [Pure]
    public PostgreSqlDataType GetFixedString(int length)
    {
        return PostgreSqlDataType.CreateVarChar( length );
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTimestamp()" />
    [Pure]
    public PostgreSqlDataType GetTimestamp()
    {
        return PostgreSqlDataType.Int8;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUtcDateTime()" />
    [Pure]
    public PostgreSqlDataType GetUtcDateTime()
    {
        return PostgreSqlDataType.TimestampTz;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDateTime()" />
    [Pure]
    public PostgreSqlDataType GetDateTime()
    {
        return PostgreSqlDataType.Timestamp;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTimeSpan()" />
    [Pure]
    public PostgreSqlDataType GetTimeSpan()
    {
        return PostgreSqlDataType.Int8;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDate()" />
    [Pure]
    public PostgreSqlDataType GetDate()
    {
        return PostgreSqlDataType.Date;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTime()" />
    [Pure]
    public PostgreSqlDataType GetTime()
    {
        return PostgreSqlDataType.Time;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBinary()" />
    [Pure]
    public PostgreSqlDataType GetBinary()
    {
        return PostgreSqlDataType.Bytea;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBinary(int)" />
    [Pure]
    public PostgreSqlDataType GetBinary(int maxLength)
    {
        return PostgreSqlDataType.Bytea;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedBinary()" />
    [Pure]
    public PostgreSqlDataType GetFixedBinary()
    {
        return PostgreSqlDataType.Bytea;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedBinary(int)" />
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
