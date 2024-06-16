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
using LfrlAnvil.Reactive.State.Internal;
using LfrlAnvil.Validation;

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Creates instances of <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> type.
/// </summary>
public static class CollectionVariableRoot
{
    /// <summary>
    /// Creates instances of <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> type without validators.
    /// </summary>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
    public static class WithoutValidators<TValidationResult>
    {
        /// <summary>
        /// Creates a new <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance.
        /// </summary>
        /// <param name="initialElements">Initial collection of elements.</param>
        /// <param name="keySelector">Element's key selector.</param>
        /// <param name="keyComparer">Element key equality comparer.</param>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TElement">Element type.</typeparam>
        /// <returns>New <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance</returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null)
            where TKey : notnull
            where TElement : VariableNode
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
            return CollectionVariableRoot.Create( initialElements, keySelector, keyComparer, validator, validator );
        }

        /// <summary>
        /// Creates a new <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance.
        /// </summary>
        /// <param name="initialElements">Initial collection of elements.</param>
        /// <param name="elementChanges">Element changes that define the collection of current elements.</param>
        /// <param name="keySelector">Element's key selector.</param>
        /// <param name="keyComparer">Element key equality comparer.</param>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TElement">Element type.</typeparam>
        /// <returns>New <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance</returns>
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement>(
            IEnumerable<TElement> initialElements,
            CollectionVariableRootChanges<TKey, TElement> elementChanges,
            Func<TElement, TKey> keySelector,
            IEqualityComparer<TKey>? keyComparer = null)
            where TKey : notnull
            where TElement : VariableNode
        {
            var validator = Validators<TValidationResult>.Pass<ICollectionVariableRootElements<TKey, TElement>>();
            return CollectionVariableRoot.Create( initialElements, elementChanges, keySelector, keyComparer, validator, validator );
        }
    }

    /// <summary>
    /// Creates a new <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="initialElements">Initial collection of elements.</param>
    /// <param name="keySelector">Element's key selector.</param>
    /// <param name="keyComparer">Element key equality comparer.</param>
    /// <param name="errorsValidator">Collection of elements validator that marks result as errors.</param>
    /// <param name="warningsValidator">Collection of elements validator that marks result as warnings.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
    /// <returns>New <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? warningsValidator = null)
        where TKey : notnull
        where TElement : VariableNode
    {
        return new CollectionVariableRoot<TKey, TElement, TValidationResult>(
            initialElements,
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );
    }

    /// <summary>
    /// Creates a new <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="initialElements">Initial collection of elements.</param>
    /// <param name="elementChanges">Element changes that define the collection of current elements.</param>
    /// <param name="keySelector">Element's key selector.</param>
    /// <param name="keyComparer">Element key equality comparer.</param>
    /// <param name="errorsValidator">Collection of elements validator that marks result as errors.</param>
    /// <param name="warningsValidator">Collection of elements validator that marks result as warnings.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TElement">Element type.</typeparam>
    /// <typeparam name="TValidationResult">Validation result type.</typeparam>
    /// <returns>New <see cref="CollectionVariableRoot{TKey,TElement,TValidationResult}"/> instance</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CollectionVariableRoot<TKey, TElement, TValidationResult> Create<TKey, TElement, TValidationResult>(
        IEnumerable<TElement> initialElements,
        CollectionVariableRootChanges<TKey, TElement> elementChanges,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? errorsValidator = null,
        IValidator<ICollectionVariableRootElements<TKey, TElement>, TValidationResult>? warningsValidator = null)
        where TKey : notnull
        where TElement : VariableNode
    {
        return new CollectionVariableRoot<TKey, TElement, TValidationResult>(
            initialElements,
            elementChanges,
            keySelector,
            keyComparer,
            errorsValidator,
            warningsValidator );
    }
}
