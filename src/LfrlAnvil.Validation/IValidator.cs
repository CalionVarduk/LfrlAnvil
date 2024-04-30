using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation;

/// <summary>
/// Represents a generic object validator.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IValidator<in T, TResult>
{
    /// <summary>
    /// Validates the provided <paramref name="obj"/>.
    /// </summary>
    /// <param name="obj">Object to validate.</param>
    /// <returns>Result of <paramref name="obj"/> validation.</returns>
    [Pure]
    Chain<TResult> Validate(T obj);
}
