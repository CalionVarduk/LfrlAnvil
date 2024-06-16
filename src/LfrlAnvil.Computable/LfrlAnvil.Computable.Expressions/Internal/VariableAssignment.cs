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
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Internal.Delegates;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class VariableAssignment
{
    internal VariableAssignment(
        BinaryExpression expression,
        IReadOnlyList<VariableAssignment> usedVariables,
        IReadOnlyList<InlineDelegateCollectionState.Result> delegates)
    {
        Expression = expression;
        UsedVariables = usedVariables;
        Delegates = delegates;
        IsUsed = false;
    }

    internal BinaryExpression Expression { get; }
    internal IReadOnlyList<VariableAssignment> UsedVariables { get; }
    internal IReadOnlyList<InlineDelegateCollectionState.Result> Delegates { get; }
    internal bool IsUsed { get; private set; }
    internal ParameterExpression Variable => ReinterpretCast.To<ParameterExpression>( Expression.Left );

    internal void MarkAsUsed()
    {
        if ( IsUsed )
            return;

        IsUsed = true;
        foreach ( var assignment in UsedVariables )
            assignment.MarkAsUsed();
    }
}
