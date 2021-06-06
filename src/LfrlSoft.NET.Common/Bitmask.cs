using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace LfrlSoft.NET.Common
{
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

        public readonly T Value;
        public int Count => this.Count();

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
                .Where( v => Enum.IsDefined( typeof( T ), v ) )
                .Select( ToLongValue )
                .Aggregate( 0UL, (p, c) => p | c );

            All = new Bitmask<T>( FromLongValue( availableValues ) );
        }

        public Bitmask(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{nameof( Bitmask )}({Value})";
        }

        public override bool Equals(object obj)
        {
            return obj is Bitmask<T> b && Equals( b );
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(Bitmask<T> other)
        {
            return Value.Equals( other.Value );
        }

        public int CompareTo(object obj)
        {
            return obj is Bitmask<T> b ? CompareTo( b ) : 1;
        }

        public int CompareTo(Bitmask<T> other)
        {
            return Value.CompareTo( other.Value );
        }

        public bool ContainsAny(T value)
        {
            var longValue = ToLongValue( value );
            return (ToLongValue( Value ) & longValue) != 0 || longValue == 0;
        }

        public bool ContainsAny(Bitmask<T> other)
        {
            return ContainsAny( other.Value );
        }

        public bool ContainsAll(T value)
        {
            var longValue = ToLongValue( value );
            return (ToLongValue( Value ) & longValue) == longValue;
        }

        public bool ContainsAll(Bitmask<T> other)
        {
            return ContainsAll( other.Value );
        }

        public bool ContainsBit(int bitIndex)
        {
            Assert.IsBetween( bitIndex, 0, BitCount - 1, nameof( bitIndex ) );
            return ContainsAll( FromLongValue( 1UL << bitIndex ) );
        }

        public Bitmask<T> Set(T value)
        {
            var result = ToLongValue( Value ) | ToLongValue( value );
            return new Bitmask<T>( FromLongValue( result ) );
        }

        public Bitmask<T> Set(Bitmask<T> other)
        {
            return Set( other.Value );
        }

        public Bitmask<T> SetBit(int bitIndex)
        {
            Assert.IsBetween( bitIndex, 0, BitCount - 1, nameof( bitIndex ) );
            return Set( FromLongValue( 1UL << bitIndex ) );
        }

        public Bitmask<T> Unset(T value)
        {
            var result = ToLongValue( Value ) & ~ToLongValue( value );
            return new Bitmask<T>( FromLongValue( result ) );
        }

        public Bitmask<T> Unset(Bitmask<T> other)
        {
            return Unset( other.Value );
        }

        public Bitmask<T> UnsetBit(int bitIndex)
        {
            Assert.IsBetween( bitIndex, 0, BitCount - 1, nameof( bitIndex ) );
            return Unset( FromLongValue( 1UL << bitIndex ) );
        }

        public Bitmask<T> Intersect(T value)
        {
            var result = ToLongValue( Value ) & ToLongValue( value );
            return new Bitmask<T>( FromLongValue( result ) );
        }

        public Bitmask<T> Intersect(Bitmask<T> other)
        {
            return Intersect( other.Value );
        }

        public Bitmask<T> Alternate(T value)
        {
            var result = ToLongValue( Value ) ^ ToLongValue( value );
            return new Bitmask<T>( FromLongValue( result ) );
        }

        public Bitmask<T> Alternate(Bitmask<T> other)
        {
            return Alternate( other.Value );
        }

        public Bitmask<T> Negate()
        {
            var result = ~ToLongValue( Value );
            return new Bitmask<T>( FromLongValue( result ) );
        }

        public Bitmask<T> Sanitize()
        {
            return Intersect( All.Value );
        }

        public Bitmask<T> Clear()
        {
            return new Bitmask<T>( FromLongValue( 0 ) );
        }

        public IEnumerator<T> GetEnumerator()
        {
            var longValue = ToLongValue( Value );

            return Enumerable.Range( 0, BitCount )
                .Where( i => ((longValue >> i) & 1) == 1 )
                .Select( i => FromLongValue( 1UL << i ) )
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator T(Bitmask<T> b)
        {
            return b.Value;
        }

        public static implicit operator Bitmask<T>(T v)
        {
            return new Bitmask<T>( v );
        }

        public static bool operator ==(Bitmask<T> a, Bitmask<T> b)
        {
            return a.Equals( b );
        }

        public static bool operator !=(Bitmask<T> a, Bitmask<T> b)
        {
            return ! a.Equals( b );
        }

        public static bool operator >(Bitmask<T> a, Bitmask<T> b)
        {
            return a.CompareTo( b ) > 0;
        }

        public static bool operator <=(Bitmask<T> a, Bitmask<T> b)
        {
            return a.CompareTo( b ) <= 0;
        }

        public static bool operator <(Bitmask<T> a, Bitmask<T> b)
        {
            return a.CompareTo( b ) < 0;
        }

        public static bool operator >=(Bitmask<T> a, Bitmask<T> b)
        {
            return a.CompareTo( b ) >= 0;
        }

        public static Bitmask<T> operator |(Bitmask<T> a, Bitmask<T> b)
        {
            return a.Set( b );
        }

        public static Bitmask<T> operator |(Bitmask<T> a, T b)
        {
            return a.Set( b );
        }

        public static Bitmask<T> operator |(T a, Bitmask<T> b)
        {
            return new Bitmask<T>( a ).Set( b );
        }

        public static Bitmask<T> operator &(Bitmask<T> a, Bitmask<T> b)
        {
            return a.Intersect( b );
        }

        public static Bitmask<T> operator &(Bitmask<T> a, T b)
        {
            return a.Intersect( b );
        }

        public static Bitmask<T> operator &(T a, Bitmask<T> b)
        {
            return new Bitmask<T>( a ).Intersect( b );
        }

        public static Bitmask<T> operator ^(Bitmask<T> a, Bitmask<T> b)
        {
            return a.Alternate( b );
        }

        public static Bitmask<T> operator ^(Bitmask<T> a, T b)
        {
            return a.Alternate( b );
        }

        public static Bitmask<T> operator ^(T a, Bitmask<T> b)
        {
            return new Bitmask<T>( a ).Alternate( b );
        }

        public static Bitmask<T> operator ~(Bitmask<T> a)
        {
            return a.Negate();
        }

        private static void TryAssertEnumType()
        {
            if ( ! IsEnum )
                return;

            if ( ! Attribute.IsDefined( typeof( T ), typeof( FlagsAttribute ) ) )
                throw new InvalidOperationException( $"Enum type {typeof( T ).FullName} doesn't have the Flags attribute" );

            var values = Enum.GetValues( typeof( T ) ).Cast<object>();
            if ( ! values.Any( v => v.Equals( FromLongValue( 0 ) ) ) )
                throw new InvalidOperationException( $"Enum type {typeof( T ).FullName} doesn't have the 0-value member" );
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
                throw new InvalidOperationException( $"Failed to create ToLongValue converter for type {typeof( T ).FullName}", exc );
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
                throw new InvalidOperationException( $"Failed to create FromLongValue converter for type {typeof( T ).FullName}", exc );
            }
        }
    }
}
