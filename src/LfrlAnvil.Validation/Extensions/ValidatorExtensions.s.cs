using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation.Validators;

namespace LfrlAnvil.Validation.Extensions;

public static class ValidatorExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<IReadOnlyCollection<T>, ElementValidatorResult<T, TResult>> ForCollectionElement<T, TResult>(
        this IValidator<T, TResult> validator)
    {
        return new ForEachValidator<T, TResult>( validator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<TTarget, TResult> ForMember<T, TTarget, TResult>(
        this IValidator<T, TResult> validator,
        Func<TTarget, T> memberSelector)
    {
        return new MemberValidator<TTarget, T, TResult>( validator, memberSelector );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, TResult> Map<T, TSourceResult, TResult>(
        this IValidator<T, TSourceResult> validator,
        Func<Chain<TSourceResult>, Chain<TResult>> resultMapper)
    {
        return new MappedValidator<T, TSourceResult, TResult>( validator, resultMapper );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IValidator<T, FormattedValidatorResult<TResource>> Format<T, TResource>(
        this IValidator<T, ValidationMessage<TResource>> validator,
        IValidationMessageFormatter<TResource> formatter,
        Func<IFormatProvider?>? formatProvider = null)
    {
        return new FormattedValidator<T, TResource>( validator, formatter, formatProvider );
    }
}
