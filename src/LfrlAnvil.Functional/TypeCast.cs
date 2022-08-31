using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

public readonly struct TypeCast<TSource, TDestination> : ITypeCast<TDestination>, IEquatable<TypeCast<TSource, TDestination>>
{
    public static readonly TypeCast<TSource, TDestination> Empty = new TypeCast<TSource, TDestination>();

    internal readonly TDestination? Result;

    internal TypeCast(TSource source)
    {
        Source = source;
        if ( source is TDestination d )
        {
            Result = d;
            IsValid = true;
        }
        else
        {
            Result = default;
            IsValid = false;
        }
    }

    public TSource Source { get; }

    [MemberNotNullWhen( true, nameof( Result ) )]
    public bool IsValid { get; }

    [MemberNotNullWhen( false, nameof( Result ) )]
    public bool IsInvalid => ! IsValid;

    object? ITypeCast<TDestination>.Source => Source;
    int IReadOnlyCollection<TDestination>.Count => IsValid ? 1 : 0;

    [Pure]
    public override string ToString()
    {
        return IsValid
            ? $"{nameof( TypeCast )}<{typeof( TSource ).FullName} -> {typeof( TDestination ).FullName}>({Result})"
            : $"Invalid{nameof( TypeCast )}<{typeof( TSource ).FullName} -> {typeof( TDestination ).FullName}>({Generic<TSource>.ToString( Source )})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( Source ).Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is TypeCast<TSource, TDestination> c && Equals( c );
    }

    [Pure]
    public bool Equals(TypeCast<TSource, TDestination> other)
    {
        if ( IsValid )
            return other.IsValid && Result.Equals( other.Result );

        return ! other.IsValid && Equality.Create( Source, other.Source ).Result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TDestination GetResult()
    {
        if ( IsValid )
            return Result;

        throw new ValueAccessException( Resources.MissingTypeCastResult<TSource, TDestination>(), nameof( Result ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TDestination? GetResultOrDefault()
    {
        return Result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TDestination GetResultOrDefault(TDestination defaultValue)
    {
        return IsValid ? Result : defaultValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeCast<TDestination, T> Bind<T>(Func<TDestination, TypeCast<TDestination, T>> valid)
    {
        return IsValid ? valid( Result ) : TypeCast<TDestination, T>.Empty;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeCast<TDestination, T> Bind<T>(
        Func<TDestination, TypeCast<TDestination, T>> valid,
        Func<TSource, TypeCast<TDestination, T>> invalid)
    {
        return Match( valid, invalid );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T Match<T>(Func<TDestination, T> valid, Func<TSource, T> invalid)
    {
        return IsValid ? valid( Result ) : invalid( Source );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil Match(Action<TDestination> valid, Action<TSource> invalid)
    {
        if ( IsValid )
            valid( Result );
        else
            invalid( Source );

        return Nil.Instance;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T> IfValid<T>(Func<TDestination, T?> valid)
        where T : notnull
    {
        return IsValid ? valid( Result ) : Maybe<T>.None;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfValid(Action<TDestination> valid)
    {
        if ( IsValid )
            valid( Result );

        return Nil.Instance;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? IfValidOrDefault<T>(Func<TDestination, T> valid)
    {
        return IsValid ? valid( Result ) : default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T IfValidOrDefault<T>(Func<TDestination, T> valid, T defaultValue)
    {
        return IsValid ? valid( Result ) : defaultValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T> IfInvalid<T>(Func<TSource, T?> invalid)
        where T : notnull
    {
        return IsValid ? Maybe<T>.None : invalid( Source );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfInvalid(Action<TSource> invalid)
    {
        if ( ! IsValid )
            invalid( Source );

        return Nil.Instance;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? IfInvalidOrDefault<T>(Func<TSource, T> invalid)
    {
        return IsValid ? default : invalid( Source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T IfInvalidOrDefault<T>(Func<TSource, T> invalid, T defaultValue)
    {
        return IsValid ? defaultValue : invalid( Source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TypeCast<TSource, TDestination>(TSource value)
    {
        return new TypeCast<TSource, TDestination>( value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TypeCast<TSource, TDestination>(PartialTypeCast<TSource> value)
    {
        return new TypeCast<TSource, TDestination>( value.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TypeCast<TSource, TDestination>(Nil value)
    {
        return Empty;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator TDestination(TypeCast<TSource, TDestination> value)
    {
        return value.GetResult();
    }

    [Pure]
    public static bool operator ==(TypeCast<TSource, TDestination> a, TypeCast<TSource, TDestination> b)
    {
        return a.Equals( b );
    }

    [Pure]
    public static bool operator !=(TypeCast<TSource, TDestination> a, TypeCast<TSource, TDestination> b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    IEnumerator<TDestination> IEnumerable<TDestination>.GetEnumerator()
    {
        return (IsValid ? One.Create( Result ) : Enumerable.Empty<TDestination>()).GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<TDestination>)this).GetEnumerator();
    }
}
