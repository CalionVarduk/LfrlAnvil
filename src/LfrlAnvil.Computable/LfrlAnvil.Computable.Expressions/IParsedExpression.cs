﻿// Copyright 2024 Łukasz Furlepa
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
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a parsed expression.
/// </summary>
/// <typeparam name="TArg">Argument type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public interface IParsedExpression<TArg, out TResult>
{
    /// <summary>
    /// Original input that created this expression.
    /// </summary>
    string Input { get; }

    /// <summary>
    /// Body node of this expression.
    /// </summary>
    Expression Body { get; }

    /// <summary>
    /// Parameter node of this expression.
    /// </summary>
    ParameterExpression Parameter { get; }

    /// <summary>
    /// Collection of named unbound arguments. Values for those arguments must be provided during delegate invocation.
    /// </summary>
    ParsedExpressionUnboundArguments UnboundArguments { get; }

    /// <summary>
    /// Collection of bound arguments.
    /// </summary>
    ParsedExpressionBoundArguments<TArg> BoundArguments { get; }

    /// <summary>
    /// Collection of discarded arguments.
    /// </summary>
    ParsedExpressionDiscardedArguments DiscardedArguments { get; }

    /// <summary>
    /// Creates a new <see cref="IParsedExpression{TArg,TResult}"/> instance with bound arguments.
    /// </summary>
    /// <param name="arguments">Collection of arguments to bind.</param>
    /// <returns>New <see cref="IParsedExpression{TArg,TResult}"/> instance.</returns>
    /// <exception cref="ParsedExpressionArgumentBindingException">When an unbound argument does not exist.</exception>
    /// <exception cref="ParsedExpressionCreationException">When expression could not be parsed.</exception>
    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<string, TArg?>> arguments);

    /// <summary>
    /// Creates a new <see cref="IParsedExpression{TArg,TResult}"/> instance with bound arguments.
    /// </summary>
    /// <param name="arguments">Collection of arguments to bind.</param>
    /// <returns>New <see cref="IParsedExpression{TArg,TResult}"/> instance.</returns>
    /// <exception cref="ParsedExpressionArgumentBindingException">When an unbound argument does not exist.</exception>
    /// <exception cref="ParsedExpressionCreationException">When expression could not be parsed.</exception>
    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<string, TArg?>[] arguments);

    /// <summary>
    /// Creates a new <see cref="IParsedExpression{TArg,TResult}"/> instance with bound arguments.
    /// </summary>
    /// <param name="arguments">Collection of arguments to bind.</param>
    /// <returns>New <see cref="IParsedExpression{TArg,TResult}"/> instance.</returns>
    /// <exception cref="ParsedExpressionArgumentBindingException">When an unbound argument does not exist.</exception>
    /// <exception cref="ParsedExpressionCreationException">When expression could not be parsed.</exception>
    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<StringSegment, TArg?>> arguments);

    /// <summary>
    /// Creates a new <see cref="IParsedExpression{TArg,TResult}"/> instance with bound arguments.
    /// </summary>
    /// <param name="arguments">Collection of arguments to bind.</param>
    /// <returns>New <see cref="IParsedExpression{TArg,TResult}"/> instance.</returns>
    /// <exception cref="ParsedExpressionArgumentBindingException">When an unbound argument does not exist.</exception>
    /// <exception cref="ParsedExpressionCreationException">When expression could not be parsed.</exception>
    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<StringSegment, TArg?>[] arguments);

    /// <summary>
    /// Creates a new <see cref="IParsedExpression{TArg,TResult}"/> instance with bound arguments.
    /// </summary>
    /// <param name="arguments">Collection of arguments to bind.</param>
    /// <returns>New <see cref="IParsedExpression{TArg,TResult}"/> instance.</returns>
    /// <exception cref="IndexOutOfRangeException">When an argument index is invalid.</exception>
    /// <exception cref="ParsedExpressionCreationException">When expression could not be parsed.</exception>
    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(IEnumerable<KeyValuePair<int, TArg?>> arguments);

    /// <summary>
    /// Creates a new <see cref="IParsedExpression{TArg,TResult}"/> instance with bound arguments.
    /// </summary>
    /// <param name="arguments">Collection of arguments to bind.</param>
    /// <returns>New <see cref="IParsedExpression{TArg,TResult}"/> instance.</returns>
    /// <exception cref="IndexOutOfRangeException">When an argument index is invalid.</exception>
    /// <exception cref="ParsedExpressionCreationException">When expression could not be parsed.</exception>
    [Pure]
    IParsedExpression<TArg, TResult> BindArguments(params KeyValuePair<int, TArg?>[] arguments);

    /// <summary>
    /// Compiles this expression.
    /// </summary>
    /// <returns>New <see cref="IParsedExpressionDelegate{TArg,TResult}"/> instance.</returns>
    [Pure]
    IParsedExpressionDelegate<TArg, TResult> Compile();
}
