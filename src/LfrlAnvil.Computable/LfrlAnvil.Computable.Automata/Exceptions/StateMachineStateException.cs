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

namespace LfrlAnvil.Computable.Automata.Exceptions;

/// <summary>
/// Represents an error related to state retrieval from a state machine.
/// </summary>
public class StateMachineStateException : ArgumentException
{
    /// <summary>
    /// Creates a new <see cref="StateMachineStateException"/> instance.
    /// </summary>
    /// <param name="message">Exception's message.</param>
    /// <param name="paramName">Exception's parameter name.</param>
    public StateMachineStateException(string message, string paramName)
        : base( message, paramName ) { }
}
