using System;
using System.Collections.Generic;

namespace LfrlSoft.NET.Core
{
    public readonly struct Bounds<T> : IEquatable<Bounds<T>>
        where T : IComparable<T>
    {
        public readonly T Min;
        public readonly T Max;

        public Bounds(T min, T max)
        {
            Assert.IsNotNull( min, EqualityComparer<T>.Default, nameof( min ) );
            Assert.IsNotNull( max, EqualityComparer<T>.Default, nameof( max ) );

            if ( min.CompareTo( max ) > 0 )
                throw new ArgumentException( $"{nameof( Min )} ({min}) cannot be greater than {nameof( Max )} ({max})" );

            Min = min;
            Max = max;
        }

        public override string ToString()
        {
            return $"{nameof( Bounds )}({Min} : {Max})";
        }

        public override int GetHashCode()
        {
            return Hash.Default
                .Add( Min )
                .Add( Max )
                .Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Bounds<T> b && Equals( b );
        }

        public bool Equals(Bounds<T> other)
        {
            return Equality.Create( Min, other.Min ).Result &&
                Equality.Create( Max, other.Max ).Result;
        }

        public Bounds<T> SetMin(T min)
        {
            return new Bounds<T>( min, Max );
        }

        public Bounds<T> SetMax(T max)
        {
            return new Bounds<T>( Min, max );
        }

        public bool Contains(T value)
        {
            return Min.CompareTo( value ) <= 0 && Max.CompareTo( value ) >= 0;
        }

        public bool Contains(Bounds<T> other)
        {
            return Min.CompareTo( other.Min ) <= 0 && Max.CompareTo( other.Max ) >= 0;
        }

        public bool Intersects(Bounds<T> other)
        {
            return Min.CompareTo( other.Max ) <= 0 && Max.CompareTo( other.Min ) >= 0;
        }

        public Bounds<T>? GetIntersection(Bounds<T> other)
        {
            var min = Min.CompareTo( other.Min ) > 0 ? Min : other.Min;
            var max = Max.CompareTo( other.Max ) < 0 ? Max : other.Max;

            if ( min.CompareTo( max ) > 0 )
                return null;

            return new Bounds<T>( min, max );
        }

        public T Clamp(T value)
        {
            if ( Min.CompareTo( value ) >= 0 )
                return Min;

            if ( Max.CompareTo( value ) <= 0 )
                return Max;

            return value;
        }

        public static bool operator ==(Bounds<T> a, Bounds<T> b)
        {
            return a.Equals( b );
        }

        public static bool operator !=(Bounds<T> a, Bounds<T> b)
        {
            return ! a.Equals( b );
        }
    }
}
