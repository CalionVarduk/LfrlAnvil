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
/// Represents a generic collection of objects validator that expects number of elements between a given range.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsElementCountInRangeValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsElementCountInRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Minimum expected number of elements.</param>
    /// <param name="maxCount">Maximum expected number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minCount"/> is not in [<b>0</b>, <paramref name="maxCount"/>] range.
    /// </exception>
    public IsElementCountInRangeValidator(int minCount, int maxCount, TResult failureResult)
    {
        Ensure.IsInRange( minCount, 0, maxCount );
        MinCount = minCount;
        MaxCount = maxCount;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Minimum expected number of elements.
    /// </summary>
    public int MinCount { get; }

    /// <summary>
    /// Maximum expected number of elements.
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
        var count = obj.Count;
        return count >= MinCount && count <= MaxCount ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
