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
/// Represents a result of <see cref="OrCompositeValidator{T,TResult}"/> validator.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public readonly struct OrValidatorResult<TResult>
{
    /// <summary>
    /// Creates a new <see cref="OrValidatorResult{TResult}"/> instance.
    /// </summary>
    /// <param name="result">Underlying result.</param>
    public OrValidatorResult(Chain<TResult> result)
    {
        Result = result;
    }

    /// <summary>
    /// Underlying result.
    /// </summary>
    public Chain<TResult> Result { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="OrValidatorResult{TResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return string.Join( $" or{Environment.NewLine}", Result.Select( static r => $"'{r}'" ) );
    }
}
