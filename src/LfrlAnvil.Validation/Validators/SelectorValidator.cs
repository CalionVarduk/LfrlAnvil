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

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that selects a value to validate.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TMember">Validated value type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class SelectorValidator<T, TMember, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="SelectorValidator{T,TMember,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="memberSelector">Validated value selector.</param>
    public SelectorValidator(IValidator<TMember, TResult> validator, Func<T, TMember> memberSelector)
    {
        Validator = validator;
        MemberSelector = memberSelector;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<TMember, TResult> Validator { get; }

    /// <summary>
    /// Validated value selector.
    /// </summary>
    public Func<T, TMember> MemberSelector { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var member = MemberSelector( obj );
        return Validator.Validate( member );
    }
}
