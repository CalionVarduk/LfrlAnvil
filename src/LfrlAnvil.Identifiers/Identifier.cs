using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Identifiers;

public readonly struct Identifier : IEquatable<Identifier>, IComparable<Identifier>, IComparable
{
    public const ulong MaxHighValue = (1UL << 48) - 1;

    public readonly ulong Value;

    public Identifier(ulong value)
    {
        Value = value;
    }

    public Identifier(ulong high, ushort low)
        : this( (high << 16) | low ) { }

    public ulong High => Value >> 16;
    public ushort Low => (ushort)Value;

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Identifier )}({Value})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Identifier id && Equals( id );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Identifier other)
    {
        return Value.Equals( other.Value );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Identifier id ? CompareTo( id ) : 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(Identifier other)
    {
        return Value.CompareTo( other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator ulong(Identifier id)
    {
        return id.Value;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Identifier a, Identifier b)
    {
        return a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(Identifier a, Identifier b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(Identifier a, Identifier b)
    {
        return a.CompareTo( b ) >= 0;
    }
}
