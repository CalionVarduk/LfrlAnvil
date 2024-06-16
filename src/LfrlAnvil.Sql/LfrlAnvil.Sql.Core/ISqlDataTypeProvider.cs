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

using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a provider of <see cref="ISqlDataType"/> instances.
/// </summary>
public interface ISqlDataTypeProvider
{
    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="bool"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="bool"/> type.</returns>
    [Pure]
    ISqlDataType GetBool();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="sbyte"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="sbyte"/> type.</returns>
    [Pure]
    ISqlDataType GetInt8();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="short"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="short"/> type.</returns>
    [Pure]
    ISqlDataType GetInt16();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="int"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="int"/> type.</returns>
    [Pure]
    ISqlDataType GetInt32();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="long"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="long"/> type.</returns>
    [Pure]
    ISqlDataType GetInt64();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="byte"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="byte"/> type.</returns>
    [Pure]
    ISqlDataType GetUInt8();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="ushort"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="ushort"/> type.</returns>
    [Pure]
    ISqlDataType GetUInt16();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="uint"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="uint"/> type.</returns>
    [Pure]
    ISqlDataType GetUInt32();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="ulong"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="ulong"/> type.</returns>
    [Pure]
    ISqlDataType GetUInt64();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="float"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="float"/> type.</returns>
    [Pure]
    ISqlDataType GetFloat();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="double"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="double"/> type.</returns>
    [Pure]
    ISqlDataType GetDouble();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="decimal"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="decimal"/> type.</returns>
    [Pure]
    ISqlDataType GetDecimal();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="decimal"/> type.
    /// </summary>
    /// <param name="precision">Decimal precision.</param>
    /// <param name="scale">Decimal scale.</param>
    /// <returns><see cref="ISqlDataType"/> for <see cref="decimal"/> type.</returns>
    [Pure]
    ISqlDataType GetDecimal(int precision, int scale);

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="Guid"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="Guid"/> type.</returns>
    [Pure]
    ISqlDataType GetGuid();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="string"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="string"/> type.</returns>
    [Pure]
    ISqlDataType GetString();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="string"/> type.
    /// </summary>
    /// <param name="maxLength">Maximum length of a string.</param>
    /// <returns><see cref="ISqlDataType"/> for <see cref="string"/> type.</returns>
    [Pure]
    ISqlDataType GetString(int maxLength);

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="string"/> type of fixed length.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="string"/> type of fixed length.</returns>
    [Pure]
    ISqlDataType GetFixedString();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="string"/> type of fixed length.
    /// </summary>
    /// <param name="length">Length of a string.</param>
    /// <returns><see cref="ISqlDataType"/> for <see cref="string"/> type of fixed length.</returns>
    [Pure]
    ISqlDataType GetFixedString(int length);

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent a number of ticks elapsed
    /// since the <see cref="DateTime.UnixEpoch"/>.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for a number of ticks elapsed since the <see cref="DateTime.UnixEpoch"/>.</returns>
    [Pure]
    ISqlDataType GetTimestamp();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="DateTime"/> type
    /// of <see cref="DateTimeKind.Utc"/> kind.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="DateTime"/> type of <see cref="DateTimeKind.Utc"/> kind.</returns>
    [Pure]
    ISqlDataType GetUtcDateTime();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="DateTime"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="DateTime"/> type.</returns>
    [Pure]
    ISqlDataType GetDateTime();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="TimeSpan"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="TimeSpan"/> type.</returns>
    [Pure]
    ISqlDataType GetTimeSpan();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="DateOnly"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="DateOnly"/> type.</returns>
    [Pure]
    ISqlDataType GetDate();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent <see cref="TimeOnly"/> type.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for <see cref="TimeOnly"/> type.</returns>
    [Pure]
    ISqlDataType GetTime();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent an array of bytes.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for an array of bytes.</returns>
    [Pure]
    ISqlDataType GetBinary();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent an array of bytes.
    /// </summary>
    /// <param name="maxLength">Maximum length of an array.</param>
    /// <returns><see cref="ISqlDataType"/> for an array of bytes.</returns>
    [Pure]
    ISqlDataType GetBinary(int maxLength);

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent an array of bytes of fixed length.
    /// </summary>
    /// <returns><see cref="ISqlDataType"/> for an array of bytes of fixed length.</returns>
    [Pure]
    ISqlDataType GetFixedBinary();

    /// <summary>
    /// Returns an <see cref="ISqlDataType"/> instance best suited to represent an array of bytes of fixed length.
    /// </summary>
    /// <param name="length">Length of an array.</param>
    /// <returns><see cref="ISqlDataType"/> for an array of bytes of fixed length.</returns>
    [Pure]
    ISqlDataType GetFixedBinary(int length);
}
