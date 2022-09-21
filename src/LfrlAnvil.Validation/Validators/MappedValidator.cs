using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class MappedValidator<T, TSourceResult, TResult> : IValidator<T, TResult>
{
    public MappedValidator(
        IValidator<T, TSourceResult> validator,
        Func<Chain<TSourceResult>, Chain<TResult>> resultMapper)
    {
        Validator = validator;
        ResultMapper = resultMapper;
    }

    public IValidator<T, TSourceResult> Validator { get; }
    public Func<Chain<TSourceResult>, Chain<TResult>> ResultMapper { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var result = Validator.Validate( obj );
        var mappedResult = ResultMapper( result );
        return mappedResult;
    }
}
