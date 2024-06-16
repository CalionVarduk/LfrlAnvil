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

namespace LfrlAnvil.Exceptions;

/// <summary>
/// Represents an error that occurred during closed <see cref="Bitmask{T}"/> type initialization.
/// </summary>
public class BitmaskTypeInitializationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="BitmaskTypeInitializationException"/> instance.
    /// </summary>
    /// <param name="type"><see cref="Bitmask{T}"/> type parameter.</param>
    /// <param name="message">Exception's <see cref="Exception.Message"/>.</param>
    public BitmaskTypeInitializationException(Type type, string message)
        : base( message )
    {
        Type = type;
    }

    /// <summary>
    /// Creates a new <see cref="BitmaskTypeInitializationException"/> instance.
    /// </summary>
    /// <param name="type"><see cref="Bitmask{T}"/> type parameter.</param>
    /// <param name="message">Exception's <see cref="Exception.Message"/>.</param>
    /// <param name="innerException">Exception's <see cref="Exception.InnerException"/>.</param>
    public BitmaskTypeInitializationException(Type type, string message, Exception innerException)
        : base( message, innerException )
    {
        Type = type;
    }

    /// <summary>
    /// <see cref="Bitmask{T}"/> type parameter.
    /// </summary>
    public Type Type { get; }
}
