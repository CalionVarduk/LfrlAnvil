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
/// Represents a generic object validator that maps result of an underlying validator to a different type.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TSourceResult">Underlying validator's result type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class MappedValidator<T, TSourceResult, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="MappedValidator{T,TSourceResult,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="resultMapper">Underlying validator's result mapper.</param>
    public MappedValidator(
        IValidator<T, TSourceResult> validator,
        Func<Chain<TSourceResult>, Chain<TResult>> resultMapper)
    {
        Validator = validator;
        ResultMapper = resultMapper;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<T, TSourceResult> Validator { get; }

    /// <summary>
    /// Underlying validator's result mapper.
    /// </summary>
    public Func<Chain<TSourceResult>, Chain<TResult>> ResultMapper { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var result = Validator.Validate( obj );
        var mappedResult = ResultMapper( result );
        return mappedResult;
    }
}
