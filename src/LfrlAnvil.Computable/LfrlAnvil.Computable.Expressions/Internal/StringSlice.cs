using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil.Computable.Expressions.Internal;

// TODO: extract to Core
internal readonly struct StringSlice : IEquatable<StringSlice>, IEquatable<char>, IEnumerable<char>
{
    private StringSlice(string source, int startIndex, int length)
    {
        Source = source;
        StartIndex = startIndex;
        Length = length;

        Assume.IsGreaterThanOrEqualTo( Length, 0, nameof( Length ) );
        Assume.IsLessThanOrEqualTo( StartIndex, Source.Length, nameof( StartIndex ) );
        Assume.IsLessThanOrEqualTo( EndIndex, Source.Length, nameof( EndIndex ) );
    }

    internal string Source { get; }
    internal int StartIndex { get; }
    internal int Length { get; }
    internal int EndIndex => StartIndex + Length;
    internal char this[int index] => Source[StartIndex + index];

    [Pure]
    public override string ToString()
    {
        return Source.Substring( StartIndex, Length );
    }

    [Pure]
    public override int GetHashCode()
    {
        var hash = Hash.Default.Add( Length );

        var endIndex = EndIndex;
        for ( var i = StartIndex; i < endIndex; ++i )
            hash = hash.Add( Source[i] );

        return hash.Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        if ( obj is StringSlice s )
            return Equals( s );

        return obj is char c && Equals( c );
    }

    [Pure]
    public bool Equals(StringSlice other)
    {
        if ( Length != other.Length )
            return false;

        var endIndex = EndIndex;
        for ( int i = StartIndex, j = other.StartIndex; i < endIndex; ++i, ++j )
        {
            if ( Source[i] != other.Source[j] )
                return false;
        }

        return true;
    }

    [Pure]
    public bool EqualsIgnoreCase(StringSlice other)
    {
        if ( Length != other.Length )
            return false;

        var endIndex = EndIndex;
        for ( int i = StartIndex, j = other.StartIndex; i < endIndex; ++i, ++j )
        {
            if ( char.ToLowerInvariant( Source[i] ) != char.ToLowerInvariant( other.Source[j] ) )
                return false;
        }

        return true;
    }

    [Pure]
    public bool Equals(char other)
    {
        return Length == 1 && Source[StartIndex] == other;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal StringSlice Slice(int startIndex)
    {
        return new StringSlice( Source, StartIndex + startIndex, Length - startIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal StringSlice Slice(int startIndex, int length)
    {
        return new StringSlice( Source, StartIndex + startIndex, length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal StringSlice Expand(int count)
    {
        var startIndex = Math.Max( StartIndex - count, 0 );
        var endIndex = Math.Min( EndIndex + count, Source.Length );
        return new StringSlice( Source, startIndex, endIndex - startIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReadOnlyMemory<char> AsMemory()
    {
        return Source.AsMemory( StartIndex, Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReadOnlySpan<char> AsSpan()
    {
        return Source.AsSpan( StartIndex, Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringSlice Create(string source)
    {
        return Create( source, 0, source.Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringSlice Create(string source, int startIndex, int length)
    {
        return new StringSlice( source, startIndex, length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringSlice Create(ReadOnlyMemory<char> source)
    {
        return MemoryMarshal.TryGetString( source, out var text, out var startIndex, out var length )
            ? Create( text, startIndex, length )
            : Create( source.ToString() );
    }

    [Pure]
    public IEnumerator<char> GetEnumerator()
    {
        return ToString().GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
