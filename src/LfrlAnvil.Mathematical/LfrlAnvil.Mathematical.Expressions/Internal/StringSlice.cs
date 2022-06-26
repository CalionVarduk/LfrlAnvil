using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Internal;

internal readonly struct StringSlice : IEquatable<StringSlice>, IEquatable<char>
{
    private StringSlice(string source, int startIndex, int length)
    {
        Source = source;
        StartIndex = startIndex;
        Length = length;

        Debug.Assert( Length >= 0, "Length >= 0" );
        Debug.Assert( StartIndex <= Source.Length, "StartIndex <= Source.Length" );
        Debug.Assert( EndIndex <= Source.Length, "EndIndex <= Source.Length" );
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
        return obj is StringSlice s && Equals( s );
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
}
