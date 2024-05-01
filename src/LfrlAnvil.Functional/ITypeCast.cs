using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Functional.Exceptions;

namespace LfrlAnvil.Functional;

/// <summary>
/// Represents a type-erased result of a type cast.
/// </summary>
/// <typeparam name="TDestination">Destination type.</typeparam>
public interface ITypeCast<out TDestination> : IReadOnlyCollection<TDestination>
{
    /// <summary>
    /// Underlying source object.
    /// </summary>
    object? Source { get; }

    /// <summary>
    /// Specifies whether or not this type cast is valid.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Specifies whether or not this type cast is invalid.
    /// </summary>
    bool IsInvalid { get; }

    /// <summary>
    /// Gets the underlying type cast result.
    /// </summary>
    /// <returns>Underlying type cast result.</returns>
    /// <exception cref="ValueAccessException">When underlying type cast result does not exist.</exception>
    [Pure]
    TDestination GetResult();

    /// <summary>
    /// Gets the underlying type cast result or a default value when it does not exist.
    /// </summary>
    /// <returns>Underlying type cast result or a default value when it does not exist.</returns>
    [Pure]
    TDestination? GetResultOrDefault();
}
