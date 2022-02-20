using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional
{
    public readonly struct Mutation<T> : IEquatable<Mutation<T>>
    {
        public static readonly Mutation<T> Empty = new Mutation<T>();

        public Mutation(T oldValue, T value)
        {
            OldValue = oldValue;
            Value = value;
        }

        public T OldValue { get; }
        public T Value { get; }
        public bool HasChanged => Generic<T>.AreNotEqual( OldValue, Value );

        [Pure]
        public override string ToString()
        {
            return $"{nameof( Mutation )}({Generic<T>.ToString( OldValue )} -> {Generic<T>.ToString( Value )})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default.Add( OldValue ).Add( Value ).Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is Mutation<T> m && Equals( m );
        }

        [Pure]
        public bool Equals(Mutation<T> other)
        {
            return Equality.Create( OldValue, other.OldValue ).Result &&
                Equality.Create( Value, other.Value ).Result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Mutation<T> Mutate(T newValue)
        {
            return new Mutation<T>( Value, newValue );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Mutation<T> Replace(T newValue)
        {
            return new Mutation<T>( OldValue, newValue );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Mutation<T> Revert()
        {
            return new Mutation<T>( OldValue, OldValue );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Mutation<T> Swap()
        {
            return new Mutation<T>( Value, OldValue );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Mutation<T2> Bind<T2>(Func<(T OldValue, T Value), Mutation<T2>> changed)
        {
            return HasChanged ? changed( (OldValue, Value) ) : Mutation<T2>.Empty;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Mutation<T2> Bind<T2>(Func<(T OldValue, T Value), Mutation<T2>> changed, Func<T, Mutation<T2>> unchanged)
        {
            return Match( changed, unchanged );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2 Match<T2>(Func<(T OldValue, T Value), T2> changed, Func<T, T2> unchanged)
        {
            return HasChanged ? changed( (OldValue, Value) ) : unchanged( Value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil Match(Action<(T OldValue, T Value)> changed, Action<T> unchanged)
        {
            if ( HasChanged )
                changed( (OldValue, Value) );
            else
                unchanged( Value );

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> IfChanged<T2>(Func<(T OldValue, T Value), T2?> changed)
            where T2 : notnull
        {
            return HasChanged ? changed( (OldValue, Value) ) : Maybe<T2>.None;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil IfChanged(Action<(T OldValue, T Value)> changed)
        {
            if ( HasChanged )
                changed( (OldValue, Value) );

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2? IfChangedOrDefault<T2>(Func<(T OldValue, T Value), T2> changed)
        {
            return HasChanged ? changed( (OldValue, Value) ) : default;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> IfUnchanged<T2>(Func<T, T2?> unchanged)
            where T2 : notnull
        {
            return HasChanged ? Maybe<T2>.None : unchanged( Value );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil IfUnchanged(Action<T> unchanged)
        {
            if ( ! HasChanged )
                unchanged( Value );

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2? IfUnchangedOrDefault<T2>(Func<T, T2> unchanged)
        {
            return HasChanged ? default : unchanged( Value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator T(Mutation<T> source)
        {
            return source.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Mutation<T>(Nil value)
        {
            return Empty;
        }

        [Pure]
        public static bool operator ==(Mutation<T> a, Mutation<T> b)
        {
            return a.Equals( b );
        }

        [Pure]
        public static bool operator !=(Mutation<T> a, Mutation<T> b)
        {
            return ! a.Equals( b );
        }
    }
}
