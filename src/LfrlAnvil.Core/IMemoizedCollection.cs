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

namespace LfrlAnvil;

/// <summary>
/// Represents a memoized collection, that is a collection that is lazily materialized.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IMemoizedCollection<T> : IReadOnlyCollection<T>
{
    /// <summary>
    /// <see cref="Lazy{T}"/> collection source.
    /// </summary>
    Lazy<IReadOnlyCollection<T>> Source { get; }

    /// <summary>
    /// Specifies whether or not this collection has been materialized.
    /// </summary>
    bool IsMaterialized { get; }
}
