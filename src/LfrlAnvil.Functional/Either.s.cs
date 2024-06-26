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
using LfrlAnvil.Internal;

namespace LfrlAnvil.Functional;

/// <summary>
/// Contains static methods related to <see cref="Either{T1,T2}"/> type.
/// </summary>
public static class Either
{
    /// <summary>
    /// Attempts to extract the underlying first value type from the provided <see cref="Either{T1,T2}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Either{T1,T2}"/> first value type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Either{T1,T2}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingFirstType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Either<,> ) );
        return result.Length == 0 ? null : result[0];
    }

    /// <summary>
    /// Attempts to extract the underlying second value type from the provided <see cref="Either{T1,T2}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract the underlying type from.</param>
    /// <returns>
    /// Underlying <see cref="Either{T1,T2}"/> second value type
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Either{T1,T2}"/> type.
    /// </returns>
    [Pure]
    public static Type? GetUnderlyingSecondType(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Either<,> ) );
        return result.Length == 0 ? null : result[1];
    }

    /// <summary>
    /// Attempts to extract underlying types from the provided <see cref="Either{T1,T2}"/> <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to extract underlying types from.</param>
    /// <returns>
    /// <see cref="Pair{T1,T2}"/> of underlying types
    /// or null when the provided <paramref name="type"/> is not related to the <see cref="Either{T1,T2}"/> type.
    /// </returns>
    [Pure]
    public static Pair<Type, Type>? GetUnderlyingTypes(Type? type)
    {
        var result = UnderlyingType.GetForType( type, typeof( Either<,> ) );
        return result.Length == 0 ? null : Pair.Create( result[0], result[1] );
    }
}
