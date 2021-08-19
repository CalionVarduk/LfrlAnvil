using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Collections;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core.Functional
{
    public readonly struct Maybe<T> : IEquatable<Maybe<T>>, IReadOnlyCollection<T>
        where T : notnull
    {
        public static readonly Maybe<T> None = new Maybe<T>();

        public readonly bool HasValue;
        internal readonly T? Value;

        internal Maybe(T value)
        {
            HasValue = true;
            Value = value;
        }

        int IReadOnlyCollection<T>.Count => HasValue ? 1 : 0;

        [Pure]
        public override string ToString()
        {
            return HasValue ? $"{nameof( Value )}({Value})" : nameof( None );
        }

        [Pure]
        public override int GetHashCode()
        {
            return HasValue ? Value!.GetHashCode() : 0;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is Maybe<T> m && Equals( m );
        }

        [Pure]
        public bool Equals(Maybe<T> other)
        {
            if ( HasValue )
                return other.HasValue && Value!.Equals( other.Value );

            return ! other.HasValue;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T GetValue()
        {
            if ( HasValue )
                return Value!;

            throw new ArgumentNullException( nameof( Value ), $"{typeof( Maybe<T> ).FullName} instance doesn't contain a value" );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T? GetValueOrDefault()
        {
            return Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> Bind<T2>(Func<T, Maybe<T2>> some)
            where T2 : notnull
        {
            return HasValue ? some( Value! ) : Maybe<T2>.None;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> Bind<T2>(Func<T, Maybe<T2>> some, Func<Maybe<T2>> none)
            where T2 : notnull
        {
            return Match( some, none );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2 Match<T2>(Func<T, T2> some, Func<T2> none)
        {
            return HasValue ? some( Value! ) : none();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil Match(Action<T> some, Action none)
        {
            if ( HasValue )
                some( Value! );
            else
                none();

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> IfSome<T2>(Func<T, T2?> some)
            where T2 : notnull
        {
            return HasValue ? some( Value! ) : Maybe<T2>.None;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil IfSome(Action<T> some)
        {
            if ( HasValue )
                some( Value! );

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2? IfSomeOrDefault<T2>(Func<T, T2?> some)
        {
            return HasValue ? some( Value! ) : default;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> IfNone<T2>(Func<T2?> none)
            where T2 : notnull
        {
            return HasValue ? Maybe<T2>.None : none();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil IfNone(Action none)
        {
            if ( ! HasValue )
                none();

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2? IfNoneOrDefault<T2>(Func<T2?> none)
        {
            return HasValue ? default : none();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Maybe<T>(T? value)
        {
            return Generic<T>.IsNull( value ) ? None : new Maybe<T>( value! );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Maybe<T>(Nil none)
        {
            return None;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator T(Maybe<T> value)
        {
            return value.GetValue();
        }

        [Pure]
        public static bool operator ==(Maybe<T> a, Maybe<T> b)
        {
            return a.Equals( b );
        }

        [Pure]
        public static bool operator !=(Maybe<T> a, Maybe<T> b)
        {
            return ! a.Equals( b );
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (HasValue ? One.Create( Value! ) : Enumerable.Empty<T>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }
    }
}
