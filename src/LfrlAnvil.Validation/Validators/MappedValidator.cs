using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that maps result of an underlying validator to a different type.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TSourceResult">Underlying validator's result type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class MappedValidator<T, TSourceResult, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="MappedValidator{T,TSourceResult,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="resultMapper">Underlying validator's result mapper.</param>
    public MappedValidator(
        IValidator<T, TSourceResult> validator,
        Func<Chain<TSourceResult>, Chain<TResult>> resultMapper)
    {
        Validator = validator;
        ResultMapper = resultMapper;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<T, TSourceResult> Validator { get; }

    /// <summary>
    /// Underlying validator's result mapper.
    /// </summary>
    public Func<Chain<TSourceResult>, Chain<TResult>> ResultMapper { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var result = Validator.Validate( obj );
        var mappedResult = ResultMapper( result );
        return mappedResult;
    }
}
