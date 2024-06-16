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
using System.Text.RegularExpressions;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects a <see cref="Regex"/> to be matched.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsRegexMatchedValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsRegexMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to match.</param>
    /// <param name="failureResult">Failure result.</param>
    public IsRegexMatchedValidator(Regex regex, TResult failureResult)
    {
        Regex = regex;
        FailureResult = failureResult;
    }

    /// <summary>
    /// <see cref="Regex"/> to match.
    /// </summary>
    public Regex Regex { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return Regex.IsMatch( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
