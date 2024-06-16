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

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator which uses specified <see cref="DefaultValue"/> when validated object is null.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class ForDefaultIfNullRefValidator<T, TResult> : IValidator<T?, TResult>
    where T : class
{
    /// <summary>
    /// Creates a new <see cref="ForDefaultIfNullRefValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="defaultValue">Default value to use instead of a null object.</param>
    public ForDefaultIfNullRefValidator(IValidator<T, TResult> validator, T defaultValue)
    {
        Validator = validator;
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<T, TResult> Validator { get; }

    /// <summary>
    /// Default value to use instead of a null object.
    /// </summary>
    public T DefaultValue { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T? obj)
    {
        return Validator.Validate( obj ?? DefaultValue );
    }
}
