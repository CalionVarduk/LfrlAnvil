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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace LfrlAnvil.Expressions;

/// <summary>
/// Represents an expression tree rewriter capable of replacing <see cref="ParameterExpression"/> nodes by position.
/// </summary>
public class ExpressionParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression[] _parametersToReplace;
    private readonly Expression[] _replacements;

    /// <summary>
    /// Creates a new <see cref="ExpressionParameterReplacer"/> instance.
    /// </summary>
    /// <param name="parametersToReplace">Collection of <see cref="ParameterExpression"/> nodes to replace.</param>
    /// <param name="replacements">Collection of replacement <see cref="Expression"/> nodes.</param>
    /// <remarks>
    /// <see cref="ParameterExpression"/> nodes that do not exist in the <paramref name="parametersToReplace"/> collection will be ignored.
    /// Replacement nodes are chosen by index, that is, if <see cref="ParameterExpression"/> exists
    /// in the <paramref name="parametersToReplace"/> collection, then the index of its first occurrence is used to find its replacement
    /// in the <paramref name="replacements"/> collection.
    /// </remarks>
    public ExpressionParameterReplacer(ParameterExpression[] parametersToReplace, Expression[] replacements)
    {
        _parametersToReplace = parametersToReplace;
        _replacements = replacements;
    }

    /// <inheritdoc />
    [return: NotNullIfNotNull( "node" )]
    public override Expression? Visit(Expression? node)
    {
        if ( node is null || node.NodeType != ExpressionType.Parameter || node is not ParameterExpression parameterExpression )
            return base.Visit( node );

        var index = Array.IndexOf( _parametersToReplace, parameterExpression );
        return index >= 0 && index < _replacements.Length
            ? _replacements[index]
            : base.Visit( node );
    }
}
