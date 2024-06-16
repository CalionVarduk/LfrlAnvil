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
/// Represents an error that occurred during registration of <see cref="TypeMappingConfigurationModule"/> as a sub-module.
/// </summary>
public class InvalidTypeMappingSubmoduleConfigurationException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="InvalidTypeMappingSubmoduleConfigurationException"/> instance.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="paramName">Error parameter name.</param>
    public InvalidTypeMappingSubmoduleConfigurationException(string message, string paramName)
        : base( message, paramName ) { }
}
