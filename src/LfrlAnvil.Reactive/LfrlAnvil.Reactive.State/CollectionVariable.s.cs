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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Creates instances of <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> type.
/// </summary>
public static class CollectionVariable
{
    /// <summary>
    /// Creates instances of <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> type without validators.
    /// </summary>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
    public static class WithoutValidators<TValidationResult>
    {
        /// <summary>
        /// Creates a new <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.
        /// </summary>
        /// <param name="initialElements">Initial collection of elements.</param>
        /// <param name="keySelector">Element's key selector.</param>
        /// <param name="keyComparer">Element key equality comparer.</param>
        /// <param name="elementComparer">Element equality comparer.</param>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TElement">Element type.</typeparam>
        /// <returns>New <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.</returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TElement>? elementComparer = null)
            where TKey : notnull
            where TElement : notnull
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();
            var elementValidator = Validators<TValidationResult>.Pass<TElement>();
            return CollectionVariable.Create(
                initialElements,
                keySelector,
                keyComparer,
                elementComparer,
                validator,
                validator,
                elementValidator,
                elementValidator );
        }

        /// <summary>
        /// Creates a new <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.
        /// </summary>
        /// <param name="initialElements">Initial collection of elements.</param>
        /// <param name="elements">Current collection of elements.</param>
        /// <param name="keySelector">Element's key selector.</param>
        /// <param name="keyComparer">Element key equality comparer.</param>
        /// <param name="elementComparer">Element equality comparer.</param>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TElement">Element type.</typeparam>
        /// <returns>New <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.</returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            IEnumerable<TElement> elements,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TElement>? elementComparer = null)
            where TKey : notnull
            where TElement : notnull
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableElements<TKey, TElement, TValidationResult>>();
            var elementValidator = Validators<TValidationResult>.Pass<TElement>();
            return CollectionVariable.Create(
                initialElements,
                elements,
                keySelector,
                keyComparer,
                elementComparer,
                validator,
                validator,
                elementValidator,
                elementValidator );
        }
    }

    /// <summary>
    /// Creates a new <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="initialElements">Initial collection of elements.</param>
    /// <param name="keySelector">Element's key selector.</param>
    /// <param name="keyComparer">Element key equality comparer.</param>
    /// <param name="elementComparer">Element equality comparer.</param>
    /// <param name="errorsValidator">Collection of elements validator that marks result as errors.</param>
    /// <param name="warningsValidator">Collection of elements validator that marks result as warnings.</param>
    /// <param name="elementErrorsValidator">Element validator that marks result as errors.</param>
    /// <param name="elementWarningsValidator">Element validator that marks result as warnings.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
    /// <returns>New <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TElement>? elementComparer = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? warningsValidator = null,
        IValidator<TElement, TValidationResult>? elementErrorsValidator = null,
        IValidator<TElement, TValidationResult>? elementWarningsValidator = null)
        where TKey : notnull
        where TElement : notnull
    {
        return new CollectionVariable<TKey, TElement, TValidationResult>(
            initialElements,
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );
    }

    /// <summary>
    /// Creates a new <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="initialElements">Initial collection of elements.</param>
    /// <param name="elements">Current collection of elements.</param>
    /// <param name="keySelector">Element's key selector.</param>
    /// <param name="keyComparer">Element key equality comparer.</param>
    /// <param name="elementComparer">Element equality comparer.</param>
    /// <param name="errorsValidator">Collection of elements validator that marks result as errors.</param>
    /// <param name="warningsValidator">Collection of elements validator that marks result as warnings.</param>
    /// <param name="elementErrorsValidator">Element validator that marks result as errors.</param>
    /// <param name="elementWarningsValidator">Element validator that marks result as warnings.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
    /// <returns>New <see cref="CollectionVariable{TKey,TElement,TValidationResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariable<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        IEnumerable<TElement> elements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TElement>? elementComparer = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableElements<TKey, TElement, TValidationResult>, TValidationResult>? warningsValidator = null,
        IValidator<TElement, TValidationResult>? elementErrorsValidator = null,
        IValidator<TElement, TValidationResult>? elementWarningsValidator = null)
        where TKey : notnull
        where TElement : notnull
    {
        return new CollectionVariable<TKey, TElement, TValidationResult>(
            initialElements,
            elements,
            keySelector,
            keyComparer,
            elementComparer,
            errorsValidator,
            warningsValidator,
            elementErrorsValidator,
            elementWarningsValidator );
    }
}
