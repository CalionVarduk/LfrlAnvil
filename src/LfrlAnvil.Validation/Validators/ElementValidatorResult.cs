using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

public readonly struct ElementValidatorResult<TElement, TElementResult>
{
    public ElementValidatorResult(TElement element, Chain<TElementResult> result)
    {
        Element = element;
        Result = result;
    }

    public TElement Element { get; }
    public Chain<TElementResult> Result { get; }

    [Pure]
    public override string ToString()
    {
        var resultText = string.Join( Environment.NewLine, Result.Select( static (r, i) => $"{i + 1}. '{r}'" ) );
        return $"{nameof( Element )}: '{Element}', {nameof( Result )}:{Environment.NewLine}{resultText}";
    }
}
