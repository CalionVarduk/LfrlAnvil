using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

public readonly struct OrValidatorResult<TResult>
{
    public OrValidatorResult(Chain<TResult> result)
    {
        Result = result;
    }

    public Chain<TResult> Result { get; }

    [Pure]
    public override string ToString()
    {
        return string.Join( $" or{Environment.NewLine}", Result.Select( static r => $"'{r}'" ) );
    }
}
