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
/// Represents a generic result of an action that may throw an error.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct Erratic<T> : IErratic, IEquatable<Erratic<T>>, IReadOnlyCollection<T>
{
    /// <summary>
    /// Represents an empty erratic, without an error.
    /// </summary>
    public static readonly Erratic<T> Empty = new Erratic<T>();

    internal readonly T? Value;
    internal readonly Exception? Error;

    internal Erratic(T value)
    {
        Value = value;
        Error = default;
    }

    internal Erratic(Exception error)
    {
        Value = default;
        Error = error;
    }

    /// <inheritdoc />
    [MemberNotNullWhen( true, nameof( Error ) )]
    [MemberNotNullWhen( false, nameof( Value ) )]
    public bool HasError => Error is not null;

    /// <inheritdoc />
    [MemberNotNullWhen( true, nameof( Value ) )]
    [MemberNotNullWhen( false, nameof( Error ) )]
    public bool IsOk => ! HasError;

    /// <inheritdoc />
    public int Count => HasError ? 0 : 1;

    /// <summary>
    /// Returns a string representation of this <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return HasError
            ? $"{nameof( Error )}({Error.GetType().Name})"
            : $"{nameof( Value )}({Generic<T>.ToString( Value )})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HasError ? Hash.Default.Add( Error ).Value : Hash.Default.Add( Value ).Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Erratic<T> u && Equals( u );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Erratic<T> other)
    {
        if ( HasError )
            return other.HasError && Error.Equals( other.Error );

        return other.IsOk && Equality.Create( Value, other.Value ).Result;
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
        if ( ! IsOk )
            ExceptionThrower.Throw( new ValueAccessException( Resources.MissingErraticValue<T>(), nameof( Value ) ) );

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
        return IsOk ? Value : defaultValue;
    }

    /// <summary>
    /// Gets the underlying error.
    /// </summary>
    /// <returns>Underlying error.</returns>
    /// <exception cref="ValueAccessException">When underlying error does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Exception GetError()
    {
        if ( ! HasError )
            ExceptionThrower.Throw( new ValueAccessException( Resources.MissingErraticError<T>(), nameof( Error ) ) );

        return Error;
    }

    /// <summary>
    /// Gets the underlying error or null when it does not exist.
    /// </summary>
    /// <returns>Underlying error or null when it does not exist.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Exception? GetErrorOrDefault()
    {
        return Error;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="ok"/> delegate invocation when <see cref="IsOk"/> is equal to <b>true</b>,
    /// otherwise returns this instance.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Erratic<T2> Bind<T2>(Func<T, Erratic<T2>> ok)
    {
        return IsOk ? ok( Value ) : new Erratic<T2>( Error );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="ok"/> delegate invocation when <see cref="IsOk"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="error"/> delegate invocation.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <param name="error">Delegate to invoke when <see cref="IsOk"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Erratic<T2> Bind<T2>(Func<T, Erratic<T2>> ok, Func<Exception, Erratic<T2>> error)
    {
        return Match( ok, error );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="ok"/> delegate invocation when <see cref="IsOk"/> is equal to <b>true</b>,
    /// otherwise returns the result of the provided <paramref name="error"/> delegate invocation.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <param name="error">Delegate to invoke when <see cref="IsOk"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 Match<T2>(Func<T, T2> ok, Func<Exception, T2> error)
    {
        return IsOk ? ok( Value ) : error( Error );
    }

    /// <summary>
    /// Invokes the provided <paramref name="ok"/> delegate when <see cref="IsOk"/> is equal to <b>true</b>,
    /// otherwise invokes the provided <paramref name="error"/> delegate.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <param name="error">Delegate to invoke when <see cref="IsOk"/> is equal to <b>false</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil Match(Action<T> ok, Action<Exception> error)
    {
        if ( IsOk )
            ok( Value );
        else
            error( Error );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="ok"/> delegate invocation when <see cref="IsOk"/> is equal to <b>true</b>,
    /// otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> IfOk<T2>(Func<T, T2?> ok)
        where T2 : notnull
    {
        return IsOk ? ok( Value ) : Maybe<T2>.None;
    }

    /// <summary>
    /// Invokes the provided <paramref name="ok"/> delegate when <see cref="IsOk"/> is equal to <b>true</b>, otherwise does nothing.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfOk(Action<T> ok)
    {
        if ( IsOk )
            ok( Value );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="ok"/> delegate invocation when <see cref="IsOk"/> is equal to <b>true</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? IfOkOrDefault<T2>(Func<T, T2> ok)
    {
        return IsOk ? ok( Value ) : default;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="ok"/> delegate invocation when <see cref="IsOk"/> is equal to <b>true</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="ok">Delegate to invoke when <see cref="IsOk"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="IsOk"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 IfOkOrDefault<T2>(Func<T, T2> ok, T2 defaultValue)
    {
        return IsOk ? ok( Value ) : defaultValue;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="error"/> delegate invocation when <see cref="HasError"/>
    /// is equal to <b>true</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="error">Delegate to invoke when <see cref="HasError"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> IfError<T2>(Func<Exception, T2?> error)
        where T2 : notnull
    {
        return HasError ? error( Error ) : Maybe<T2>.None;
    }

    /// <summary>
    /// Invokes the provided <paramref name="error"/> delegate when <see cref="HasError"/> is equal to <b>true</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="error">Delegate to invoke when <see cref="HasError"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfError(Action<Exception> error)
    {
        if ( HasError )
            error( Error );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="error"/> delegate invocation when <see cref="HasError"/> is equal to <b>true</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="error">Delegate to invoke when <see cref="HasError"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? IfErrorOrDefault<T2>(Func<Exception, T2> error)
    {
        return HasError ? error( Error ) : default;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="error"/> delegate invocation when <see cref="HasError"/> is equal to <b>true</b>,
    /// otherwise returns a default value.
    /// </summary>
    /// <param name="error">Delegate to invoke when <see cref="HasError"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="HasError"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 IfErrorOrDefault<T2>(Func<Exception, T2> error, T2 defaultValue)
    {
        return HasError ? error( Error ) : defaultValue;
    }

    /// <summary>
    /// Converts the provided <paramref name="value"/> to an <see cref="Erratic{T}"/> instance without an error.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Erratic<T>(T value)
    {
        return new Erratic<T>( value );
    }

    /// <summary>
    /// Converts the provided <paramref name="error"/> to an <see cref="Erratic{T}"/> instance with an error.
    /// </summary>
    /// <param name="error">Exception to convert.</param>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Erratic<T>(Exception error)
    {
        return new Erratic<T>( error );
    }

    /// <summary>
    /// Converts the provided <paramref name="value"/> to an equivalent <see cref="Either{T1,T2}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>New <see cref="Either{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Either<T, Exception>(Erratic<T> value)
    {
        return value.HasError ? new Either<T, Exception>( value.Error ) : new Either<T, Exception>( value.Value );
    }

    /// <summary>
    /// Converts the provided <paramref name="value"/> to an equivalent <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Erratic<T>(Either<T, Exception> value)
    {
        return value.HasFirst ? new Erratic<T>( value.First ) : new Erratic<T>( value.Second );
    }

    /// <summary>
    /// Converts <see cref="Nil"/> to an <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="Empty"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Erratic<T>(Nil value)
    {
        return Empty;
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <returns>Underlying value.</returns>
    /// <exception cref="ValueAccessException">When underlying value does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator T(Erratic<T> value)
    {
        return value.GetValue();
    }

    /// <summary>
    /// Gets the underlying error.
    /// </summary>
    /// <returns>Underlying error.</returns>
    /// <exception cref="ValueAccessException">When underlying error does not exist.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator Exception(Erratic<T> value)
    {
        return value.GetError();
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Erratic<T> a, Erratic<T> b)
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
    public static bool operator !=(Erratic<T> a, Erratic<T> b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    object IErratic.GetValue()
    {
        return GetValue()!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    object? IErratic.GetValueOrDefault()
    {
        return GetValueOrDefault();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    Exception IErratic.GetError()
    {
        return GetError();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    Exception? IErratic.GetErrorOrDefault()
    {
        return GetErrorOrDefault();
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return (IsOk ? Ref.Create( Value ) : Enumerable.Empty<T>()).GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return (( IEnumerable<T> )this).GetEnumerator();
    }
}
