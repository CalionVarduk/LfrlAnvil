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
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a result of validation of an element of a collection.
/// </summary>
/// <typeparam name="TElement">Element type.</typeparam>
/// <typeparam name="TElementResult">Element validation result type.</typeparam>
public readonly struct ElementValidatorResult<TElement, TElementResult>
{
    /// <summary>
    /// Creates a new <see cref="ElementValidatorResult{TElement,TElementResult}"/> instance.
    /// </summary>
    /// <param name="element">Validated element.</param>
    /// <param name="result">Element validation result.</param>
    public ElementValidatorResult(TElement element, Chain<TElementResult> result)
    {
        Element = element;
        Result = result;
    }

    /// <summary>
    /// Validated element.
    /// </summary>
    public TElement Element { get; }

    /// <summary>
    /// Element validation result.
    /// </summary>
    public Chain<TElementResult> Result { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ElementValidatorResult{TElement,TElementResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var resultText = string.Join( Environment.NewLine, Result.Select( static (r, i) => $"{i + 1}. '{r}'" ) );
        return $"{nameof( Element )}: '{Element}', {nameof( Result )}:{Environment.NewLine}{resultText}";
    }
}
