// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
