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
    public readonly struct Unsafe<T> : IUnsafe, IEquatable<Unsafe<T>>, IReadOnlyCollection<T>
    {
        public static readonly Unsafe<T> Empty = new Unsafe<T>();

        internal readonly T? Value;
        internal readonly Exception? Error;

        internal Unsafe(T value)
        {
            Value = value;
            Error = default;
        }

        internal Unsafe(Exception error)
        {
            Value = default;
            Error = error;
        }

        public bool HasError => Error is not null;
        public bool IsOk => ! HasError;
        public int Count => HasError ? 0 : 1;

        [Pure]
        public override string ToString()
        {
            return HasError
                ? $"{nameof( Error )}({Error!.GetType().Name})"
                : $"{nameof( Value )}({Generic<T>.ToString( Value )})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return HasError ? Hash.Default.Add( Error ).Value : Hash.Default.Add( Value ).Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is Unsafe<T> u && Equals( u );
        }

        [Pure]
        public bool Equals(Unsafe<T> other)
        {
            if ( HasError )
                return other.HasError && Error!.Equals( other.Error );

            return other.IsOk && Equality.Create( Value, other.Value ).Result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T GetValue()
        {
            if ( IsOk )
                return Value!;

            throw new ArgumentNullException(
                nameof( Value ),
                $"{typeof( Unsafe<T> ).FullName} instance doesn't contain a value" );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T? GetValueOrDefault()
        {
            return Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Exception GetError()
        {
            if ( HasError )
                return Error!;

            throw new ArgumentNullException(
                nameof( Error ),
                $"{typeof( Unsafe<T> ).FullName} instance doesn't contain an error" );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Exception? GetErrorOrDefault()
        {
            return Error;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Unsafe<T2> Bind<T2>(Func<T, Unsafe<T2>> ok)
        {
            return IsOk ? ok( Value! ) : new Unsafe<T2>( Error! );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Unsafe<T2> Bind<T2>(Func<T, Unsafe<T2>> ok, Func<Exception, Unsafe<T2>> error)
        {
            return Match( ok, error );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2 Match<T2>(Func<T, T2> ok, Func<Exception, T2> error)
        {
            return IsOk ? ok( Value! ) : error( Error! );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil Match(Action<T> ok, Action<Exception> error)
        {
            if ( IsOk )
                ok( Value! );
            else
                error( Error! );

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> IfOk<T2>(Func<T, T2?> ok)
            where T2 : notnull
        {
            return IsOk ? ok( Value! ) : Maybe<T2>.None;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil IfOk(Action<T> ok)
        {
            if ( IsOk )
                ok( Value! );

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2? IfOkOrDefault<T2>(Func<T, T2> ok)
        {
            return IsOk ? ok( Value! ) : default;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Maybe<T2> IfError<T2>(Func<Exception, T2?> error)
            where T2 : notnull
        {
            return HasError ? error( Error! ) : Maybe<T2>.None;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Nil IfError(Action<Exception> error)
        {
            if ( HasError )
                error( Error! );

            return Nil.Instance;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public T2? IfErrorOrDefault<T2>(Func<Exception, T2> error)
        {
            return HasError ? error( Error! ) : default;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Unsafe<T>(T value)
        {
            return new Unsafe<T>( value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Unsafe<T>(Exception error)
        {
            return new Unsafe<T>( error );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Either<T, Exception>(Unsafe<T> value)
        {
            return value.HasError ? new Either<T, Exception>( value.Error! ) : new Either<T, Exception>( value.Value! );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Unsafe<T>(Either<T, Exception> value)
        {
            return value.HasFirst ? new Unsafe<T>( value.First! ) : new Unsafe<T>( value.Second! );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Unsafe<T>(Nil value)
        {
            return Empty;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator T(Unsafe<T> value)
        {
            return value.GetValue();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator Exception(Unsafe<T> value)
        {
            return value.GetError();
        }

        [Pure]
        public static bool operator ==(Unsafe<T> a, Unsafe<T> b)
        {
            return a.Equals( b );
        }

        [Pure]
        public static bool operator !=(Unsafe<T> a, Unsafe<T> b)
        {
            return ! a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        object IUnsafe.GetValue()
        {
            return GetValue()!;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        object? IUnsafe.GetValueOrDefault()
        {
            return GetValueOrDefault();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        Exception IUnsafe.GetError()
        {
            return GetError();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        Exception? IUnsafe.GetErrorOrDefault()
        {
            return GetErrorOrDefault();
        }

        [Pure]
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return (IsOk ? One.Create( Value! ) : Enumerable.Empty<T>()).GetEnumerator();
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>) this).GetEnumerator();
        }
    }
}
