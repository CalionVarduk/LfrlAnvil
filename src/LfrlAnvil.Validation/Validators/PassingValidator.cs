using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that always passes.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class PassingValidator<T, TResult> : IValidator<T, TResult>
{
    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Chain<TResult>.Empty;
    }
}
