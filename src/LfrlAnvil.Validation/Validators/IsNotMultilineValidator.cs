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
/// Represents a <see cref="String"/> validator that expects a single-line string.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsNotMultilineValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsNotMultilineValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    public IsNotMultilineValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Contains( '\n' ) ? Chain.Create( FailureResult ) : Chain<TResult>.Empty;
    }
}
