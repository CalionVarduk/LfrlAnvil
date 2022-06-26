using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

public readonly struct Either<T1, T2> : IEquatable<Either<T1, T2>>
{
    public static readonly Either<T1, T2> Empty = new Either<T1, T2>();

    internal readonly T1? First;
    internal readonly T2? Second;

    internal Either(T1 first)
    {
        HasFirst = true;
        First = first;
        Second = default;
    }

    internal Either(T2 second)
    {
        HasFirst = false;
        First = default;
        Second = second;
    }

    public bool HasFirst { get; }
    public bool HasSecond => ! HasFirst;

    [Pure]
    public override string ToString()
    {
        return HasFirst
            ? $"{nameof( First )}({Generic<T1>.ToString( First )})"
            : $"{nameof( Second )}({Generic<T2>.ToString( Second )})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HasFirst ? Hash.Default.Add( First ).Value : Hash.Default.Add( Second ).Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Either<T1, T2> e && Equals( e );
    }

    [Pure]
    public bool Equals(Either<T1, T2> other)
    {
        if ( HasFirst )
            return other.HasFirst && Equality.Create( First, other.First ).Result;

        return other.HasSecond && Equality.Create( Second, other.Second ).Result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T1 GetFirst()
    {
        if ( HasFirst )
            return First!;

        throw new ValueAccessException( Resources.MissingFirstEitherValue<T1, T2>(), nameof( First ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T1? GetFirstOrDefault()
    {
        return First;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 GetSecond()
    {
        if ( HasSecond )
            return Second!;

        throw new ValueAccessException( Resources.MissingSecondEitherValue<T1, T2>(), nameof( Second ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? GetSecondOrDefault()
    {
        return Second;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T2, T1> Swap()
    {
        return HasFirst ? new Either<T2, T1>( First! ) : new Either<T2, T1>( Second! );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T3, T2> Bind<T3>(Func<T1, Either<T3, T2>> first)
    {
        return HasFirst ? first( First! ) : new Either<T3, T2>( Second! );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T1, T3> BindSecond<T3>(Func<T2, Either<T1, T3>> second)
    {
        return HasSecond ? second( Second! ) : new Either<T1, T3>( First! );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T3, T4> Bind<T3, T4>(Func<T1, Either<T3, T4>> first, Func<T2, Either<T3, T4>> second)
    {
        return Match( first, second );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3 Match<T3>(Func<T1, T3> first, Func<T2, T3> second)
    {
        return HasFirst ? first( First! ) : second( Second! );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil Match(Action<T1> first, Action<T2> second)
    {
        if ( HasFirst )
            first( First! );
        else
            second( Second! );

        return Nil.Instance;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T3> IfFirst<T3>(Func<T1, T3?> first)
        where T3 : notnull
    {
        return HasFirst ? first( First! ) : Maybe<T3>.None;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfFirst(Action<T1> first)
    {
        if ( HasFirst )
            first( First! );

        return Nil.Instance;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3? IfFirstOrDefault<T3>(Func<T1, T3> first)
    {
        return HasFirst ? first( First! ) : default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T3> IfSecond<T3>(Func<T2, T3?> second)
        where T3 : notnull
    {
        return HasSecond ? second( Second! ) : Maybe<T3>.None;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfSecond(Action<T2> second)
    {
        if ( HasSecond )
            second( Second! );

        return Nil.Instance;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3? IfSecondOrDefault<T3>(Func<T2, T3> second)
    {
        return HasSecond ? second( Second! ) : default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(T1 first)
    {
        return new Either<T1, T2>( first );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(T2 second)
    {
        return new Either<T1, T2>( second );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(PartialEither<T1> part)
    {
        return new Either<T1, T2>( part.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(PartialEither<T2> part)
    {
        return new Either<T1, T2>( part.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(Nil value)
    {
        return Empty;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator T1(Either<T1, T2> value)
    {
        return value.GetFirst();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator T2(Either<T1, T2> value)
    {
        return value.GetSecond();
    }

    [Pure]
    public static bool operator ==(Either<T1, T2> a, Either<T1, T2> b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(Either<T1, T2> a, Either<T1, T2> b)
    {
        return ! a.Equals( b );
    }
}
