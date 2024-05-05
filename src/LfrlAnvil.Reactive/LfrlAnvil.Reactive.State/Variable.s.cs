using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Creates instances of <see cref="Variable{TValue,TValidationResult}"/> type.
/// </summary>
public static class Variable
{
    /// <summary>
    /// Creates instances of <see cref="Variable{TValue,TValidationResult}"/> type without validators.
    /// </summary>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
    public static class WithoutValidators<TValidationResult>
    {
        /// <summary>
        /// Creates a new <see cref="Variable{TValue,TValidationResult}"/> instance.
        /// </summary>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="comparer">Value comparer.</param>
        /// <returns>New <see cref="Variable{TValue,TValidationResult}"/> instance.</returns>
        /// <typeparam name="TValue">Value type.</typeparam>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Variable<TValue, TValidationResult> Create<TValue>(
            TValue initialValue,
            IEqualityComparer<TValue>? comparer = null)
        {
            return Create( initialValue, initialValue, comparer );
        }

        /// <summary>
        /// Creates a new <see cref="Variable{TValue,TValidationResult}"/> instance.
        /// </summary>
        /// <param name="initialValue">Initial value.</param>
        /// <param name="value">Current value.</param>
        /// <param name="comparer">Value comparer.</param>
        /// <returns>New <see cref="Variable{TValue,TValidationResult}"/> instance.</returns>
        /// <typeparam name="TValue">Value type.</typeparam>
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

    /// <summary>
    /// Creates a new <see cref="Variable{TValue,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="initialValue">Initial value.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="errorsValidator">Value validator that marks result as errors.</param>
    /// <param name="warningsValidator">Value validator that marks result as warnings.</param>
    /// <returns>New <see cref="Variable{TValue,TValidationResult}"/> instance.</returns>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
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

    /// <summary>
    /// Creates a new <see cref="Variable{TValue,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="initialValue">Initial value.</param>
    /// <param name="value">Current value.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="errorsValidator">Value validator that marks result as errors.</param>
    /// <param name="warningsValidator">Value validator that marks result as warnings.</param>
    /// <returns>New <see cref="Variable{TValue,TValidationResult}"/> instance.</returns>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
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
