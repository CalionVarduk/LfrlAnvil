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
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation
/// due to an exception thrown by a construct.
/// </summary>
public sealed class ParsedExpressionBuilderConstructError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderConstructError(
        ParsedExpressionBuilderErrorType type,
        object construct,
        StringSegment? token = null,
        Exception? exception = null)
        : base( type, token )
    {
        Construct = construct;
        Exception = exception;
    }

    /// <summary>
    /// Construct that has thrown the <see cref="Exception"/>.
    /// </summary>
    public object Construct { get; }

    /// <summary>
    /// Thrown exception.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderConstructError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var headerText = $"{base.ToString()}, construct of type {Construct.GetType().GetDebugString()}";
        if ( Exception is null )
            return headerText;

        return $"{headerText}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
