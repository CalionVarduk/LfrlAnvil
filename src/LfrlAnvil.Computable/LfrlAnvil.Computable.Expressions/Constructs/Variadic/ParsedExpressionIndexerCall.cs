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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

/// <summary>
/// Represents an indexer call construct.
/// </summary>
public sealed class ParsedExpressionIndexerCall : ParsedExpressionVariadicFunction
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionIndexerCall"/> instance.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <param name="foldConstantsWhenPossible">
    /// Specifies whether or not indexer invocations with all parameters being constant and target being constant
    /// should be resolved immediately as constant expression. Equal to <b>true</b> by default.
    /// </param>
    public ParsedExpressionIndexerCall(
        ParsedExpressionFactoryInternalConfiguration configuration,
        bool foldConstantsWhenPossible = true)
    {
        _configuration = configuration;
        FoldConstantsWhenPossible = foldConstantsWhenPossible;
    }

    /// <summary>
    /// Specifies whether or not indexer invocations with all parameters being constant and target being constant
    /// should be resolved immediately as constant expression.
    /// </summary>
    public bool FoldConstantsWhenPossible { get; }

    /// <inheritdoc />
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 2 );

        var target = parameters[0];
        var callParameters = parameters.Slice( 1 );
        var parameterTypes = callParameters.GetExpressionTypes();

        var indexer = _configuration.TryFindTypeIndexer( target.Type, parameterTypes );
        if ( indexer is null )
            throw new ParsedExpressionUnresolvableIndexerException( target.Type, parameterTypes );

        return FoldConstantsWhenPossible
            && target is ConstantExpression constantTarget
            && callParameters.All( static p => p is ConstantExpression )
                ? ExpressionHelpers.CreateConstantIndexer( constantTarget, indexer, callParameters )
                : CreateVariableIndexer( target, indexer, callParameters );
    }

    [Pure]
    private static Expression CreateVariableIndexer(Expression target, MemberInfo indexer, Expression[] parameters)
    {
        if ( indexer is PropertyInfo property )
            return Expression.MakeIndex( target, property, parameters );

        var arrayMethod = ReinterpretCast.To<MethodInfo>( indexer );
        return Expression.Call( target, arrayMethod, parameters );
    }
}
