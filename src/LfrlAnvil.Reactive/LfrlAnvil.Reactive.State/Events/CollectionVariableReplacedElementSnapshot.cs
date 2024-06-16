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

namespace LfrlAnvil.Reactive.State.Events;

/// <summary>
/// Represents a generic snapshot of a replaced <see cref="IReadOnlyCollectionVariable"/> element.
/// </summary>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TValidationResult">Validation result type.</typeparam>
public class CollectionVariableReplacedElementSnapshot<TElement, TValidationResult>
    : CollectionVariableElementSnapshot<TElement, TValidationResult>
    where TElement : notnull
{
    /// <summary>
    /// Creates a new <see cref="CollectionVariableReplacedElementSnapshot{TElement,TValidationResult}"/> instance.
    /// </summary>
    /// <param name="previousElement">Replaced element.</param>
    /// <param name="element">Underlying element.</param>
    /// <param name="previousState">
    /// Previous state of the <see cref="CollectionVariableElementSnapshot{TElement,TValidationResult}.Element"/>.
    /// </param>
    /// <param name="newState">
    /// Current state of the <see cref="CollectionVariableElementSnapshot{TElement,TValidationResult}.Element"/>.
    /// </param>
    /// <param name="previousErrors">Collection of validation errors before the change.</param>
    /// <param name="newErrors">Collection of validation errors after the change.</param>
    /// <param name="previousWarnings">Collection of validation warnings before the change.</param>
    /// <param name="newWarnings">Collection of validation warnings after the change.</param>
    public CollectionVariableReplacedElementSnapshot(
        TElement previousElement,
        TElement element,
        CollectionVariableElementState previousState,
        CollectionVariableElementState newState,
        Chain<TValidationResult> previousErrors,
        Chain<TValidationResult> newErrors,
        Chain<TValidationResult> previousWarnings,
        Chain<TValidationResult> newWarnings)
        : base( element, previousState, newState, previousErrors, newErrors, previousWarnings, newWarnings )
    {
        PreviousElement = previousElement;
    }

    /// <summary>
    /// Replaced element.
    /// </summary>
    public TElement PreviousElement { get; }
}
