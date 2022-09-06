using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil;

public readonly struct StringSlice : IEquatable<StringSlice>, IComparable<StringSlice>, IComparable, IReadOnlyList<char>
{
    public static readonly StringSlice Empty = new StringSlice( string.Empty );

    private readonly string? _source;

    public StringSlice(string source)
    {
        _source = source;
        StartIndex = 0;
        Length = source.Length;
    }

    public StringSlice(string source, int startIndex)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0, nameof( startIndex ) );

        _source = source;
        StartIndex = Math.Min( startIndex, source.Length );
        Length = source.Length - StartIndex;
    }

    public StringSlice(string source, int startIndex, int length)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0, nameof( startIndex ) );
        Ensure.IsGreaterThanOrEqualTo( length, 0, nameof( length ) );

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
    public static StringSlice FromMemory(ReadOnlyMemory<char> source)
    {
        return MemoryMarshal.TryGetString( source, out var text, out var startIndex, out var length )
            ? new StringSlice( text, startIndex, length )
            : new StringSlice( source.ToString() );
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
        return obj is StringSlice s && Equals( s );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(StringSlice other)
    {
        return Equals( other, StringComparison.Ordinal );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(StringSlice other, StringComparison comparisonType)
    {
        return AsSpan().Equals( other.AsSpan(), comparisonType );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is StringSlice s ? CompareTo( s ) : 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(StringSlice other)
    {
        return CompareTo( other, StringComparison.CurrentCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(StringSlice other, StringComparison comparisonType)
    {
        return AsSpan().CompareTo( other.AsSpan(), comparisonType );
    }

    [Pure]
    public StringSlice Slice(int startIndex)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0, nameof( startIndex ) );

        var endIndex = EndIndex;

        return startIndex >= endIndex
            ? new StringSlice( Source, endIndex, 0 )
            : new StringSlice( Source, StartIndex + startIndex, Length - startIndex );
    }

    [Pure]
    public StringSlice Slice(int startIndex, int length)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0, nameof( startIndex ) );
        Ensure.IsGreaterThanOrEqualTo( length, 0, nameof( length ) );

        var endIndex = EndIndex;

        return startIndex >= endIndex
            ? new StringSlice( Source, endIndex, 0 )
            : new StringSlice( Source, StartIndex + startIndex, Math.Min( length, Length - startIndex ) );
    }

    [Pure]
    public StringSlice SetStartIndex(int value)
    {
        return new StringSlice( Source, value, Length );
    }

    [Pure]
    public StringSlice SetEndIndex(int value)
    {
        Ensure.IsGreaterThanOrEqualTo( value, 0, nameof( value ) );
        return Offset( value - EndIndex );
    }

    [Pure]
    public StringSlice SetLength(int value)
    {
        return new StringSlice( Source, StartIndex, value );
    }

    [Pure]
    public StringSlice Offset(int offset)
    {
        var startIndex = StartIndex + offset;
        var length = Length;

        if ( startIndex < 0 )
        {
            length = Math.Max( length + startIndex, 0 );
            startIndex = 0;
        }

        return new StringSlice( Source, startIndex, length );
    }

    [Pure]
    public StringSlice Expand(int count)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0, nameof( count ) );

        var startIndex = StartIndex - count;
        var length = Length + (count << 1);

        if ( startIndex < 0 )
        {
            length += startIndex;
            startIndex = 0;
        }

        return new StringSlice( Source, startIndex, length );
    }

    [Pure]
    public StringSlice Shrink(int count)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0, nameof( count ) );

        var startIndex = StartIndex + count;
        var length = Length - (count << 1);

        if ( length < 0 )
        {
            startIndex += length >> 1;
            length = 0;
        }

        return new StringSlice( Source, startIndex, length );
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
    public static bool operator ==(StringSlice a, StringSlice b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(StringSlice a, StringSlice b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    public static bool operator >(StringSlice a, StringSlice b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    public static bool operator <(StringSlice a, StringSlice b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    public static bool operator >=(StringSlice a, StringSlice b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    public static bool operator <=(StringSlice a, StringSlice b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
