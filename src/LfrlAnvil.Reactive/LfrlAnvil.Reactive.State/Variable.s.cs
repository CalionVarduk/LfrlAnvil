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
        public static Variable<TValue, TValidationResult> Create<TValue>(TValue initialValue)
        {
            return Create( initialValue, initialValue, EqualityComparer<TValue>.Default );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(
            TValue initialValue,
            IEqualityComparer<TValue> comparer)
        {
            return Create( initialValue, initialValue, comparer );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(TValue initialValue, TValue value)
        {
            return Create( initialValue, value, EqualityComparer<TValue>.Default );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(
            TValue initialValue,
            TValue value,
            IEqualityComparer<TValue> comparer)
        {
            var validator = Validators<TValidationResult>.Pass<TValue>();
            return Variable.Create( initialValue, value, comparer, validator, validator );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create(
            initialValue,
            initialValue,
            EqualityComparer<TValue>.Default,
            errorsValidator,
            Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create( initialValue, initialValue, comparer, errorsValidator, Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        TValue value,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create(
            initialValue,
            value,
            EqualityComparer<TValue>.Default,
            errorsValidator,
            Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return Create( initialValue, initialValue, EqualityComparer<TValue>.Default, errorsValidator, warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        TValue value,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator)
    {
        return Create( initialValue, value, comparer, errorsValidator, Validators<TValidationResult>.Pass<TValue>() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return Create( initialValue, initialValue, comparer, errorsValidator, warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        TValue value,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return Create( initialValue, value, EqualityComparer<TValue>.Default, errorsValidator, warningsValidator );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Variable<TValue, TValidationResult> Create<TValue, TValidationResult>(
        TValue initialValue,
        TValue value,
        IEqualityComparer<TValue> comparer,
        IValidator<TValue, TValidationResult> errorsValidator,
        IValidator<TValue, TValidationResult> warningsValidator)
    {
        return new Variable<TValue, TValidationResult>( initialValue, value, comparer, errorsValidator, warningsValidator );
    }
}
