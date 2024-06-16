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

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a generic collection variable.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public interface ICollectionVariable<TKey, TElement, TValidationResult> : IReadOnlyCollectionVariable<TKey, TElement, TValidationResult>
    where TKey : notnull
    where TElement : notnull
{
    /// <summary>
    /// Attempts to change the <see cref="IReadOnlyCollectionVariable{TKey,TElement}.Elements"/>.
    /// Elements that exist and are considered equal are registered as refreshed.
    /// </summary>
    /// <param name="elements">Elements to set.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult TryChange(IEnumerable<TElement> elements);

    /// <summary>
    /// Changes the <see cref="IReadOnlyCollectionVariable{TKey,TElement}.Elements"/>.
    /// Elements that exist and are considered equal are registered as replaced.
    /// </summary>
    /// <param name="elements">Elements to set.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Change(IEnumerable<TElement> elements);

    /// <summary>
    /// Adds an element to this collection.
    /// </summary>
    /// <param name="element">Element to add.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Add(TElement element);

    /// <summary>
    /// Adds a collection of elements to this collection.
    /// </summary>
    /// <param name="elements">Collection of elements to add.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Add(IEnumerable<TElement> elements);

    /// <summary>
    /// Attempts to replace an existing element in this collection, unless it and its replacement are considered to be equal.
    /// </summary>
    /// <param name="element">Element's replacement.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult TryReplace(TElement element);

    /// <summary>
    /// Replaces a collection of existing elements in this collection, unless they and their replacements are considered to be equal.
    /// </summary>
    /// <param name="elements">Collection of element replacements.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult TryReplace(IEnumerable<TElement> elements);

    /// <summary>
    /// Replaces an existing element in this collection.
    /// </summary>
    /// <param name="element">Element's replacement.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Replace(TElement element);

    /// <summary>
    /// Replaces a collection of existing elements in this collection.
    /// </summary>
    /// <param name="elements">Collection of element replacements.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Replace(IEnumerable<TElement> elements);

    /// <summary>
    /// Adds or attempts to replace an element in this collection.
    /// Existing elements will not be replaced when they and their replacements are considered to be equal.
    /// </summary>
    /// <param name="element">Element to add or to replace with.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult AddOrTryReplace(TElement element);

    /// <summary>
    /// Adds or attempts to replace a collection of elements in this collection.
    /// Existing elements will not be replaced when they and their replacements are considered to be equal.
    /// </summary>
    /// <param name="elements">Collection of elements to add or to replace with.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult AddOrTryReplace(IEnumerable<TElement> elements);

    /// <summary>
    /// Adds or replaces an element in this collection.
    /// </summary>
    /// <param name="element">Element to add or to replace with.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult AddOrReplace(TElement element);

    /// <summary>
    /// Adds or replaces a collection of elements in this collection.
    /// </summary>
    /// <param name="elements">Collection of elements to add or to replace with.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult AddOrReplace(IEnumerable<TElement> elements);

    /// <summary>
    /// Removes an element associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Remove(TKey key);

    /// <summary>
    /// Removes elements associated with the provided collection of <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Collection of keys to remove.</param>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Remove(IEnumerable<TKey> keys);

    /// <summary>
    /// Removes all elements from this collection.
    /// </summary>
    /// <returns>Result of this change attempt.</returns>
    VariableChangeResult Clear();

    /// <summary>
    /// Refreshes this variable.
    /// </summary>
    void Refresh();

    /// <summary>
    /// Refreshes an element associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to refresh.</param>
    void Refresh(TKey key);

    /// <summary>
    /// Refreshes elements associated with the provided collection <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Collection of keys to refresh.</param>
    void Refresh(IEnumerable<TKey> keys);

    /// <summary>
    /// Refreshes this variable's validation.
    /// </summary>
    void RefreshValidation();

    /// <summary>
    /// Refreshes validation of an element associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to refresh.</param>
    void RefreshValidation(TKey key);

    /// <summary>
    /// Refreshes validation of elements associated with the provided collection <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Collection of keys to refresh.</param>
    void RefreshValidation(IEnumerable<TKey> keys);

    /// <summary>
    /// Removes all errors and warnings from this variable.
    /// </summary>
    void ClearValidation();

    /// <summary>
    /// Clears validation of an element associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to refresh.</param>
    void ClearValidation(TKey key);

    /// <summary>
    /// Clears validation of elements associated with the provided collection <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">Collection of keys to refresh.</param>
    void ClearValidation(IEnumerable<TKey> keys);
}
