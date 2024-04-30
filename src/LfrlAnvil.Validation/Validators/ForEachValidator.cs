using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator for a collection of elements where each element is validated separately.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <typeparam name="TElementResult">Element result type.</typeparam>
public sealed class ForEachValidator<T, TElementResult> : IValidator<IReadOnlyCollection<T>, ElementValidatorResult<T, TElementResult>>
{
    /// <summary>
    /// Creates a new <see cref="ForEachValidator{T,TElementResult}"/> instance.
    /// </summary>
    /// <param name="elementValidator">Underlying element validator.</param>
    public ForEachValidator(IValidator<T, TElementResult> elementValidator)
    {
        ElementValidator = elementValidator;
    }

    /// <summary>
    /// Underlying element validator.
    /// </summary>
    public IValidator<T, TElementResult> ElementValidator { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<ElementValidatorResult<T, TElementResult>> Validate(IReadOnlyCollection<T> obj)
    {
        var result = Chain<ElementValidatorResult<T, TElementResult>>.Empty;
        foreach ( var element in obj )
        {
            var elementResult = ElementValidator.Validate( element );
            if ( elementResult.Count > 0 )
                result = result.Extend( new ElementValidatorResult<T, TElementResult>( element, elementResult ) );
        }

        return result;
    }
}
