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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a generic pair of exclusive values.
/// </summary>
/// <typeparam name="T1">First value type.</typeparam>
/// <typeparam name="T2">Second value type.</typeparam>
public readonly struct Either<T1, T2> : IEquatable<Either<T1, T2>>
{
    /// <summary>
    /// Represents an empty either, with default second value.
    /// </summary>
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

    /// <summary>
    /// Specifies whether or not this instance contains first value.
    /// </summary>
    [MemberNotNullWhen( true, nameof( First ) )]
    [MemberNotNullWhen( false, nameof( Second ) )]
    public bool HasFirst { get; }

    /// <summary>
    /// Specifies whether or not this instance contains second value.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Second ) )]
    [MemberNotNullWhen( false, nameof( First ) )]
    public bool HasSecond => ! HasFirst;

    /// <summary>
    /// Returns a string representation of this <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return HasFirst
            ? $"{nameof( First )}({Generic<T1>.ToString( First )})"
            : $"{nameof( Second )}({Generic<T2>.ToString( Second )})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HasFirst ? Hash.Default.Add( First ).Value : Hash.Default.Add( Second ).Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Either<T1, T2> e && Equals( e );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Either<T1, T2> other)
    {
        if ( HasFirst )
            return other.HasFirst && Equality.Create( First, other.First ).Result;

        return other.HasSecond && Equality.Create( Second, other.Second ).Result;
    }

    /// <summary>
    /// Gets the first value.
    /// </summary>
    /// <returns>First value.</returns>
    /// <exception cref="ValueAccessException">When first value does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T1 GetFirst()
    {
        if ( ! HasFirst )
            ExceptionThrower.Throw( new ValueAccessException( Resources.MissingFirstEitherValue<T1, T2>(), nameof( First ) ) );

        return First;
    }

    /// <summary>
    /// Gets the first value or a default value when it does not exist.
    /// </summary>
    /// <returns>First value or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T1? GetFirstOrDefault()
    {
        return First;
    }

    /// <summary>
    /// Gets the first value or a default value when it does not exist.
    /// </summary>
    /// <param name="defaultValue">Default value to return in case the first value does not exist.</param>
    /// <returns>First value or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T1 GetFirstOrDefault(T1 defaultValue)
    {
        return HasFirst ? First : defaultValue;
    }

    /// <summary>
    /// Gets the second value.
    /// </summary>
    /// <returns>Second value.</returns>
    /// <exception cref="ValueAccessException">When second value does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 GetSecond()
    {
        if ( ! HasSecond )
            ExceptionThrower.Throw( new ValueAccessException( Resources.MissingSecondEitherValue<T1, T2>(), nameof( Second ) ) );

        return Second;
    }

    /// <summary>
    /// Gets the second value or a default value when it does not exist.
    /// </summary>
    /// <returns>Second value or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? GetSecondOrDefault()
    {
        return Second;
    }

    /// <summary>
    /// Gets the second value or a default value when it does not exist.
    /// </summary>
    /// <param name="defaultValue">Default value to return in case the second value does not exist.</param>
    /// <returns>Second value or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 GetSecondOrDefault(T2 defaultValue)
    {
        return HasSecond ? Second : defaultValue;
    }

    /// <summary>
    /// Creates a new <see cref="Either{T1,T2}"/> instance by swapping first and second positions.
    /// </summary>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T2, T1> Swap()
    {
        return HasFirst ? new Either<T2, T1>( First ) : new Either<T2, T1>( Second );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="first"/> delegate invocation when <see cref="HasFirst"/> is equal to <b>true</b>,
    /// otherwise returns this instance.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T3, T2> Bind<T3>(Func<T1, Either<T3, T2>> first)
    {
        return HasFirst ? first( First ) : new Either<T3, T2>( Second );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="second"/> delegate invocation when <see cref="HasSecond"/>
    /// is equal to <b>true</b>, otherwise returns this instance.
    /// </summary>
    /// <param name="second">Delegate to invoke when <see cref="HasSecond"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T1, T3> BindSecond<T3>(Func<T2, Either<T1, T3>> second)
    {
        return HasSecond ? second( Second ) : new Either<T1, T3>( First );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="first"/> delegate invocation when <see cref="HasFirst"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="second"/> delegate invocation.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <param name="second">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T3">First delegate's result type.</typeparam>
    /// <typeparam name="T4">Second delegate's result type.</typeparam>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Either<T3, T4> Bind<T3, T4>(Func<T1, Either<T3, T4>> first, Func<T2, Either<T3, T4>> second)
    {
        return Match( first, second );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="first"/> delegate invocation when <see cref="HasFirst"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="second"/> delegate invocation.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <param name="second">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>Delegate invocation result.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3 Match<T3>(Func<T1, T3> first, Func<T2, T3> second)
    {
        return HasFirst ? first( First ) : second( Second );
    }

    /// <summary>
    /// Invokes the provided <paramref name="first"/> delegate when <see cref="HasFirst"/> is equal to <b>true</b>,
    /// otherwise invokes the provided <paramref name="second"/> delegate.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <param name="second">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>false</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil Match(Action<T1> first, Action<T2> second)
    {
        if ( HasFirst )
            first( First );
        else
            second( Second );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="first"/> delegate invocation when <see cref="HasFirst"/>
    /// is equal to <b>true</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T3> IfFirst<T3>(Func<T1, T3?> first)
        where T3 : notnull
    {
        return HasFirst ? first( First ) : Maybe<T3>.None;
    }

    /// <summary>
    /// Invokes the provided <paramref name="first"/> delegate when <see cref="HasFirst"/> is equal to <b>true</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfFirst(Action<T1> first)
    {
        if ( HasFirst )
            first( First );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="first"/> delegate invocation when <see cref="HasFirst"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3? IfFirstOrDefault<T3>(Func<T1, T3> first)
    {
        return HasFirst ? first( First ) : default;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="first"/> delegate invocation when <see cref="HasFirst"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="first">Delegate to invoke when <see cref="HasFirst"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="HasFirst"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3 IfFirstOrDefault<T3>(Func<T1, T3> first, T3 defaultValue)
    {
        return HasFirst ? first( First ) : defaultValue;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="second"/> delegate invocation when <see cref="HasSecond"/>
    /// is equal to <b>true</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="second">Delegate to invoke when <see cref="HasSecond"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T3> IfSecond<T3>(Func<T2, T3?> second)
        where T3 : notnull
    {
        return HasSecond ? second( Second ) : Maybe<T3>.None;
    }

    /// <summary>
    /// Invokes the provided <paramref name="second"/> delegate when <see cref="HasSecond"/> is equal to <b>true</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="second">Delegate to invoke when <see cref="HasSecond"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfSecond(Action<T2> second)
    {
        if ( HasSecond )
            second( Second );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="second"/> delegate invocation when <see cref="HasSecond"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="second">Delegate to invoke when <see cref="HasSecond"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3? IfSecondOrDefault<T3>(Func<T2, T3> second)
    {
        return HasSecond ? second( Second ) : default;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="second"/> delegate invocation when <see cref="HasSecond"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="second">Delegate to invoke when <see cref="HasSecond"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="HasSecond"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T3">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T3 IfSecondOrDefault<T3>(Func<T2, T3> second, T3 defaultValue)
    {
        return HasSecond ? second( Second ) : defaultValue;
    }

    /// <summary>
    /// Converts the provided <paramref name="first"/> to an <see cref="Either{T1,T2}"/> instance with a first value.
    /// </summary>
    /// <param name="first">Value to convert.</param>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(T1 first)
    {
        return new Either<T1, T2>( first );
    }

    /// <summary>
    /// Converts the provided <paramref name="second"/> to an <see cref="Either{T1,T2}"/> instance with a second value.
    /// </summary>
    /// <param name="second">Value to convert.</param>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(T2 second)
    {
        return new Either<T1, T2>( second );
    }

    /// <summary>
    /// Converts the provided <paramref name="part"/> to an <see cref="Either{T1,T2}"/> instance with a first value.
    /// </summary>
    /// <param name="part">Value to convert.</param>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(PartialEither<T1> part)
    {
        return new Either<T1, T2>( part.Value );
    }

    /// <summary>
    /// Converts the provided <paramref name="part"/> to an <see cref="Either{T1,T2}"/> instance with a second value.
    /// </summary>
    /// <param name="part">Value to convert.</param>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(PartialEither<T2> part)
    {
        return new Either<T1, T2>( part.Value );
    }

    /// <summary>
    /// Converts <see cref="Nil"/> to an <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="Empty"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T1, T2>(Nil value)
    {
        return Empty;
    }

    /// <summary>
    /// Gets the first value.
    /// </summary>
    /// <returns>First value.</returns>
    /// <exception cref="ValueAccessException">When first value does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator T1(Either<T1, T2> value)
    {
        return value.GetFirst();
    }

    /// <summary>
    /// Gets the second value.
    /// </summary>
    /// <returns>Second value.</returns>
    /// <exception cref="ValueAccessException">When second value does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator T2(Either<T1, T2> value)
    {
        return value.GetSecond();
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Either<T1, T2> a, Either<T1, T2> b)
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
    public static bool operator !=(Either<T1, T2> a, Either<T1, T2> b)
    {
        return ! a.Equals( b );
    }
}
