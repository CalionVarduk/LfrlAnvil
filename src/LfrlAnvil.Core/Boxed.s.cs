﻿// Copyright 2024 Łukasz Furlepa
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

namespace LfrlAnvil;

/// <summary>
/// Contains boxed instances of value types.
/// </summary>
public static class Boxed
{
    /// <summary>
    /// Represents boxed <see cref="Boolean"/> equal to <b>true</b>.
    /// </summary>
    public static readonly object True = true;

    /// <summary>
    /// Represents boxed <see cref="Boolean"/> equal to <b>false</b>.
    /// </summary>
    public static readonly object False = false;

    /// <summary>
    /// Gets a stored boxed representation of the provided <see cref="Boolean"/> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to get.</param>
    /// <returns>Boxed <paramref name="value"/> representation.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static object GetBool(bool value)
    {
        return value ? True : False;
    }
}
