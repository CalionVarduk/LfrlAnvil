using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public struct InterlockedInt32 : IEquatable<InterlockedInt32>, IComparable<InterlockedInt32>, IComparable
{
    private int _value;

    public InterlockedInt32(int value)
    {
        _value = value;
    }

    public int Value => Interlocked.Add( ref _value, 0 );

    [Pure]
    public override string ToString()
    {
        return Value.ToString();
    }

    [Pure]
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is InterlockedInt32 b && Equals( b );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is InterlockedInt32 b ? CompareTo( b ) : 1;
    }

    [Pure]
    public bool Equals(InterlockedInt32 other)
    {
        return Value == other.Value;
    }

    [Pure]
    public int CompareTo(InterlockedInt32 other)
    {
        return Value.CompareTo( other.Value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Exchange(int value)
    {
        return Interlocked.Exchange( ref _value, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareExchange(int value, int comparand)
    {
        return Interlocked.CompareExchange( ref _value, value, comparand );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(int value)
    {
        return Exchange( value ) != value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(int value, int expected)
    {
        return CompareExchange( value, expected ) == expected;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Increment()
    {
        return Interlocked.Increment( ref _value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Decrement()
    {
        return Interlocked.Decrement( ref _value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Add(int value)
    {
        return Interlocked.Add( ref _value, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Subtract(int value)
    {
        return Add( unchecked( -value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int And(int value)
    {
        return Interlocked.And( ref _value, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int Or(int value)
    {
        return Interlocked.Or( ref _value, value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.Equals( b );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(InterlockedInt32 a, InterlockedInt32 b)
    {
        return ! a.Equals( b );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) > 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(InterlockedInt32 a, InterlockedInt32 b)
    {
        return a.CompareTo( b ) < 0;
    }
}
