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
using System.Collections.Generic;
using System.Globalization;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that can be thrown by compiled expression delegates.
/// </summary>
public class ParsedExpressionInvocationException : Exception
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionInvocationException"/> instance.
    /// </summary>
    /// <param name="format">Exception's message format.</param>
    /// <param name="args">Exception's message arguments.</param>
    public ParsedExpressionInvocationException(string? format, params object?[] args)
        : base( string.Format( CultureInfo.InvariantCulture, format ?? string.Empty, args ) )
    {
        Format = format ?? string.Empty;
        Args = args;
    }

    /// <summary>
    /// Exception's message format.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Exception's message arguments.
    /// </summary>
    public IReadOnlyList<object?> Args { get; }
}
