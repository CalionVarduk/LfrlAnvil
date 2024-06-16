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

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to an unexpected factory exception.
/// </summary>
public sealed class ParsedExpressionBuilderExceptionError : ParsedExpressionBuilderError
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBuilderExceptionError"/> instance.
    /// </summary>
    /// <param name="exception">Thrown exception.</param>
    public ParsedExpressionBuilderExceptionError(Exception exception)
    {
        Exception = exception;
    }

    internal ParsedExpressionBuilderExceptionError(Exception exception, ParsedExpressionBuilderErrorType type, StringSegment? token)
        : base( type, token )
    {
        Exception = exception;
    }

    /// <summary>
    /// Thrown exception.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderExceptionError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
