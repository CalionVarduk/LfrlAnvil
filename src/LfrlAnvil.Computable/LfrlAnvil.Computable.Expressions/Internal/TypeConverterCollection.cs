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
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class TypeConverterCollection
{
    internal static readonly TypeConverterCollection Empty = new TypeConverterCollection( null, null, null, int.MaxValue );

    internal TypeConverterCollection(
        Type? targetType,
        ParsedExpressionTypeConverter? genericConstruct,
        IReadOnlyDictionary<Type, ParsedExpressionTypeConverter>? specializedConstructs,
        int precedence)
    {
        TargetType = targetType;
        GenericConstruct = genericConstruct;
        SpecializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal ParsedExpressionTypeConverter? GenericConstruct { get; }
    internal IReadOnlyDictionary<Type, ParsedExpressionTypeConverter>? SpecializedConstructs { get; }
    internal Type? TargetType { get; }
    internal int Precedence { get; }
    internal bool IsEmpty => TargetType is null;

    [Pure]
    internal ParsedExpressionTypeConverter? FindConstruct(Type sourceType)
    {
        if ( SpecializedConstructs is null )
            return GenericConstruct;

        var specialized = SpecializedConstructs.GetValueOrDefault( sourceType );
        return specialized ?? GenericConstruct;
    }
}
