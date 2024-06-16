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

namespace LfrlAnvil.MySql;

/// <inheritdoc cref="ISqlDataTypeProvider" />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlDataTypeProvider : ISqlDataTypeProvider
{
    internal static readonly MySqlDataType Guid = MySqlDataType.CreateBinary( 16 );

    internal MySqlDataTypeProvider() { }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBool()" />
    [Pure]
    public MySqlDataType GetBool()
    {
        return MySqlDataType.Bool;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt8()" />
    [Pure]
    public MySqlDataType GetInt8()
    {
        return MySqlDataType.TinyInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt16()" />
    [Pure]
    public MySqlDataType GetInt16()
    {
        return MySqlDataType.SmallInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt32()" />
    [Pure]
    public MySqlDataType GetInt32()
    {
        return MySqlDataType.Int;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetInt64()" />
    [Pure]
    public MySqlDataType GetInt64()
    {
        return MySqlDataType.BigInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt8()" />
    [Pure]
    public MySqlDataType GetUInt8()
    {
        return MySqlDataType.UnsignedTinyInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt16()" />
    [Pure]
    public MySqlDataType GetUInt16()
    {
        return MySqlDataType.UnsignedSmallInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt32()" />
    [Pure]
    public MySqlDataType GetUInt32()
    {
        return MySqlDataType.UnsignedInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUInt64()" />
    [Pure]
    public MySqlDataType GetUInt64()
    {
        return MySqlDataType.UnsignedBigInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFloat()" />
    [Pure]
    public MySqlDataType GetFloat()
    {
        return MySqlDataType.Float;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDouble()" />
    [Pure]
    public MySqlDataType GetDouble()
    {
        return MySqlDataType.Double;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDecimal()" />
    [Pure]
    public MySqlDataType GetDecimal()
    {
        return MySqlDataType.Decimal;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDecimal(int,int)" />
    [Pure]
    public MySqlDataType GetDecimal(int precision, int scale)
    {
        return MySqlDataType.CreateDecimal( precision, scale );
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetGuid()" />
    [Pure]
    public MySqlDataType GetGuid()
    {
        return Guid;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetString()" />
    [Pure]
    public MySqlDataType GetString()
    {
        return MySqlDataType.VarChar;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetString(int)" />
    [Pure]
    public MySqlDataType GetString(int maxLength)
    {
        return maxLength <= MySqlDataType.VarChar.ParameterDefinitions[0].Bounds.Max
            ? MySqlDataType.CreateVarChar( maxLength )
            : MySqlDataType.Text;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedString()" />
    [Pure]
    public MySqlDataType GetFixedString()
    {
        return MySqlDataType.Char;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedString(int)" />
    [Pure]
    public MySqlDataType GetFixedString(int length)
    {
        return length <= MySqlDataType.Char.ParameterDefinitions[0].Bounds.Max
            ? MySqlDataType.CreateChar( length )
            : GetString( length );
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTimestamp()" />
    [Pure]
    public MySqlDataType GetTimestamp()
    {
        return MySqlDataType.BigInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetUtcDateTime()" />
    [Pure]
    public MySqlDataType GetUtcDateTime()
    {
        return MySqlDataType.DateTime;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDateTime()" />
    [Pure]
    public MySqlDataType GetDateTime()
    {
        return MySqlDataType.DateTime;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTimeSpan()" />
    [Pure]
    public MySqlDataType GetTimeSpan()
    {
        return MySqlDataType.BigInt;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetDate()" />
    [Pure]
    public MySqlDataType GetDate()
    {
        return MySqlDataType.Date;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetTime()" />
    [Pure]
    public MySqlDataType GetTime()
    {
        return MySqlDataType.Time;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBinary()" />
    [Pure]
    public MySqlDataType GetBinary()
    {
        return MySqlDataType.VarBinary;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetBinary(int)" />
    [Pure]
    public MySqlDataType GetBinary(int maxLength)
    {
        return maxLength <= MySqlDataType.VarBinary.ParameterDefinitions[0].Bounds.Max
            ? MySqlDataType.CreateVarBinary( maxLength )
            : MySqlDataType.Blob;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedBinary()" />
    [Pure]
    public MySqlDataType GetFixedBinary()
    {
        return MySqlDataType.Binary;
    }

    /// <inheritdoc cref="ISqlDataTypeProvider.GetFixedBinary(int)" />
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
