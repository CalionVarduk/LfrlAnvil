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

namespace LfrlAnvil.Reactive.State;

/// <summary>
/// Represents the state of an element in its collection.
/// </summary>
[Flags]
public enum CollectionVariableElementState : byte
{
    /// <summary>
    /// Specifies that the element has not changed.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Specifies that the element has changed.
    /// </summary>
    Changed = 1,

    /// <summary>
    /// Specifies that the element contains validation errors.
    /// </summary>
    Invalid = 2,

    /// <summary>
    /// Specifies that the element contains validation warnings.
    /// </summary>
    Warning = 4,

    /// <summary>
    /// Specifies that the element has been added.
    /// </summary>
    Added = 8,

    /// <summary>
    /// Specifies that the element has been removed.
    /// </summary>
    Removed = 16,

    /// <summary>
    /// Specifies that the element does not exist.
    /// </summary>
    NotFound = 32
}
