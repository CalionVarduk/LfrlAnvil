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
using LfrlAnvil.Computable.Expressions.Errors;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred during an attempt to create an <see cref="IParsedExpression{TArg,TResult}"/> instance.
/// </summary>
public class ParsedExpressionCreationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCreationException"/> instance.
    /// </summary>
    /// <param name="input">Parsed input.</param>
    /// <param name="errors">Collection of underlying errors.</param>
    public ParsedExpressionCreationException(string input, Chain<ParsedExpressionBuilderError> errors)
        : base( Resources.FailedExpressionCreation( input, errors ) )
    {
        Input = input;
        Errors = errors;
    }

    /// <summary>
    /// Parsed input.
    /// </summary>
    public string Input { get; }

    /// <summary>
    /// Collection of underlying errors.
    /// </summary>
    public Chain<ParsedExpressionBuilderError> Errors { get; }
}
