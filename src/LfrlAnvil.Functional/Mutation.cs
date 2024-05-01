using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a generic tuple of (old, new) values.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public readonly struct Mutation<T> : IEquatable<Mutation<T>>
{
    /// <summary>
    /// Represents an empty, unchanged mutation.
    /// </summary>
    public static readonly Mutation<T> Empty = new Mutation<T>();

    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="oldValue">Old value.</param>
    /// <param name="value">New value.</param>
    public Mutation(T oldValue, T value)
    {
        OldValue = oldValue;
        Value = value;
    }

    /// <summary>
    /// Old value.
    /// </summary>
    public T OldValue { get; }

    /// <summary>
    /// New value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Specifies whether or not the <see cref="OldValue"/> is different than the <see cref="Value"/>,
    /// using the <see cref="EqualityComparer{T}.Default"/> comparer.
    /// </summary>
    public bool HasChanged => Generic<T>.AreNotEqual( OldValue, Value );

    /// <summary>
    /// Returns a string representation of this <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( Mutation )}({Generic<T>.ToString( OldValue )} -> {Generic<T>.ToString( Value )})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( OldValue ).Add( Value ).Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Mutation<T> m && Equals( m );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(Mutation<T> other)
    {
        return Equality.Create( OldValue, other.OldValue ).Result && Equality.Create( Value, other.Value ).Result;
    }

    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="newValue">New value.</param>
    /// <returns>New <see cref="Mutation{T}"/> instance with <see cref="Value"/> as <see cref="OldValue"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Mutation<T> Mutate(T newValue)
    {
        return new Mutation<T>( Value, newValue );
    }

    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="newValue">New value.</param>
    /// <returns>New <see cref="Mutation{T}"/> instance with unchanged <see cref="OldValue"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Mutation<T> Replace(T newValue)
    {
        return new Mutation<T>( OldValue, newValue );
    }

    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <returns>New <see cref="Mutation{T}"/> instance with <see cref="OldValue"/> as <see cref="Value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Mutation<T> Revert()
    {
        return new Mutation<T>( OldValue, OldValue );
    }

    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <returns>New <see cref="Mutation{T}"/> instance with <see cref="OldValue"/> and <see cref="Value"/> swapped.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Mutation<T> Swap()
    {
        return new Mutation<T>( Value, OldValue );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="changed"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>true</b>, otherwise returns <see cref="Empty"/>.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Mutation{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Mutation<T2> Bind<T2>(Func<(T OldValue, T Value), Mutation<T2>> changed)
    {
        return HasChanged ? changed( (OldValue, Value) ) : Mutation<T2>.Empty;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="changed"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>true</b>, otherwise returns the result of the provided <paramref name="unchanged"/> delegate invocation.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <param name="unchanged">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Mutation{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Mutation<T2> Bind<T2>(Func<(T OldValue, T Value), Mutation<T2>> changed, Func<T, Mutation<T2>> unchanged)
    {
        return Match( changed, unchanged );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="changed"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>true</b>, otherwise returns the result of the provided <paramref name="unchanged"/> delegate invocation.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <param name="unchanged">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 Match<T2>(Func<(T OldValue, T Value), T2> changed, Func<T, T2> unchanged)
    {
        return HasChanged ? changed( (OldValue, Value) ) : unchanged( Value );
    }

    /// <summary>
    /// Invokes the provided <paramref name="changed"/> delegate when <see cref="HasChanged"/> is equal to <b>true</b>,
    /// otherwise invokes the provided <paramref name="unchanged"/> delegate.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <param name="unchanged">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil Match(Action<(T OldValue, T Value)> changed, Action<T> unchanged)
    {
        if ( HasChanged )
            changed( (OldValue, Value) );
        else
            unchanged( Value );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="changed"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>true</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> IfChanged<T2>(Func<(T OldValue, T Value), T2?> changed)
        where T2 : notnull
    {
        return HasChanged ? changed( (OldValue, Value) ) : Maybe<T2>.None;
    }

    /// <summary>
    /// Invokes the provided <paramref name="changed"/> delegate when <see cref="HasChanged"/> is equal to <b>true</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfChanged(Action<(T OldValue, T Value)> changed)
    {
        if ( HasChanged )
            changed( (OldValue, Value) );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="changed"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? IfChangedOrDefault<T2>(Func<(T OldValue, T Value), T2> changed)
    {
        return HasChanged ? changed( (OldValue, Value) ) : default;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="changed"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>true</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="changed">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 IfChangedOrDefault<T2>(Func<(T OldValue, T Value), T2> changed, T2 defaultValue)
    {
        return HasChanged ? changed( (OldValue, Value) ) : defaultValue;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="unchanged"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>false</b>, otherwise returns <see cref="Maybe{T}.None"/>.
    /// </summary>
    /// <param name="unchanged">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>New <see cref="Maybe{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Maybe<T2> IfUnchanged<T2>(Func<T, T2?> unchanged)
        where T2 : notnull
    {
        return HasChanged ? Maybe<T2>.None : unchanged( Value );
    }

    /// <summary>
    /// Invokes the provided <paramref name="unchanged"/> delegate when <see cref="HasChanged"/> is equal to <b>false</b>,
    /// otherwise does nothing.
    /// </summary>
    /// <param name="unchanged">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <returns><see cref="Nil"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Nil IfUnchanged(Action<T> unchanged)
    {
        if ( ! HasChanged )
            unchanged( Value );

        return Nil.Instance;
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="unchanged"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>false</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="unchanged">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2? IfUnchangedOrDefault<T2>(Func<T, T2> unchanged)
    {
        return HasChanged ? default : unchanged( Value );
    }

    /// <summary>
    /// Returns the result of the provided <paramref name="unchanged"/> delegate invocation when <see cref="HasChanged"/>
    /// is equal to <b>false</b>, otherwise returns a default value.
    /// </summary>
    /// <param name="unchanged">Delegate to invoke when <see cref="HasChanged"/> is equal to <b>false</b>.</param>
    /// <param name="defaultValue">Value to return <see cref="HasChanged"/> is equal to <b>true</b>.</param>
    /// <typeparam name="T2">Result type.</typeparam>
    /// <returns>Delegate invocation result or a default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T2 IfUnchangedOrDefault<T2>(Func<T, T2> unchanged, T2 defaultValue)
    {
        return HasChanged ? defaultValue : unchanged( Value );
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <returns><see cref="Value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator T(Mutation<T> source)
    {
        return source.Value;
    }

    /// <summary>
    /// Converts <see cref="Nil"/> to a <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="Empty"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator Mutation<T>(Nil value)
    {
        return Empty;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool operator ==(Mutation<T> a, Mutation<T> b)
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
    public static bool operator !=(Mutation<T> a, Mutation<T> b)
    {
        return ! a.Equals( b );
    }
}
