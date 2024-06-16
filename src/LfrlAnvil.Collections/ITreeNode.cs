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

using System.Collections.Generic;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a tree data structure's node.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public interface ITreeNode<out T>
{
    /// <summary>
    /// Underlying value.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Optional parent node.
    /// </summary>
    ITreeNode<T>? Parent { get; }

    /// <summary>
    /// Collection of children nodes.
    /// </summary>
    IReadOnlyList<ITreeNode<T>> Children { get; }
}
