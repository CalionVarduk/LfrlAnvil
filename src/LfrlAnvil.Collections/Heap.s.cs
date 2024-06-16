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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections;

/// <summary>
/// Contains helper <see cref="IHeap{T}"/> methods.
/// </summary>
public static class Heap
{
    /// <summary>
    /// Calculates child's parent index.
    /// </summary>
    /// <param name="childIndex">Child index.</param>
    /// <returns>Child's parent index.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int GetParentIndex(int childIndex)
    {
        return (childIndex - 1) >> 1;
    }

    /// <summary>
    /// Calculates parent's left child index.
    /// </summary>
    /// <param name="parentIndex">Parent index.</param>
    /// <returns>Parent's left child index.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int GetLeftChildIndex(int parentIndex)
    {
        return (parentIndex << 1) + 1;
    }

    /// <summary>
    /// Calculates parent's right child index.
    /// </summary>
    /// <param name="parentIndex">Parent index.</param>
    /// <returns>Parent's right child index.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int GetRightChildIndex(int parentIndex)
    {
        return GetLeftChildIndex( parentIndex ) + 1;
    }
}
