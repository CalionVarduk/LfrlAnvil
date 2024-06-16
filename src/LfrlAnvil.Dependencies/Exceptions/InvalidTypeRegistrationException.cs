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

namespace LfrlAnvil.Dependencies.Exceptions;

/// <summary>
/// Represents an error that occurred due to an invalid dependency or implementor type registration attempt.
/// </summary>
public class InvalidTypeRegistrationException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidTypeRegistrationException"/> instance.
    /// </summary>
    /// <param name="type">Invalid type.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public InvalidTypeRegistrationException(Type type, string paramName)
        : base( Resources.InvalidTypeRegistration( type ), paramName )
    {
        Type = type;
    }

    /// <summary>
    /// Invalid type.
    /// </summary>
    public Type Type { get; }
}
