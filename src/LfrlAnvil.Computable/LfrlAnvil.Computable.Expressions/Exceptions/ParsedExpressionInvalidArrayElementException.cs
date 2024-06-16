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

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid array element expression.
/// </summary>
public class ParsedExpressionInvalidArrayElementException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionInvalidArrayElementException"/> instance.
    /// </summary>
    /// <param name="expectedType">Expected element type.</param>
    /// <param name="actualType">Actual element type.</param>
    public ParsedExpressionInvalidArrayElementException(Type expectedType, Type actualType)
        : base( Resources.InvalidArrayElementType( expectedType, actualType ) )
    {
        ExpectedType = expectedType;
        ActualType = actualType;
    }

    /// <summary>
    /// Expected element type.
    /// </summary>
    public Type ExpectedType { get; }

    /// <summary>
    /// Actual element type.
    /// </summary>
    public Type ActualType { get; }
}
