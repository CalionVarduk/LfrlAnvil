using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that selects a value to validate.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TMember">Validated value type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class SelectorValidator<T, TMember, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="SelectorValidator{T,TMember,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="memberSelector">Validated value selector.</param>
    public SelectorValidator(IValidator<TMember, TResult> validator, Func<T, TMember> memberSelector)
    {
        Validator = validator;
        MemberSelector = memberSelector;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<TMember, TResult> Validator { get; }

    /// <summary>
    /// Validated value selector.
    /// </summary>
    public Func<T, TMember> MemberSelector { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var member = MemberSelector( obj );
        return Validator.Validate( member );
    }
}
