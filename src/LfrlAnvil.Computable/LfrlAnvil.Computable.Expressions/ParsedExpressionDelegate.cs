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

/// <inheritdoc cref="IParsedExpressionDelegate{TArg,TResult}" />
public sealed class ParsedExpressionDelegate<TArg, TResult> : IParsedExpressionDelegate<TArg, TResult>
{
    internal ParsedExpressionDelegate(Func<TArg?[], TResult> @delegate, ParsedExpressionUnboundArguments arguments)
    {
        Delegate = @delegate;
        Arguments = arguments;
    }

    /// <inheritdoc />
    public Func<TArg?[], TResult> Delegate { get; }

    /// <inheritdoc />
    public ParsedExpressionUnboundArguments Arguments { get; }

    /// <inheritdoc />
    [Pure]
    public TResult Invoke(params TArg?[] arguments)
    {
        if ( Arguments.Count != arguments.Length )
            throw new InvalidParsedExpressionArgumentCountException( arguments.Length, Arguments.Count, nameof( arguments ) );

        return Delegate( arguments );
    }
}
