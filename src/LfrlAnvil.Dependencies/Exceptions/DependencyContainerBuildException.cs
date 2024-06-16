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
/// Represents an error that occurred due to failed <see cref="IDependencyContainer"/> build attempt.
/// </summary>
public class DependencyContainerBuildException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="DependencyContainerBuildException"/> instance.
    /// </summary>
    /// <param name="messages">Messages that describe what went wrong.</param>
    public DependencyContainerBuildException(Chain<DependencyContainerBuildMessages> messages)
        : base( Resources.ContainerIsNotConfiguredCorrectly( messages ) )
    {
        Messages = messages;
    }

    /// <summary>
    /// Messages that describe what went wrong.
    /// </summary>
    public Chain<DependencyContainerBuildMessages> Messages { get; }
}
