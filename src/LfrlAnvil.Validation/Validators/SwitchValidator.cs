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

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic switch of object validators.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
/// <typeparam name="TSwitchValue">Object's switch value type used as a validator identifier.</typeparam>
public sealed class SwitchValidator<T, TResult, TSwitchValue> : IValidator<T, TResult>
    where TSwitchValue : notnull
{
    /// <summary>
    /// Creates a new <see cref="SwitchValidator{T,TResult,TSwitchValue}"/> instance.
    /// </summary>
    /// <param name="switchValueSelector">Object's switch value selector.</param>
    /// <param name="validators">Dictionary of validators identified by object's switch values.</param>
    /// <param name="defaultValidator">
    /// Default validator that gets chosen when object's switch value does not exist in the <see cref="Validators"/> dictionary.
    /// </param>
    public SwitchValidator(
        Func<T, TSwitchValue> switchValueSelector,
        IReadOnlyDictionary<TSwitchValue, IValidator<T, TResult>> validators,
        IValidator<T, TResult> defaultValidator)
    {
        SwitchValueSelector = switchValueSelector;
        Validators = validators;
        DefaultValidator = defaultValidator;
    }

    /// <summary>
    /// Object's switch value selector.
    /// </summary>
    public Func<T, TSwitchValue> SwitchValueSelector { get; }

    /// <summary>
    /// Dictionary of validators identified by object's switch values.
    /// </summary>
    public IReadOnlyDictionary<TSwitchValue, IValidator<T, TResult>> Validators { get; }

    /// <summary>
    /// Default validator that gets chosen when object's switch value does not exist in the <see cref="Validators"/> dictionary.
    /// </summary>
    public IValidator<T, TResult> DefaultValidator { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var switchValue = SwitchValueSelector( obj );
        var validator = Validators.GetValueOrDefault( switchValue, DefaultValidator );
        return validator.Validate( obj );
    }
}
