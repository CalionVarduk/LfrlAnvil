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
/// Represents an error that occurred during an attempt to create an <see cref="IParsedExpressionFactory"/> instance.
/// </summary>
public class ParsedExpressionFactoryBuilderException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFactoryBuilderException"/> instance.
    /// </summary>
    /// <param name="messages">Collection of error messages.</param>
    public ParsedExpressionFactoryBuilderException(Chain<string> messages)
        : base( Resources.FailedExpressionFactoryCreation( messages ) )
    {
        Messages = messages;
    }

    /// <summary>
    /// Collection of error messages.
    /// </summary>
    public Chain<string> Messages { get; }
}
