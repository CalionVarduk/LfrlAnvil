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

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents a source of variable's value change.
/// </summary>
public enum VariableChangeSource : byte
{
    /// <summary>
    /// Specifies manual change invocation.
    /// </summary>
    Change = 0,

    /// <summary>
    /// Specifies manual change attempt invocation.
    /// </summary>
    TryChange = 1,

    /// <summary>
    /// Specifies variable refresh.
    /// </summary>
    Refresh = 2,

    /// <summary>
    /// Specifies variable reset.
    /// </summary>
    Reset = 3,

    /// <summary>
    /// Specifies variable read-only change.
    /// </summary>
    SetReadOnly = 4,

    /// <summary>
    /// Specifies child node change.
    /// </summary>
    ChildNode = 5
}
