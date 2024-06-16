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
using System.Linq.Expressions;

namespace LfrlAnvil.Expressions;

/// <summary>
/// Represents an expression tree rewriter capable of replacing <see cref="ParameterExpression"/> nodes
/// by their <see cref="ParameterExpression.Name"/>.
/// </summary>
/// <remarks>Parameters without a name are ignored.</remarks>
public class ExpressionParameterByNameReplacer : ExpressionVisitor
{
    private readonly IReadOnlyDictionary<string, Expression> _parametersToReplace;

    /// <summary>
    /// Creates a new <see cref="ExpressionParameterByNameReplacer"/> instance.
    /// </summary>
    /// <param name="parametersToReplace">Collection of (parameter-name, replacement-node) entries.</param>
    public ExpressionParameterByNameReplacer(IReadOnlyDictionary<string, Expression> parametersToReplace)
    {
        _parametersToReplace = parametersToReplace;
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull( "node" )]
    public override Expression? Visit(Expression? node)
    {
        if ( node is null
            || node.NodeType != ExpressionType.Parameter
            || node is not ParameterExpression parameterExpression
            || parameterExpression.Name is null )
            return base.Visit( node );

        return _parametersToReplace.TryGetValue( parameterExpression.Name, out var expression )
            ? expression
            : base.Visit( node );
    }
}
