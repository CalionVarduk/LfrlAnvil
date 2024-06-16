// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a generic result of a type cast.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
public readonly struct TypeCast<TSource, TDestination> : ITypeCast<TDestination>, IEquatable<TypeCast<TSource, TDestination>>
{
    /// <summary>
    /// Represents an invalid type cast.
    /// </summary>
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

    /// <summary>
    /// Underlying source object.
    /// </summary>
    public TSource Source { get; }

    /// <inheritdoc />
    [MemberNotNullWhen( true, nameof( Result ) )]
    public bool IsValid { get; }

    /// <inheritdoc />
    [MemberNotNullWhen( false, nameof( Result ) )]
    public bool IsInvalid => ! IsValid;

    object? ITypeCast<TDestination>.Source => Source;
    int IReadOnlyCollection<TDestination>.Count => IsValid ? 1 : 0;

    /// <summary>
    /// Returns a string representation of this <see cref="TypeCast{TSource,TDestination}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return IsValid
            ? $"{nameof( TypeCast )}<{typeof( TSource ).GetDebugString()} -> {typeof( TDestination ).GetDebugString()}>({Result})"
            : $"Invalid{nameof( TypeCast )}<{typeof( TSource ).GetDebugString()} -> {typeof( TDestination ).GetDebugString()}>({Generic<TSource>.ToString( Source )})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( Source ).Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is TypeCast<TSource, TDestination> c && Equals( c );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(TypeCast<TSource, TDestination> other)
    {
        if ( IsValid )
            return other.IsValid && Result.Equals( other.Result );

        return ! other.IsValid && Equality.Create( Source, other.Source ).Result;
    }

    /// <summary>
    /// Gets the underlying type cast result.
    /// </summary>
    /// <returns>Underlying type cast result.</returns>
    /// <exception cref="ValueAccessException">When underlying type cast result does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TDestination GetResult()
    {
        if ( ! IsValid )
        {
            ExceptionThrower.Throw(
                new ValueAccessException( Resources.MissingTypeCastResult<TSource, TDestination>(), nameof( Result ) ) );
        }

        return Result;
    }

    /// <summary>
    /// Gets the underlying type cast result or a default value when it does not exist.
    /// </summary>
    /// <returns>Underlying type cast result or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TDestination? GetResultOrDefault()
    {
        return Result;
    }

    /// <summary>
    /// Gets the underlying type cast result or a default value when it does not exist.
    /// </summary>
    /// <param name="defaultValue">Default value to return in case the type cast result does not exist.</param>
    /// <returns>Underlying type cast result or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TDestination GetResultOrDefault(TDestination defaultValue)
    {
        return IsValid ? Result : defaultValue;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="valid"/> delegate invocation when <see cref="IsValid"/> is equal to <b>true</b>,
    /// otherwise returns this instance.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>New <see cref="TypeCast{TSoure,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeCast<TDestination, T> Bind<T>(Func<TDestination, TypeCast<TDestination, T>> valid)
    {
        return IsValid ? valid( Result ) : TypeCast<TDestination, T>.Empty;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="valid"/> delegate invocation when <see cref="IsValid"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="invalid"/> delegate invocation.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <param name="invalid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T">Delegate's result type.</typeparam>
    /// <returns>New <see cref="TypeCast{TSoure,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeCast<TDestination, T> Bind<T>(
        Func<TDestination, TypeCast<TDestination, T>> valid,
        Func<TSource, TypeCast<TDestination, T>> invalid)
    {
        return Match( valid, invalid );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="valid"/> delegate invocation when <see cref="IsValid"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="invalid"/> delegate invocation.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <param name="invalid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>Delegate invocation result.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T Match<T>(Func<TDestination, T> valid, Func<TSource, T> invalid)
    {
        return IsValid ? valid( Result ) : invalid( Source );
    }

    /// <summary>
    /// Invokes the provided <paramref name="valid"/> delegate when <see cref="IsValid"/> is equal to <b>true</b>,
    /// otherwise invokes the provided <paramref name="invalid"/> delegate.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <param name="invalid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>false</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil Match(Action<TDestination> valid, Action<TSource> invalid)
    {
        if ( IsValid )
            valid( Result );
        else
            invalid( Source );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="valid"/> delegate invocation when <see cref="IsValid"/>
    /// is equal to <b>true</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T> IfValid<T>(Func<TDestination, T?> valid)
        where T : notnull
    {
        return IsValid ? valid( Result ) : Maybe<T>.None;
    }

    /// <summary>
    /// Invokes the provided <paramref name="valid"/> delegate when <see cref="IsValid"/> is equal to <b>true</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfValid(Action<TDestination> valid)
    {
        if ( IsValid )
            valid( Result );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="valid"/> delegate invocation when <see cref="IsValid"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? IfValidOrDefault<T>(Func<TDestination, T> valid)
    {
        return IsValid ? valid( Result ) : default;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="valid"/> delegate invocation when <see cref="IsValid"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="valid">Delegate to invoke when <see cref="IsValid"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="IsValid"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T IfValidOrDefault<T>(Func<TDestination, T> valid, T defaultValue)
    {
        return IsValid ? valid( Result ) : defaultValue;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="invalid"/> delegate invocation when <see cref="IsInvalid"/>
    /// is equal to <b>true</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="invalid">Delegate to invoke when <see cref="IsInvalid"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T> IfInvalid<T>(Func<TSource, T?> invalid)
        where T : notnull
    {
        return IsValid ? Maybe<T>.None : invalid( Source );
    }

    /// <summary>
    /// Invokes the provided <paramref name="invalid"/> delegate when <see cref="IsInvalid"/> is equal to <b>true</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="invalid">Delegate to invoke when <see cref="IsInvalid"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfInvalid(Action<TSource> invalid)
    {
        if ( ! IsValid )
            invalid( Source );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="invalid"/> delegate invocation when <see cref="IsInvalid"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="invalid">Delegate to invoke when <see cref="IsInvalid"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? IfInvalidOrDefault<T>(Func<TSource, T> invalid)
    {
        return IsValid ? default : invalid( Source );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="invalid"/> delegate invocation when <see cref="IsInvalid"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="invalid">Delegate to invoke when <see cref="IsInvalid"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="IsInvalid"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T IfInvalidOrDefault<T>(Func<TSource, T> invalid, T defaultValue)
    {
        return IsValid ? defaultValue : invalid( Source );
    }

    /// <summary>
    /// Converts the provided <paramref name="value"/> to a <see cref="TypeCast{TSource,TDestination}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>New <see cref="TypeCast{TSource,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TypeCast<TSource, TDestination>(TSource value)
    {
        return new TypeCast<TSource, TDestination>( value );
    }

    /// <summary>
    /// Converts the provided <paramref name="value"/> to a <see cref="TypeCast{TSource,TDestination}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>New <see cref="TypeCast{TSource,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TypeCast<TSource, TDestination>(PartialTypeCast<TSource> value)
    {
        return new TypeCast<TSource, TDestination>( value.Value );
    }

    /// <summary>
    /// Converts <see cref="Nil"/> to an <see cref="TypeCast{TSource,TDestination}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="Empty"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TypeCast<TSource, TDestination>(Nil value)
    {
        return Empty;
    }

    /// <summary>
    /// Gets the underlying type cast result.
    /// </summary>
    /// <returns>Underlying type cast result.</returns>
    /// <exception cref="ValueAccessException">When underlying type cast result does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator TDestination(TypeCast<TSource, TDestination> value)
    {
        return value.GetResult();
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(TypeCast<TSource, TDestination> a, TypeCast<TSource, TDestination> b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator !=(TypeCast<TSource, TDestination> a, TypeCast<TSource, TDestination> b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    IEnumerator<TDestination> IEnumerable<TDestination>.GetEnumerator()
    {
        return (IsValid ? Ref.Create( Result ) : Enumerable.Empty<TDestination>()).GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (( IEnumerable<TDestination> )this).GetEnumerator();
    }
}
