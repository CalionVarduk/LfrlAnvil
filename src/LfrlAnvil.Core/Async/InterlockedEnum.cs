using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public struct InterlockedEnum<T> : IEquatable<InterlockedEnum<T>>, IComparable<InterlockedEnum<T>>, IComparable
    where T : struct, Enum
{
    private int _value;

    public InterlockedEnum(T value)
    {
        _value = ( int )( object )value;
    }

    public T Value => ( T )( object )Interlocked.Add( ref _value, 0 );

    [Pure]
    public override string ToString()
    {
        return Value.ToString();
    }

    [Pure]
    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode( Value );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is InterlockedEnum<T> b && Equals( b );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is InterlockedEnum<T> b ? CompareTo( b ) : 1;
    }

    [Pure]
    public bool Equals(InterlockedEnum<T> other)
    {
        return EqualityComparer<T>.Default.Equals( Value, other.Value );
    }

    [Pure]
    public int CompareTo(InterlockedEnum<T> other)
    {
        return Comparer<T>.Default.Compare( Value, other.Value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T Exchange(T value)
    {
        return ( T )( object )Interlocked.Exchange( ref _value, ( int )( object )value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T CompareExchange(T value, T comparand)
    {
        return ( T )( object )Interlocked.CompareExchange( ref _value, ( int )( object )value, ( int )( object )comparand );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(T value)
    {
        return ! EqualityComparer<T>.Default.Equals( Exchange( value ), value );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Write(T value, T expected)
    {
        return EqualityComparer<T>.Default.Equals( CompareExchange( value, expected ), expected );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.Equals( b );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return ! a.Equals( b );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) > 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(InterlockedEnum<T> a, InterlockedEnum<T> b)
    {
        return a.CompareTo( b ) < 0;
    }
}
