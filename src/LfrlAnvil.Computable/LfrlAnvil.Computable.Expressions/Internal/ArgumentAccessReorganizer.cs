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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ArgumentAccessReorganizer : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;
    private readonly IReadOnlyDictionary<int, Expression> _argumentAccessExpressions;
    private readonly int _oldArgumentCount;

    internal ArgumentAccessReorganizer(
        ParameterExpression parameter,
        IReadOnlyDictionary<int, Expression> argumentAccessExpressions,
        int oldArgumentCount)
    {
        _parameter = parameter;
        _argumentAccessExpressions = argumentAccessExpressions;
        _oldArgumentCount = oldArgumentCount;
    }

    [Pure]
    [return: NotNullIfNotNull( "node" )]
    public override Expression? Visit(Expression? node)
    {
        if ( ! node.TryGetArgumentAccessIndex( _parameter, _oldArgumentCount, out var oldIndex ) )
            return base.Visit( node );

        var result = _argumentAccessExpressions[oldIndex];
        return result;
    }
}
