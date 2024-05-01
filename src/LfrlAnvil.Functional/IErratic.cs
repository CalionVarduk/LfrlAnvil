using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Functional.Exceptions;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a type-erased result of an action that may throw an error.
/// </summary>
public interface IErratic
{
    /// <summary>
    /// Specifies whether or not this instance contains an error.
    /// </summary>
    bool HasError { get; }

    /// <summary>
    /// Specifies whether or not this instance contains a value.
    /// </summary>
    bool IsOk { get; }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    /// <returns>Underlying value.</returns>
    /// <exception cref="ValueAccessException">When underlying value does not exist.</exception>
    [Pure]
    object GetValue();

    /// <summary>
    /// Gets the underlying value or a default value when it does not exist.
    /// </summary>
    /// <returns>Underlying value or a default value when it does not exist.</returns>
    [Pure]
    object? GetValueOrDefault();

    /// <summary>
    /// Gets the underlying error.
    /// </summary>
    /// <returns>Underlying error.</returns>
    /// <exception cref="ValueAccessException">When underlying error does not exist.</exception>
    [Pure]
    Exception GetError();

    /// <summary>
    /// Gets the underlying error or null when it does not exist.
    /// </summary>
    /// <returns>Underlying error or null when it does not exist.</returns>
    [Pure]
    Exception? GetErrorOrDefault();
}
