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
        public static Variable<TValue, TValidationResult> Create<TValue>(TValue originalValue)
        {
            return Create( originalValue, originalValue, EqualityComparer<TValue>.Default );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(
            TValue originalValue,
            IEqualityComparer<TValue> comparer)
        {
            return Create( originalValue, originalValue, comparer );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(TValue originalValue, TValue value)
        {
            return Create( originalValue, value, EqualityComparer<TValue>.Default );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(
            TValue originalValue,
            TValue value,
            IEqualityComparer<TValue> comparer)
        {
            var validator = Validators<TValidationResult>.Pass<TValue>();
            return Variable.Create( originalValue, value, comparer, validator, validator );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create(
            originalValue,
            originalValue,
            EqualityComparer<TValue>.Default,
            errorsValidator,
            Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create( originalValue, originalValue, comparer, errorsValidator, Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        TValue value,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create(
            originalValue,
            value,
            EqualityComparer<TValue>.Default,
            errorsValidator,
            Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return Create( originalValue, originalValue, EqualityComparer<TValue>.Default, errorsValidator, warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        TValue value,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create( originalValue, value, comparer, errorsValidator, Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return Create( originalValue, originalValue, comparer, errorsValidator, warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        TValue value,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return Create( originalValue, value, EqualityComparer<TValue>.Default, errorsValidator, warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue originalValue,
        TValue value,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return new Variable<TValue, TValidationResult>( originalValue, value, comparer, errorsValidator, warningsValidator );
    }
}
