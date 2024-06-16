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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class FunctionCollection
{
    internal static readonly FunctionCollection Empty =
        new FunctionCollection( new Dictionary<FunctionSignatureKey, ParsedExpressionFunction>() );

    internal FunctionCollection(IReadOnlyDictionary<FunctionSignatureKey, ParsedExpressionFunction> functions)
    {
        Functions = functions;
    }

    internal IReadOnlyDictionary<FunctionSignatureKey, ParsedExpressionFunction> Functions { get; }

    [Pure]
    internal ParsedExpressionFunction? FindConstruct(IReadOnlyList<Expression> parameters)
    {
        return Functions.GetValueOrDefault( new FunctionSignatureKey( parameters ) );
    }
}
