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

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Represents an information about a binary operator construct.
/// </summary>
public readonly struct ParsedExpressionBinaryOperatorInfo
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBinaryOperatorInfo"/> instance.
    /// </summary>
    /// <param name="operatorType">Construct's type.</param>
    /// <param name="leftArgumentType">Left argument's type.</param>
    /// <param name="rightArgumentType">Right argument's type.</param>
    public ParsedExpressionBinaryOperatorInfo(Type operatorType, Type leftArgumentType, Type rightArgumentType)
    {
        OperatorType = operatorType;
        LeftArgumentType = leftArgumentType;
        RightArgumentType = rightArgumentType;
    }

    /// <summary>
    /// Construct's type.
    /// </summary>
    public Type OperatorType { get; }

    /// <summary>
    /// Left argument's type.
    /// </summary>
    public Type LeftArgumentType { get; }

    /// <summary>
    /// Right argument's type.
    /// </summary>
    public Type RightArgumentType { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionBinaryOperatorInfo"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{OperatorType.GetDebugString()}({LeftArgumentType.GetDebugString()}, {RightArgumentType.GetDebugString()})";
    }
}
