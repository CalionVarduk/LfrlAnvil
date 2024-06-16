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
/// Represents the result of a variable change attempt.
/// </summary>
public enum VariableChangeResult : byte
{
    /// <summary>
    /// Specifies that the variable has changed.
    /// </summary>
    Changed = 0,

    /// <summary>
    /// Specifies that the variable has not changed due to e.g. new value being equal to the current value.
    /// </summary>
    NotChanged = 1,

    /// <summary>
    /// Specifies that the variable has not changed due to being read-only.
    /// </summary>
    ReadOnly = 2
}
