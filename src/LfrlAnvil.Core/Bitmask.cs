using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

public readonly struct Bitmask<T> : IEquatable<Bitmask<T>>, IComparable<Bitmask<T>>, IComparable, IReadOnlyCollection<T>
    where T : struct, IConvertible, IComparable
{
    public static readonly Bitmask<T> Empty = new Bitmask<T>();
    public static readonly Bitmask<T> All;

    public static readonly bool IsEnum = typeof( T ).IsEnum;
    public static readonly Type BaseType = IsEnum ? Enum.GetUnderlyingType( typeof( T ) ) : typeof( T );
    public static readonly int BitCount = Marshal.SizeOf( BaseType ) << 3;

    public static readonly Func<T, ulong> ToLongValue;
    public static readonly Func<ulong, T> FromLongValue;

    static Bitmask()
    {
        var toLongValueExpr = BuildToLongValueExpr();
        var fromLongValueExpr = BuildFromLongValueExpr();
        ToLongValue = toLongValueExpr.Compile();
        FromLongValue = fromLongValueExpr.Compile();

        TryAssertEnumType();

        All = new Bitmask<T>( FromLongValue( ~0UL ) );
        if ( ! IsEnum )
            return;

        var availableValues = All
            .Where( static v => Enum.IsDefined( typeof( T ), v ) )
            .Select( ToLongValue )
            .Aggregate( 0UL, static (p, c) => p | c );

        All = new Bitmask<T>( FromLongValue( availableValues ) );
    }

    public Bitmask(T value)
    {
        Value = value;
    }

    public T Value { get; }

    public int Count
    {
        get
        {
            var result = 0;
            foreach ( var _ in this )
                ++result;

            return result;
        }
    }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( Bitmask )}({Value})";
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Bitmask<T> b && Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode( Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Bitmask<T> other)
    {
        return EqualityComparer<T>.Default.Equals( Value, other.Value );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Bitmask<T> b ? CompareTo( b ) : 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(Bitmask<T> other)
    {
        return Comparer<T>.Default.Compare( Value, other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAny(T value)
    {
        var longValue = ToLongValue( value );
        return (ToLongValue( Value ) & longValue) != 0 || longValue == 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAny(Bitmask<T> other)
    {
        return ContainsAny( other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAll(T value)
    {
        var longValue = ToLongValue( value );
        return (ToLongValue( Value ) & longValue) == longValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsAll(Bitmask<T> other)
    {
        return ContainsAll( other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool ContainsBit(int bitIndex)
    {
        Ensure.IsInRange( bitIndex, 0, BitCount - 1, nameof( bitIndex ) );
        return ContainsAll( FromLongValue( 1UL << bitIndex ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Set(T value)
    {
        var result = ToLongValue( Value ) | ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Set(Bitmask<T> other)
    {
        return Set( other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> SetBit(int bitIndex)
    {
        Ensure.IsInRange( bitIndex, 0, BitCount - 1, nameof( bitIndex ) );
        return Set( FromLongValue( 1UL << bitIndex ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Unset(T value)
    {
        var result = ToLongValue( Value ) & ~ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Unset(Bitmask<T> other)
    {
        return Unset( other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> UnsetBit(int bitIndex)
    {
        Ensure.IsInRange( bitIndex, 0, BitCount - 1, nameof( bitIndex ) );
        return Unset( FromLongValue( 1UL << bitIndex ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Intersect(T value)
    {
        var result = ToLongValue( Value ) & ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Intersect(Bitmask<T> other)
    {
        return Intersect( other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Alternate(T value)
    {
        var result = ToLongValue( Value ) ^ ToLongValue( value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Alternate(Bitmask<T> other)
    {
        return Alternate( other.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Negate()
    {
        var result = ~ToLongValue( Value );
        return new Bitmask<T>( FromLongValue( result ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Sanitize()
    {
        return Intersect( All.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitmask<T> Clear()
    {
        return new Bitmask<T>( FromLongValue( 0 ) );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( Value );
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator T(Bitmask<T> b)
    {
        return b.Value;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Bitmask<T>(T v)
    {
        return new Bitmask<T>( v );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(Bitmask<T> a, Bitmask<T> b)
    {
        return a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(Bitmask<T> a, Bitmask<T> b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(Bitmask<T> a, Bitmask<T> b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator |(Bitmask<T> a, Bitmask<T> b)
    {
        return a.Set( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator |(Bitmask<T> a, T b)
    {
        return a.Set( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator |(T a, Bitmask<T> b)
    {
        return new Bitmask<T>( a ).Set( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator &(Bitmask<T> a, Bitmask<T> b)
    {
        return a.Intersect( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator &(Bitmask<T> a, T b)
    {
        return a.Intersect( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator &(T a, Bitmask<T> b)
    {
        return new Bitmask<T>( a ).Intersect( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ^(Bitmask<T> a, Bitmask<T> b)
    {
        return a.Alternate( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ^(Bitmask<T> a, T b)
    {
        return a.Alternate( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ^(T a, Bitmask<T> b)
    {
        return new Bitmask<T>( a ).Alternate( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> operator ~(Bitmask<T> a)
    {
        return a.Negate();
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly ulong _value;
        private int _index;

        internal Enumerator(T value)
        {
            _value = ToLongValue( value );
            _index = -1;
        }

        public T Current => FromLongValue( 1UL << _index );
        object IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            ++_index;
            while ( true )
            {
                if ( _index >= BitCount )
                {
                    _index = BitCount;
                    return false;
                }

                if ( ((_value >> _index) & 1UL) == 1UL )
                    break;

                ++_index;
            }

            return true;
        }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }

    private static void TryAssertEnumType()
    {
        if ( ! IsEnum )
            return;

        if ( ! typeof( T ).HasAttribute<FlagsAttribute>() )
            throw new BitmaskTypeInitializationException( typeof( T ), ExceptionResources.MissingEnumFlagsAttribute<T>() );

        var values = Enum.GetValues( typeof( T ) ).Cast<object>();
        if ( ! values.Any( static v => v.Equals( FromLongValue( 0 ) ) ) )
            throw new BitmaskTypeInitializationException( typeof( T ), ExceptionResources.MissingEnumZeroValueMember<T>() );
    }

    private static Expression<Func<T, ulong>> BuildToLongValueExpr()
    {
        try
        {
            var parameterExpr = Expression.Parameter( typeof( T ), "value" );

            if ( IsEnum )
            {
                var underlyingTypeConvertExpr = Expression.Convert( parameterExpr, BaseType );
                var enumUlongConvertExpr = Expression.Convert( underlyingTypeConvertExpr, typeof( ulong ) );

                return Expression.Lambda<Func<T, ulong>>( enumUlongConvertExpr, parameterExpr );
            }

            var ulongConvertExpr = Expression.Convert( parameterExpr, typeof( ulong ) );
            return Expression.Lambda<Func<T, ulong>>( ulongConvertExpr, parameterExpr );
        }
        catch ( Exception exc )
        {
            throw new BitmaskTypeInitializationException(
                typeof( T ),
                ExceptionResources.FailedToCreateConverter<T>( nameof( ToLongValue ) ),
                exc );
        }
    }

    private static Expression<Func<ulong, T>> BuildFromLongValueExpr()
    {
        try
        {
            var parameterExpr = Expression.Parameter( typeof( ulong ), "value" );

            if ( IsEnum )
            {
                var underlyingTypeConvertExpr = Expression.Convert( parameterExpr, BaseType );
                var enumTypeConvertExpr = Expression.Convert( underlyingTypeConvertExpr, typeof( T ) );

                return Expression.Lambda<Func<ulong, T>>( enumTypeConvertExpr, parameterExpr );
            }

            var typeConvertExpr = Expression.Convert( parameterExpr, typeof( T ) );
            return Expression.Lambda<Func<ulong, T>>( typeConvertExpr, parameterExpr );
        }
        catch ( Exception exc )
        {
            throw new BitmaskTypeInitializationException(
                typeof( T ),
                ExceptionResources.FailedToCreateConverter<T>( nameof( FromLongValue ) ),
                exc );
        }
    }
}
