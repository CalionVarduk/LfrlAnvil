using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Functional.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a generic optional value.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct Maybe<T> : IEquatable<Maybe<T>>, IReadOnlyCollection<T>
    where T : notnull
{
    /// <summary>
    /// Represents a lack of value.
    /// </summary>
    public static readonly Maybe<T> None = new Maybe<T>();

    internal readonly T? Value;

    internal Maybe(T value)
    {
        HasValue = true;
        Value = value;
    }

    /// <summary>
    /// Specifies whether or not this instance contains a non-null value.
    /// </summary>
    [MemberNotNullWhen( true, nameof( Value ) )]
    public bool HasValue { get; }

    int IReadOnlyCollection<T>.Count => HasValue ? 1 : 0;

    /// <summary>
    /// Returns a string representation of this <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return HasValue ? $"{nameof( Value )}({Value})" : nameof( None );
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HasValue ? Value.GetHashCode() : 0;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Maybe<T> m && Equals( m );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Maybe<T> other)
    {
        if ( HasValue )
            return other.HasValue && Value.Equals( other.Value );

        return ! other.HasValue;
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <returns>Underlying value.</returns>
    /// <exception cref="ValueAccessException">When underlying value does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T GetValue()
    {
        if ( ! HasValue )
            ExceptionThrower.Throw( new ValueAccessException( Resources.MissingMaybeValue<T>(), nameof( Value ) ) );

        return Value;
    }

    /// <summary>
    /// Gets the underlying value or a default value when it does not exist.
    /// </summary>
    /// <returns>Underlying value or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T? GetValueOrDefault()
    {
        return Value;
    }

    /// <summary>
    /// Gets the underlying value or a default value when it does not exist.
    /// </summary>
    /// <param name="defaultValue">Default value to return in case the underlying value does not exist.</param>
    /// <returns>Underlying value or a default value when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T GetValueOrDefault(T defaultValue)
    {
        return HasValue ? Value : defaultValue;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="some"/> delegate invocation when <see cref="HasValue"/> is equal to <b>true</b>,
    /// otherwise returns this instance.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> Bind<T2>(Func<T, Maybe<T2>> some)
        where T2 : notnull
    {
        return HasValue ? some( Value ) : Maybe<T2>.None;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="some"/> delegate invocation when <see cref="HasValue"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="none"/> delegate invocation.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <param name="none">Delegate to invoke when <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> Bind<T2>(Func<T, Maybe<T2>> some, Func<Maybe<T2>> none)
        where T2 : notnull
    {
        return Match( some, none );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="some"/> delegate invocation when <see cref="HasValue"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="none"/> delegate invocation.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <param name="none">Delegate to invoke when <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 Match<T2>(Func<T, T2> some, Func<T2> none)
    {
        return HasValue ? some( Value ) : none();
    }

    /// <summary>
    /// Invokes the provided <paramref name="some"/> delegate when <see cref="HasValue"/> is equal to <b>true</b>,
    /// otherwise invokes the provided <paramref name="none"/> delegate.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <param name="none">Delegate to invoke when <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil Match(Action<T> some, Action none)
    {
        if ( HasValue )
            some( Value );
        else
            none();

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="some"/> delegate invocation when <see cref="HasValue"/> is equal to <b>true</b>,
    /// otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> IfSome<T2>(Func<T, T2?> some)
        where T2 : notnull
    {
        return HasValue ? some( Value ) : Maybe<T2>.None;
    }

    /// <summary>
    /// Invokes the provided <paramref name="some"/> delegate when <see cref="HasValue"/> is equal to <b>true</b>, otherwise does nothing.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfSome(Action<T> some)
    {
        if ( HasValue )
            some( Value );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="some"/> delegate invocation when <see cref="HasValue"/> is equal to <b>true</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? IfSomeOrDefault<T2>(Func<T, T2> some)
    {
        return HasValue ? some( Value ) : default;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="some"/> delegate invocation when <see cref="HasValue"/> is equal to <b>true</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="some">Delegate to invoke when <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 IfSomeOrDefault<T2>(Func<T, T2> some, T2 defaultValue)
    {
        return HasValue ? some( Value ) : defaultValue;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="none"/> delegate invocation when <see cref="HasValue"/>
    /// is equal to <b>false</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="none">Delegate to invoke when <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> IfNone<T2>(Func<T2?> none)
        where T2 : notnull
    {
        return HasValue ? Maybe<T2>.None : none();
    }

    /// <summary>
    /// Invokes the provided <paramref name="none"/> delegate when <see cref="HasValue"/> is equal to <b>false</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="none">Delegate to invoke when <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfNone(Action none)
    {
        if ( ! HasValue )
            none();

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="none"/> delegate invocation when <see cref="HasValue"/> is equal to <b>false</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="none">Delegate to invoke when <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? IfNoneOrDefault<T2>(Func<T2> none)
    {
        return HasValue ? default : none();
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="none"/> delegate invocation when <see cref="HasValue"/> is equal to <b>false</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="none">Delegate to invoke when <see cref="HasValue"/> is equal to <b>false</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="HasValue"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 IfNoneOrDefault<T2>(Func<T2> none, T2 defaultValue)
    {
        return HasValue ? defaultValue : none();
    }

    /// <summary>
    /// Converts the provided <paramref name="value"/> to a <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Maybe<T>(T? value)
    {
        return Generic<T>.IsNull( value ) ? None : new Maybe<T>( value );
    }

    /// <summary>
    /// Converts <see cref="Nil"/> to a <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="none">Value to convert.</param>
    /// <returns><see cref="None"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Maybe<T>(Nil none)
    {
        return None;
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <returns>Underlying value.</returns>
    /// <exception cref="ValueAccessException">When underlying value does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator T(Maybe<T> value)
    {
        return value.GetValue();
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Maybe<T> a, Maybe<T> b)
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
    public static bool operator !=(Maybe<T> a, Maybe<T> b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return (HasValue ? Ref.Create( Value ) : Enumerable.Empty<T>()).GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (( IEnumerable<T> )this).GetEnumerator();
    }
}
