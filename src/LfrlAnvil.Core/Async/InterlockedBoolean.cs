using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Async;

public struct InterlockedBoolean : IEquatable<InterlockedBoolean>, IComparable<InterlockedBoolean>, IComparable
{
    private int _value;

    public InterlockedBoolean(bool value)
    {
        _value = value ? 1 : 0;
    }

    public bool Value => Interlocked.Add( ref _value, 0 ).IsOdd();

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
        return obj is InterlockedBoolean b && Equals( b );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is InterlockedBoolean b ? CompareTo( b ) : 1;
    }

    [Pure]
    public bool Equals(InterlockedBoolean other)
    {
        return Value == other.Value;
    }

    [Pure]
    public int CompareTo(InterlockedBoolean other)
    {
        return Value.CompareTo( other.Value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool WriteTrue()
    {
        return Interlocked.Exchange( ref _value, 1 ).IsEven();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool WriteFalse()
    {
        return Interlocked.Exchange( ref _value, 0 ).IsOdd();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(bool value)
    {
        return value ? WriteTrue() : WriteFalse();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Toggle()
    {
        return Interlocked.Increment( ref _value ).IsOdd();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedBoolean a, InterlockedBoolean b)
    {
        return a.Equals( b );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(InterlockedBoolean a, InterlockedBoolean b)
    {
        return ! a.Equals( b );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(InterlockedBoolean a, InterlockedBoolean b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(InterlockedBoolean a, InterlockedBoolean b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(InterlockedBoolean a, InterlockedBoolean b)
    {
        return a.CompareTo( b ) > 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(InterlockedBoolean a, InterlockedBoolean b)
    {
        return a.CompareTo( b ) < 0;
    }
}
