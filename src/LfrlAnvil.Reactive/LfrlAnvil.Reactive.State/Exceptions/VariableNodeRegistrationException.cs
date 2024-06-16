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

namespace LfrlAnvil.Reactive.State.Exceptions;

/// <summary>
/// Represents an error that occurred during child <see cref="IVariableNode"/> registration.
/// </summary>
public class VariableNodeRegistrationException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="VariableNodeRegistrationException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="parent">Parent node.</param>
    /// <param name="child">Child node.</param>
    public VariableNodeRegistrationException(string message, IVariableNode parent, IVariableNode child)
        : base( message )
    {
        Parent = parent;
        Child = child;
    }

    /// <summary>
    /// Parent node.
    /// </summary>
    public IVariableNode Parent { get; }

    /// <summary>
    /// Child node.
    /// </summary>
    public IVariableNode Child { get; }
}
