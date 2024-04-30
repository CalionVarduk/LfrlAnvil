using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a result of validation of an element of a collection.
/// </summary>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TElementResult">Element validation result type.</typeparam>
public readonly struct ElementValidatorResult<TElement, TElementResult>
{
    /// <summary>
    /// Creates a new <see cref="ElementValidatorResult{TElement,TElementResult}"/> instance.
    /// </summary>
    /// <param name="element">Validated element.</param>
    /// <param name="result">Element validation result.</param>
    public ElementValidatorResult(TElement element, Chain<TElementResult> result)
    {
        Element = element;
        Result = result;
    }

    /// <summary>
    /// Validated element.
    /// </summary>
    public TElement Element { get; }

    /// <summary>
    /// Element validation result.
    /// </summary>
    public Chain<TElementResult> Result { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ElementValidatorResult{TElement,TElementResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var resultText = string.Join( Environment.NewLine, Result.Select( static (r, i) => $"{i + 1}. '{r}'" ) );
        return $"{nameof( Element )}: '{Element}', {nameof( Result )}:{Environment.NewLine}{resultText}";
    }
}
