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
/// Represents a <see cref="String"/> validator that expects an exact <see cref="String.Length"/>.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsLengthExactValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="length">Expected exact <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="length"/> is less than <b>0</b>.</exception>
    public IsLengthExactValidator(int length, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( length, 0 );
        Length = length;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected exact <see cref="String.Length"/>.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length == Length ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
