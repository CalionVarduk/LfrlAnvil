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
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents parameters for creating an <see cref="IParsedExpressionNumberParser"/> instance.
/// </summary>
public readonly struct ParsedExpressionNumberParserParams
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionNumberParserParams"/> instance.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <param name="argumentType">Argument type.</param>
    /// <param name="resultType">Result type.</param>
    public ParsedExpressionNumberParserParams(
        ParsedExpressionFactoryInternalConfiguration configuration,
        Type argumentType,
        Type resultType)
    {
        Configuration = configuration;
        ArgumentType = argumentType;
        ResultType = resultType;
    }

    /// <summary>
    /// Underling configuration.
    /// </summary>
    public ParsedExpressionFactoryInternalConfiguration Configuration { get; }

    /// <summary>
    /// Argument type.
    /// </summary>
    public Type ArgumentType { get; }

    /// <summary>
    /// Result type.
    /// </summary>
    public Type ResultType { get; }
}
