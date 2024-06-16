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

namespace LfrlAnvil.Collections.Extensions;

/// <summary>
/// Contains <see cref="GraphDirection"/> extension methods.
/// </summary>
public static class GraphDirectionExtensions
{
    /// <summary>
    /// Inverts the provided <paramref name="direction"/>.
    /// Returns <see cref="GraphDirection.Out"/> for <see cref="GraphDirection.In"/>,
    /// <see cref="GraphDirection.In"/> for <see cref="GraphDirection.Out"/>
    /// and <see cref="GraphDirection.Both"/> for <see cref="GraphDirection.Both"/>.
    /// </summary>
    /// <param name="direction">Direction to invert.</param>
    /// <returns>Inverted <paramref name="direction"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static GraphDirection Invert(this GraphDirection direction)
    {
        return ( GraphDirection )((( byte )(direction & GraphDirection.In) << 1) | (( byte )(direction & GraphDirection.Out) >> 1));
    }

    /// <summary>
    /// Sanitizes the provided <paramref name="direction"/> by computing bitwise and with <see cref="GraphDirection.Both"/>.
    /// </summary>
    /// <param name="direction">Direction to sanitize.</param>
    /// <returns>Sanitized <paramref name="direction"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static GraphDirection Sanitize(this GraphDirection direction)
    {
        return direction & GraphDirection.Both;
    }
}
