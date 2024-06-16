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
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="String"/> extension methods.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance from the given string and <paramref name="startIndex"/>.
    /// </summary>
    /// <param name="source">Source string.</param>
    /// <param name="startIndex">Index of the first character that should be included in the segment.</param>
    /// <returns>New <see cref="StringSegment"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="startIndex"/> is less than <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSegment AsSegment(this string source, int startIndex)
    {
        return new StringSegment( source, startIndex );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance from the given string,
    /// <paramref name="startIndex"/> and <paramref name="length"/>.
    /// </summary>
    /// <param name="source">Source string.</param>
    /// <param name="startIndex">Index of the first character that should be included in the segment.</param>
    /// <param name="length">Length of the segment.</param>
    /// <returns>New <see cref="StringSegment"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="startIndex"/> is less than <b>0</b> or when <paramref name="length"/> is less than <b>0</b>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringSegment AsSegment(this string source, int startIndex, int length)
    {
        return new StringSegment( source, startIndex, length );
    }

    /// <summary>
    /// Reverses the given string.
    /// </summary>
    /// <param name="source">Source string.</param>
    /// <returns>New <see cref="String"/> instance that represents reversed <paramref name="source"/> string.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string Reverse(this string source)
    {
        const int stackallocThreshold = 64;
        if ( source.Length <= 1 )
            return source;

        var buffer = source.Length > stackallocThreshold ? new char[source.Length] : stackalloc char[source.Length];
        var sourceSpan = source.AsSpan();

        do
        {
            var charLength = StringInfo.GetNextTextElementLength( sourceSpan );
            sourceSpan.Slice( 0, charLength ).CopyTo( buffer.Slice( sourceSpan.Length - charLength ) );
            sourceSpan = sourceSpan.Slice( charLength );
        }
        while ( sourceSpan.Length > 0 );

        return new string( buffer );
    }
}
