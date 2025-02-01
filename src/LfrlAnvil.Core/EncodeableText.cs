// Copyright 2025 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil;

/// <summary>
/// Allows to safely encode a set of characters into a span of bytes.
/// </summary>
public readonly struct EncodeableText
{
    private readonly Encoding? _encoding;

    private EncodeableText(Encoding encoding, ReadOnlyMemory<char> value, int byteCount)
    {
        _encoding = encoding;
        Value = value;
        ByteCount = byteCount;
    }

    /// <summary>
    /// Set of characters to encode into a span of bytes.
    /// </summary>
    public ReadOnlyMemory<char> Value { get; }

    /// <summary>
    /// Byte count of the <see cref="Value"/> to encode, according to the provided <see cref="Encoding"/>.
    /// </summary>
    public int ByteCount { get; }

    /// <summary>
    /// <see cref="System.Text.Encoding"/> used for encoding the <see cref="Value"/> into a span of bytes.
    /// </summary>
    public Encoding Encoding => _encoding ?? Encoding.UTF8;

    /// <summary>
    /// Attempts to create a new <see cref="EncodeableText"/> instance.
    /// </summary>
    /// <param name="encoding">
    /// <see cref="System.Text.Encoding"/> used for encoding the <paramref name="value"/> into a span of bytes.
    /// </param>
    /// <param name="value"><see cref="System.String"/> to encode into a span of bytes.</param>
    /// <returns>
    /// New <see cref="EncodeableText"/> instance wrapped in a <see cref="Result{T}"/>
    /// that specifies whether or not the operation was successful.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Result<EncodeableText> Create(Encoding encoding, string value)
    {
        return Create( encoding, value.AsMemory() );
    }

    /// <summary>
    /// Attempts to create a new <see cref="EncodeableText"/> instance.
    /// </summary>
    /// <param name="encoding">
    /// <see cref="System.Text.Encoding"/> used for encoding the <paramref name="value"/> into a span of bytes.
    /// </param>
    /// <param name="value">Set of characters to encode into a span of bytes.</param>
    /// <returns>
    /// New <see cref="EncodeableText"/> instance wrapped in a <see cref="Result{T}"/>
    /// that specifies whether or not the operation was successful.
    /// </returns>
    [Pure]
    public static Result<EncodeableText> Create(Encoding encoding, ReadOnlyMemory<char> value)
    {
        try
        {
            var byteCount = encoding.GetByteCount( value.Span );
            return new EncodeableText( encoding, value, byteCount );
        }
        catch ( Exception exc )
        {
            return Result.Error<EncodeableText>( exc );
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="EncodeableText"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[Encoding: {Encoding.EncodingName}, ByteCount: {ByteCount}] '{Value}'";
    }

    /// <summary>
    /// Attempts to encode the <see cref="Value"/> into the provided span of bytes.
    /// </summary>
    /// <param name="target">
    /// Span of bytes to encode the <see cref="Value"/> into.
    /// </param>
    /// <returns><see cref="Result"/> instance that specifies whether or not the operation was successful.</returns>
    public Result Encode(Span<byte> target)
    {
        try
        {
            Encoding.GetBytes( Value.Span, target );
            return Result.Valid;
        }
        catch ( Exception exc )
        {
            return Result.Error( exc );
        }
    }
}
