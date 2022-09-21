using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class MemberValidator<T, TMember, TResult> : IValidator<T, TResult>
{
    public MemberValidator(IValidator<TMember, TResult> validator, Func<T, TMember> memberSelector)
    {
        Validator = validator;
        MemberSelector = memberSelector;
    }

    public IValidator<TMember, TResult> Validator { get; }
    public Func<T, TMember> MemberSelector { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var member = MemberSelector( obj );
        return Validator.Validate( member );
    }
}
