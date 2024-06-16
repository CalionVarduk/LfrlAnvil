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
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a compiled parsed expression.
/// </summary>
/// <typeparam name="TArg">Argument type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IParsedExpressionDelegate<in TArg, out TResult>
{
    /// <summary>
    /// Underlying delegate.
    /// </summary>
    Func<TArg?[], TResult> Delegate { get; }

    /// <summary>
    /// Collection of named unbound arguments. Values for those arguments must be provided during delegate invocation.
    /// </summary>
    ParsedExpressionUnboundArguments Arguments { get; }

    /// <summary>
    /// Invokes this delegate.
    /// </summary>
    /// <param name="arguments">Argument values.</param>
    /// <returns>Invocation result.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentCountException">
    /// When not all <see cref="Arguments"/> received their value or too many values were provided.
    /// </exception>
    [Pure]
    TResult Invoke(params TArg?[] arguments);
}
