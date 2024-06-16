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
/// Represents an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation due to an invalid result type.
/// </summary>
public sealed class ParsedExpressionBuilderResultTypeError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderResultTypeError(
        ParsedExpressionBuilderErrorType type,
        Type resultType,
        Type expectedType)
        : base( type )
    {
        ResultType = resultType;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Actual result type.
    /// </summary>
    public Type ResultType { get; }

    /// <summary>
    /// Expected result type.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBuilderResultTypeError"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{base.ToString()}, result type: {ResultType.GetDebugString()}, expected type: {ExpectedType.GetDebugString()}";
    }
}
