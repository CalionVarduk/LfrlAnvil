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
using System.Collections.Generic;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic circular range of elements with constant <see cref="IReadOnlyCollection{T}.Count"/>.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IRing<T> : IReadOnlyRing<T>
{
    /// <summary>
    /// Gets or sets an element at the specified position.
    /// </summary>
    /// <param name="index">0-based element position.</param>
    new T? this[int index] { get; set; }

    /// <inheritdoc cref="IReadOnlyRing{T}.WriteIndex" />
    new int WriteIndex { get; set; }

    /// <summary>
    /// Sets a value of the next element of this ring located at the <see cref="WriteIndex"/>
    /// and increments the <see cref="WriteIndex"/> by <b>1</b>.
    /// </summary>
    /// <param name="item">Value to set.</param>
    /// <exception cref="IndexOutOfRangeException">When size of this ring is equal to <b>0</b>.</exception>
    void SetNext(T item);

    /// <summary>
    /// Resets all elements in this ring to default value and sets <see cref="WriteIndex"/> to <b>0</b>.
    /// </summary>
    void Clear();
}
