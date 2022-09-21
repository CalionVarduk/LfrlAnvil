using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class ForEachValidator<T, TElementResult> : IValidator<IReadOnlyCollection<T>, ElementValidatorResult<T, TElementResult>>
{
    public ForEachValidator(IValidator<T, TElementResult> elementValidator)
    {
        ElementValidator = elementValidator;
    }

    public IValidator<T, TElementResult> ElementValidator { get; }

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
