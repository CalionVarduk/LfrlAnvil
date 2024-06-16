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

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator for a collection of elements where each element is validated separately.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <typeparam name="TElementResult">Element result type.</typeparam>
public sealed class ForEachValidator<T, TElementResult> : IValidator<IReadOnlyCollection<T>, ElementValidatorResult<T, TElementResult>>
{
    /// <summary>
    /// Creates a new <see cref="ForEachValidator{T,TElementResult}"/> instance.
    /// </summary>
    /// <param name="elementValidator">Underlying element validator.</param>
    public ForEachValidator(IValidator<T, TElementResult> elementValidator)
    {
        ElementValidator = elementValidator;
    }

    /// <summary>
    /// Underlying element validator.
    /// </summary>
    public IValidator<T, TElementResult> ElementValidator { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<ElementValidatorResult<T, TElementResult>> Validate(IReadOnlyCollection<T> obj)
    {
        var result = Chain<ElementValidatorResult<T, TElementResult>>.Empty;
        foreach ( var element in obj )
        {
            var elementResult = ElementValidator.Validate( element );
            if ( elementResult.Count > 0 )
                result = result.Extend( new ElementValidatorResult<T, TElementResult>( element, elementResult ) );
        }

        return result;
    }
}
