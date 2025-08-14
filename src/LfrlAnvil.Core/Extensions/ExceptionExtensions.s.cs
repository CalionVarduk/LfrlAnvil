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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Exception"/> extension methods.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Rethrows the provided <paramref name="exception"/>.
    /// </summary>
    /// <param name="exception">Exception to rethrow.</param>
    /// <returns>This method does not return.</returns>
    /// <remarks>See <see cref="ExceptionDispatchInfo.Throw(Exception)"/> for more information.</remarks>
    [DoesNotReturn]
    [StackTraceHidden]
    public static Exception Rethrow(this Exception exception)
    {
        ExceptionDispatchInfo.Throw( exception );
        return exception;
    }

    /// <summary>
    /// Consolidates a collection of <paramref name="exceptions"/> into a single exception.
    /// </summary>
    /// <param name="exceptions">Collection of exceptions to consolidate.</param>
    /// <returns>
    /// New <see cref="AggregateException"/> when <paramref name="exceptions"/> contains more than one element
    /// or the sole exception when <paramref name="exceptions"/> contains exactly one element
    /// or <b>null</b> when <paramref name="exceptions"/> is empty.
    /// </returns>
    [Pure]
    public static Exception? Consolidate(this IEnumerable<Exception> exceptions)
    {
        var materialized = exceptions.Materialize();
        return materialized.Count switch
        {
            0 => null,
            1 => materialized.Single(),
            _ => new AggregateException( materialized )
        };
    }
}
