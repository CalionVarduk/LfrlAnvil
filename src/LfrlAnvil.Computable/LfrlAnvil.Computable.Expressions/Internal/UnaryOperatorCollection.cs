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

internal sealed class UnaryOperatorCollection
{
    internal static readonly UnaryOperatorCollection Empty = new UnaryOperatorCollection( null, null, int.MaxValue );

    internal UnaryOperatorCollection(
        ParsedExpressionUnaryOperator? genericConstruct,
        IReadOnlyDictionary<Type, ParsedExpressionTypedUnaryOperator>? specializedConstructs,
        int precedence)
    {
        GenericConstruct = genericConstruct;
        SpecializedConstructs = specializedConstructs;
        Precedence = precedence;
    }

    internal ParsedExpressionUnaryOperator? GenericConstruct { get; }
    internal IReadOnlyDictionary<Type, ParsedExpressionTypedUnaryOperator>? SpecializedConstructs { get; }
    internal int Precedence { get; }
    internal bool IsEmpty => GenericConstruct is null && SpecializedConstructs is null;

    [Pure]
    internal ParsedExpressionUnaryOperator? FindConstruct(Type argumentType)
    {
        if ( SpecializedConstructs is null )
            return GenericConstruct;

        var specialized = SpecializedConstructs.GetValueOrDefault( argumentType );
        return specialized ?? GenericConstruct;
    }
}
