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
/// Represents an error that occurred due to an invalid number of arguments during delegate invocation.
/// </summary>
public class InvalidParsedExpressionArgumentCountException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidParsedExpressionArgumentCountException"/> instance.
    /// </summary>
    /// <param name="actual">Provided number of arguments.</param>
    /// <param name="expected">Expected number of arguments.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public InvalidParsedExpressionArgumentCountException(int actual, int expected, string paramName)
        : base( Resources.InvalidExpressionArgumentCount( actual, expected, paramName ), paramName )
    {
        Actual = actual;
        Expected = expected;
    }

    /// <summary>
    /// Provided number of arguments.
    /// </summary>
    public int Actual { get; }

    /// <summary>
    /// Expected number of arguments.
    /// </summary>
    public int Expected { get; }
}
