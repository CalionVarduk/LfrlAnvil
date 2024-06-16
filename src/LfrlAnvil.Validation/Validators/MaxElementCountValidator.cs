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
/// Represents a generic collection of objects validator that expects a maximum number of elements.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class MaxElementCountValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    /// <summary>
    /// Creates a new <see cref="MaxElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="maxCount">Expected maximum number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxCount"/> is less than <b>0</b>.</exception>
    public MaxElementCountValidator(int maxCount, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( maxCount, 0 );
        MaxCount = maxCount;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected maximum number of elements.
    /// </summary>
    public int MaxCount { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        return obj.Count <= MaxCount ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
