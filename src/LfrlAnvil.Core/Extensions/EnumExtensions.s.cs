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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Enum"/> extension methods.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Creates a new <see cref="Bitmask{T}"/> instance out of the provided <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Source value.</param>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <returns>New <see cref="Bitmask{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Bitmask<T> ToBitmask<T>(this T value)
        where T : struct, Enum
    {
        return new Bitmask<T>( value );
    }
}
