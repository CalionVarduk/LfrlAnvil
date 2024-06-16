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

namespace LfrlAnvil.Reactive.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid argument type.
/// </summary>
public class InvalidArgumentTypeException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidArgumentTypeException"/> instance.
    /// </summary>
    /// <param name="argument">Invalid argument.</param>
    /// <param name="expectedType">Expected type.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public InvalidArgumentTypeException(object? argument, Type expectedType, string paramName)
        : base( Resources.InvalidArgumentType( argument, expectedType ), paramName )
    {
        Argument = argument;
        ExpectedType = expectedType;
    }

    /// <summary>
    /// Invalid argument.
    /// </summary>
    public object? Argument { get; }

    /// <summary>
    /// Expected type.
    /// </summary>
    public Type ExpectedType { get; }
}
