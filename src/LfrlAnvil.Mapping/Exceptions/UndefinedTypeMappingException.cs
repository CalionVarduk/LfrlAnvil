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

namespace LfrlAnvil.Mapping.Exceptions;

/// <summary>
/// Represents an error that occurred due to an undefined mapping from <see cref="SourceType"/> to <see cref="DestinationType"/>.
/// </summary>
public class UndefinedTypeMappingException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="UndefinedTypeMappingException"/> instance.
    /// </summary>
    /// <param name="sourceType">Source type.</param>
    /// <param name="destinationType">Destination type.</param>
    public UndefinedTypeMappingException(Type sourceType, Type destinationType)
        : base( Resources.UndefinedTypeMapping( sourceType, destinationType ) )
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }

    /// <summary>
    /// Source type.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Destination type.
    /// </summary>
    public Type DestinationType { get; }
}
