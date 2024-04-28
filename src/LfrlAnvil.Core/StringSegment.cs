using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil;

/// <summary>
/// A lightweight representation of a segment of <see cref="String"/>.
/// </summary>
public readonly struct StringSegment : IEquatable<StringSegment>, IComparable<StringSegment>, IComparable, IReadOnlyList<char>
{
    /// <summary>
    /// An empty <see cref="String"/> segment.
    /// </summary>
    public static readonly StringSegment Empty = new StringSegment( string.Empty );

    private readonly string? _source;

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance that contains the whole <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source string.</param>
    public StringSegment(string source)
    {
        _source = source;
        StartIndex = 0;
        Length = source.Length;
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance.
    /// </summary>
    /// <param name="source">Source string.</param>
    /// <param name="startIndex">Position of the first character to include in the segment.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="startIndex"/> is less than <b>0</b>.</exception>
    public StringSegment(string source, int startIndex)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );

        _source = source;
        StartIndex = Math.Min( startIndex, source.Length );
        Length = source.Length - StartIndex;
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance.
    /// </summary>
    /// <param name="source">Source string.</param>
    /// <param name="startIndex">Position of the first character to include in the segment.</param>
    /// <param name="length">Length of the segment.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="startIndex"/> is less than <b>0</b> or <paramref name="length"/> is less than <b>0</b>.
    /// </exception>
    public StringSegment(string source, int startIndex, int length)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );
        Ensure.IsGreaterThanOrEqualTo( length, 0 );

        _source = source;
        StartIndex = Math.Min( startIndex, source.Length );
        Length = Math.Min( length, source.Length - StartIndex );
    }

    /// <summary>
    /// Position of the first character from the <see cref="Source"/> string included in this segment.
    /// </summary>
    public int StartIndex { get; }

    /// <summary>
    /// Length of this segment.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Source string.
    /// </summary>
    public string Source => _source ?? string.Empty;

    /// <summary>
    /// Position of the character right after the last character from the <see cref="Source"/> string included in this segment.
    /// </summary>
    public int EndIndex => StartIndex + Length;

    /// <inheritdoc />
    public char this[int index] => Source[StartIndex + index];

    int IReadOnlyCollection<char>.Count => Length;

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance from the provided <see cref="ReadOnlyMemory{T}"/> <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source read-only memory.</param>
    /// <returns>New <see cref="StringSegment"/> instance</returns>
    /// <remarks>
    /// Creates a new <see cref="String"/> instance when underlying object of the <paramref name="source"/>
    /// is not of <see cref="String"/> type.
    /// </remarks>
    [Pure]
    public static StringSegment FromMemory(ReadOnlyMemory<char> source)
    {
        return MemoryMarshal.TryGetString( source, out var text, out var startIndex, out var length )
            ? new StringSegment( text, startIndex, length )
            : new StringSegment( source.ToString() );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="StringSegment"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return Source.Substring( StartIndex, Length );
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return string.GetHashCode( AsSpan() );
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is StringSegment s && Equals( s );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(StringSegment other)
    {
        return Equals( other, StringComparison.Ordinal );
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type using provided <paramref name="comparisonType"/>.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <param name="comparisonType"><see cref="StringComparison"/> to use.</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(StringSegment other, StringComparison comparisonType)
    {
        return AsSpan().Equals( other.AsSpan(), comparisonType );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is StringSegment s ? CompareTo( s ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(StringSegment other)
    {
        return CompareTo( other, StringComparison.CurrentCulture );
    }

    /// <summary>
    /// Compares the current instance with another object of the same type and returns an integer that indicates whether
    /// the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <param name="comparisonType"><see cref="StringComparison"/> to use.</param>
    /// <returns>A value that indicates the relative order of the objects being compared.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(StringSegment other, StringComparison comparisonType)
    {
        return AsSpan().CompareTo( other.AsSpan(), comparisonType );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance.
    /// </summary>
    /// <param name="startIndex">Position of the first character of this segment to include in the new segment.</param>
    /// <returns>New <see cref="StringSegment"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="startIndex"/> is less than <b>0</b>.</exception>
    [Pure]
    public StringSegment Slice(int startIndex)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );

        var endIndex = EndIndex;

        return startIndex >= endIndex
            ? new StringSegment( Source, endIndex, 0 )
            : new StringSegment( Source, StartIndex + startIndex, Length - startIndex );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance.
    /// </summary>
    /// <param name="startIndex">Position of the first character of this segment to include in the new segment.</param>
    /// <param name="length">Length of the new segment.</param>
    /// <returns>New <see cref="StringSegment"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="startIndex"/> is less than <b>0</b> or <paramref name="length"/> is less than <b>0</b>.
    /// </exception>
    [Pure]
    public StringSegment Slice(int startIndex, int length)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );
        Ensure.IsGreaterThanOrEqualTo( length, 0 );

        var endIndex = EndIndex;

        return startIndex >= endIndex
            ? new StringSegment( Source, endIndex, 0 )
            : new StringSegment( Source, StartIndex + startIndex, Math.Min( length, Length - startIndex ) );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance with the provided <see cref="StartIndex"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Position of the first character of this segment to include in the new segment.</param>
    /// <returns>New <see cref="StringSegment"/> instance with unchanged <see cref="Length"/>, if possible.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than <b>0</b>.</exception>
    [Pure]
    public StringSegment SetStartIndex(int value)
    {
        return new StringSegment( Source, value, Length );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance with the provided <see cref="EndIndex"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">
    /// Position of the character right after the last character from the <see cref="Source"/> string included in this segment.
    /// </param>
    /// <returns>New <see cref="StringSegment"/> instance with unchanged <see cref="Length"/>, if possible.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than <b>0</b>.</exception>
    [Pure]
    public StringSegment SetEndIndex(int value)
    {
        Ensure.IsGreaterThanOrEqualTo( value, 0 );
        return Offset( value - EndIndex );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance with the provided <see cref="Length"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Length of the new segment.</param>
    /// <returns>New <see cref="StringSegment"/> instance with unchanged <see cref="StartIndex"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than <b>0</b>.</exception>
    [Pure]
    public StringSegment SetLength(int value)
    {
        return new StringSegment( Source, StartIndex, value );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance by adding the specified <paramref name="offset"/>
    /// to the <see cref="StartIndex"/> of this instance.
    /// </summary>
    /// <param name="offset">Number of characters to offset the new segment by.</param>
    /// <returns>New <see cref="StringSegment"/> instance with unchanged <see cref="Length"/>, if possible.</returns>
    [Pure]
    public StringSegment Offset(int offset)
    {
        var startIndex = StartIndex + offset;
        var length = Length;

        if ( startIndex < 0 )
        {
            length = Math.Max( length + startIndex, 0 );
            startIndex = 0;
        }

        return new StringSegment( Source, startIndex, length );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance by subtracting the specified <paramref name="count"/>
    /// from the <see cref="StartIndex"/> of this instance and adding it to its <see cref="EndIndex"/> at the same time,
    /// increasing its <see cref="Length"/>.
    /// </summary>
    /// <param name="count">Number of characters to expand the new segment by.</param>
    /// <returns>New <see cref="StringSegment"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    public StringSegment Expand(int count)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0 );

        var startIndex = StartIndex - count;
        var length = Length + (count << 1);

        if ( startIndex < 0 )
        {
            length += startIndex;
            startIndex = 0;
        }

        return new StringSegment( Source, startIndex, length );
    }

    /// <summary>
    /// Creates a new <see cref="StringSegment"/> instance by adding the specified <paramref name="count"/>
    /// to the <see cref="StartIndex"/> of this instance and subtracting it from its <see cref="EndIndex"/> at the same time,
    /// decreasing its <see cref="Length"/>.
    /// </summary>
    /// <param name="count">Number of characters to shrink the new segment by.</param>
    /// <returns>New <see cref="StringSegment"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    public StringSegment Shrink(int count)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0 );

        var startIndex = StartIndex + count;
        var length = Length - (count << 1);

        if ( length < 0 )
        {
            startIndex += length >> 1;
            length = 0;
        }

        return new StringSegment( Source, startIndex, length );
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> instance from this segment.
    /// </summary>
    /// <returns>New <see cref="ReadOnlyMemory{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlyMemory<char> AsMemory()
    {
        return Source.AsMemory( StartIndex, Length );
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlySpan{T}"/> instance from this segment.
    /// </summary>
    /// <returns>New <see cref="ReadOnlySpan{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<char> AsSpan()
    {
        return Source.AsSpan( StartIndex, Length );
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<char> GetEnumerator()
    {
        return Source.Skip( StartIndex ).Take( Length ).GetEnumerator();
    }

    /// <summary>
    /// Converts a <see cref="String"/> <paramref name="s"/> to <see cref="StringSegment"/>.
    /// </summary>
    /// <param name="s">String to convert.</param>
    /// <returns>New <see cref="StringSegment"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator StringSegment(string s)
    {
        return new StringSegment( s );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(StringSegment a, StringSegment b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(StringSegment a, StringSegment b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >(StringSegment a, StringSegment b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <(StringSegment a, StringSegment b)
    {
        return a.CompareTo( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator >=(StringSegment a, StringSegment b)
    {
        return a.CompareTo( b ) >= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator <=(StringSegment a, StringSegment b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
