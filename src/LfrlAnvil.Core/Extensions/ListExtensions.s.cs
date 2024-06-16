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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IList{T}"/> extension methods.
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Swaps two items in the provided list.
    /// </summary>
    /// <param name="list">Source list.</param>
    /// <param name="index1">Index of the first item to swap.</param>
    /// <param name="index2">Index of the second item to swap.</param>
    /// <typeparam name="T">List item type.</typeparam>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void SwapItems<T>(this IList<T> list, int index1, int index2)
    {
        (list[index2], list[index1]) = (list[index1], list[index2]);
    }

    /// <summary>
    /// Removes the last item from the provided list.
    /// </summary>
    /// <param name="list">Source list.</param>
    /// <typeparam name="T">List item type.</typeparam>
    /// <exception cref="ArgumentOutOfRangeException">When list is empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void RemoveLast<T>(this IList<T> list)
    {
        list.RemoveAt( list.Count - 1 );
    }
}
