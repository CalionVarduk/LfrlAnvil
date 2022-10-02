using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

public static class Variable
{
    public static class WithoutValidators<TValidationResult>
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(
            TValue initialValue,
            IEqualityComparer<TValue>? comparer = null)
        {
            return Create( initialValue, initialValue, comparer );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(
            TValue initialValue,
            TValue value,
            IEqualityComparer<TValue>? comparer = null)
        {
            var validator = Validators<TValidationResult>.Pass<TValue>();
            return Variable.Create( initialValue, value, comparer, validator, validator );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        IEqualityComparer<TValue>? comparer = null,
        IValidator<TValue, TValidationResult>? errorsValidator = null,
        IValidator<TValue, TValidationResult>? warningsValidator = null)
    {
        return new Variable<TValue, TValidationResult>( initialValue, comparer, errorsValidator, warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        TValue value,
        IEqualityComparer<TValue>? comparer = null,
        IValidator<TValue, TValidationResult>? errorsValidator = null,
        IValidator<TValue, TValidationResult>? warningsValidator = null)
    {
        return new Variable<TValue, TValidationResult>( initialValue, value, comparer, errorsValidator, warningsValidator );
    }
}
