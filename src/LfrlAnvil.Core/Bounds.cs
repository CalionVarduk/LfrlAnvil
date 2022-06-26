using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil
{
    public readonly struct Bounds<T> : IEquatable<Bounds<T>>
        where T : IComparable<T>
    {
        public Bounds(T min, T max)
        {
            if ( min.CompareTo( max ) > 0 )
                throw new ArgumentException( ExceptionResources.MinCannotBeGreaterThanMax( min, max, nameof( Min ), nameof( Max ) ) );

            Min = min;
            Max = max;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Bounds((T Min, T Max) values)
        {
            Min = values.Min;
            Max = values.Max;
        }

        public T Min { get; }
        public T Max { get; }

        [Pure]
        public override string ToString()
        {
            return $"{nameof( Bounds )}({Min} : {Max})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default
                .Add( Min )
                .Add( Max )
                .Value;
        }

        [Pure]
        public override bool Equals(object? obj)
        {
            return obj is Bounds<T> b && Equals( b );
        }

        [Pure]
        public bool Equals(Bounds<T> other)
        {
            return Equality.Create( Min, other.Min ).Result &&
                Equality.Create( Max, other.Max ).Result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Bounds<T> SetMin(T min)
        {
            return new Bounds<T>( min, Max );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Bounds<T> SetMax(T max)
        {
            return new Bounds<T>( Min, max );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains(T value)
        {
            return Min.CompareTo( value ) <= 0 && Max.CompareTo( value ) >= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ContainsExclusively(T value)
        {
            return Min.CompareTo( value ) < 0 && Max.CompareTo( value ) > 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Contains(Bounds<T> other)
        {
            return Min.CompareTo( other.Min ) <= 0 && Max.CompareTo( other.Max ) >= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool ContainsExclusively(Bounds<T> other)
        {
            return Min.CompareTo( other.Min ) < 0 && Max.CompareTo( other.Max ) > 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Intersects(Bounds<T> other)
        {
            return Min.CompareTo( other.Max ) <= 0 && Max.CompareTo( other.Min ) >= 0;
        }

        [Pure]
        public Bounds<T>? GetIntersection(Bounds<T> other)
        {
            var min = Min.CompareTo( other.Min ) > 0 ? Min : other.Min;
            var max = Max.CompareTo( other.Max ) < 0 ? Max : other.Max;

            if ( min.CompareTo( max ) > 0 )
                return null;

            return new Bounds<T>( (min, max) );
        }

        [Pure]
        public Bounds<T>? MergeWith(Bounds<T> other)
        {
            if ( ! Intersects( other ) )
                return null;

            var min = Min.CompareTo( other.Min ) > 0 ? other.Min : Min;
            var max = Max.CompareTo( other.Max ) < 0 ? other.Max : Max;

            return new Bounds<T>( (min, max) );
        }

        [Pure]
        public Pair<Bounds<T>, Bounds<T>?> SplitAt(T value)
        {
            return ContainsExclusively( value )
                ? Pair.Create( new Bounds<T>( (Min, value) ), (Bounds<T>?)new Bounds<T>( (value, Max) ) )
                : Pair.Create( this, (Bounds<T>?)null );
        }

        [Pure]
        public Pair<Bounds<T>?, Bounds<T>?> Remove(Bounds<T> other)
        {
            var minComparisonResult = Min.CompareTo( other.Min );
            var maxComparisonResult = Max.CompareTo( other.Max );

            if ( minComparisonResult >= 0 && maxComparisonResult <= 0 )
                return Pair.Create( (Bounds<T>?)null, (Bounds<T>?)null );

            if ( minComparisonResult > 0 || maxComparisonResult < 0 )
                return Pair.Create( (Bounds<T>?)this, (Bounds<T>?)null );

            if ( minComparisonResult == 0 )
                return Pair.Create( (Bounds<T>?)new Bounds<T>( (other.Max, Max) ), (Bounds<T>?)null );

            if ( maxComparisonResult == 0 )
                return Pair.Create( (Bounds<T>?)new Bounds<T>( (Min, other.Min) ), (Bounds<T>?)null );

            return Pair.Create( (Bounds<T>?)new Bounds<T>( (Min, other.Min) ), (Bounds<T>?)new Bounds<T>( (other.Max, Max) ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T Clamp(T value)
        {
            if ( Min.CompareTo( value ) >= 0 )
                return Min;

            if ( Max.CompareTo( value ) <= 0 )
                return Max;

            return value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(Bounds<T> a, Bounds<T> b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(Bounds<T> a, Bounds<T> b)
        {
            return ! a.Equals( b );
        }
    }
}
