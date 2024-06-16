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

namespace LfrlAnvil.Computable.Automata;

/// <summary>
/// Represents the type of state machine's node.
/// </summary>
[Flags]
public enum StateMachineNodeType : byte
{
    /// <summary>
    /// Specifies that the node is a standard node.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Specifies that the node is an initial node, at which the state machine starts.
    /// </summary>
    Initial = 1,

    /// <summary>
    /// Specifies that the node is marked as accept or final node.
    /// </summary>
    Accept = 2,

    /// <summary>
    /// Specifies that no <see cref="Accept"/> node can be reached from this node.
    /// </summary>
    Dead = 4
}
