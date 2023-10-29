using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil;

public readonly struct StringSegment : IEquatable<StringSegment>, IComparable<StringSegment>, IComparable, IReadOnlyList<char>
{
    public static readonly StringSegment Empty = new StringSegment( string.Empty );

    private readonly string? _source;

    public StringSegment(string source)
    {
        _source = source;
        StartIndex = 0;
        Length = source.Length;
    }

    public StringSegment(string source, int startIndex)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );

        _source = source;
        StartIndex = Math.Min( startIndex, source.Length );
        Length = source.Length - StartIndex;
    }

    public StringSegment(string source, int startIndex, int length)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );
        Ensure.IsGreaterThanOrEqualTo( length, 0 );

        _source = source;
        StartIndex = Math.Min( startIndex, source.Length );
        Length = Math.Min( length, source.Length - StartIndex );
    }

    public int StartIndex { get; }
    public int Length { get; }
    public string Source => _source ?? string.Empty;
    public int EndIndex => StartIndex + Length;
    public char this[int index] => Source[StartIndex + index];

    int IReadOnlyCollection<char>.Count => Length;

    [Pure]
    public static StringSegment FromMemory(ReadOnlyMemory<char> source)
    {
        return MemoryMarshal.TryGetString( source, out var text, out var startIndex, out var length )
            ? new StringSegment( text, startIndex, length )
            : new StringSegment( source.ToString() );
    }

    [Pure]
    public override string ToString()
    {
        return Source.Substring( StartIndex, Length );
    }

    [Pure]
    public override int GetHashCode()
    {
        return string.GetHashCode( AsSpan() );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is StringSegment s && Equals( s );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(StringSegment other)
    {
        return Equals( other, StringComparison.Ordinal );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(StringSegment other, StringComparison comparisonType)
    {
        return AsSpan().Equals( other.AsSpan(), comparisonType );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is StringSegment s ? CompareTo( s ) : 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(StringSegment other)
    {
        return CompareTo( other, StringComparison.CurrentCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(StringSegment other, StringComparison comparisonType)
    {
        return AsSpan().CompareTo( other.AsSpan(), comparisonType );
    }

    [Pure]
    public StringSegment Slice(int startIndex)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );

        var endIndex = EndIndex;

        return startIndex >= endIndex
            ? new StringSegment( Source, endIndex, 0 )
            : new StringSegment( Source, StartIndex + startIndex, Length - startIndex );
    }

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

    [Pure]
    public StringSegment SetStartIndex(int value)
    {
        return new StringSegment( Source, value, Length );
    }

    [Pure]
    public StringSegment SetEndIndex(int value)
    {
        Ensure.IsGreaterThanOrEqualTo( value, 0 );
        return Offset( value - EndIndex );
    }

    [Pure]
    public StringSegment SetLength(int value)
    {
        return new StringSegment( Source, StartIndex, value );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlyMemory<char> AsMemory()
    {
        return Source.AsMemory( StartIndex, Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<char> AsSpan()
    {
        return Source.AsSpan( StartIndex, Length );
    }

    [Pure]
    public IEnumerator<char> GetEnumerator()
    {
        return Source.Skip( StartIndex ).Take( Length ).GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator StringSegment(string s)
    {
        return new StringSegment( s );
    }

    [Pure]
    public static bool operator ==(StringSegment a, StringSegment b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(StringSegment a, StringSegment b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    public static bool operator >(StringSegment a, StringSegment b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    public static bool operator <(StringSegment a, StringSegment b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    public static bool operator >=(StringSegment a, StringSegment b)
    {
        return a.CompareTo( b ) >= 0;
    }

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
