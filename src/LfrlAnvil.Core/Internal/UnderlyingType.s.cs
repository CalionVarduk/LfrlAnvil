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

namespace LfrlAnvil.Internal;

/// <summary>
/// An internal helper class for extracting underlying types from generic classes and structs.
/// </summary>
public static class UnderlyingType
{
    /// <summary>
    /// Returns generic arguments of the provided <paramref name="type"/>,
    /// if it is a generic type closed over open generic <paramref name="targetType"/>.
    /// </summary>
    /// <param name="type">Type to extract generic arguments from.</param>
    /// <param name="targetType">Open generic type that the provided <paramref name="type"/> should close over.</param>
    /// <returns>
    /// Generic arguments of <paramref name="type"/>,
    /// or an empty array when <paramref name="type"/> is null or does not close over <paramref name="targetType"/>.
    /// </returns>
    [Pure]
    public static Type[] GetForType(Type? type, Type? targetType)
    {
        if ( type is null || ! type.IsGenericType )
            return Type.EmptyTypes;

        var openType = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
        return openType == targetType ? type.GetGenericArguments() : Type.EmptyTypes;
    }
}
