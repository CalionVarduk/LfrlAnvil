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

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to a missing indexer property.
/// </summary>
public class ParsedExpressionUnresolvableIndexerException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionUnresolvableIndexerException"/> instance.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="parameterTypes">Indexer parameter types.</param>
    public ParsedExpressionUnresolvableIndexerException(Type targetType, IReadOnlyList<Type> parameterTypes)
        : base( Resources.UnresolvableIndexer( targetType, parameterTypes ) )
    {
        TargetType = targetType;
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// Target type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Indexer parameter types.
    /// </summary>
    public IReadOnlyList<Type> ParameterTypes { get; }
}
