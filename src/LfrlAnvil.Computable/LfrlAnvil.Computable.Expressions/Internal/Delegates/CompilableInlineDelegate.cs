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
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace LfrlAnvil.Computable.Expressions.Internal.Delegates;

internal sealed class CompilableInlineDelegate
{
    private readonly ParameterExpression[] _parameters;
    private readonly NewExpression? _closureCtorCall;
    private readonly MethodInfo? _bindClosureMethod;
    private readonly ExpressionReplacement[] _capturedParameterReplacements;
    private readonly CompilableInlineDelegate[] _nestedDelegates;
    private Expression _body;

    internal CompilableInlineDelegate(
        Expression body,
        ParameterExpression[] parameters,
        Expression placeholder,
        NewExpression? closureCtorCall,
        MethodInfo? bindClosureMethod,
        ExpressionReplacement[] capturedParameterReplacements,
        CompilableInlineDelegate[] nestedDelegates)
    {
        _body = body;
        _parameters = parameters;
        Placeholder = placeholder;
        _closureCtorCall = closureCtorCall;
        _bindClosureMethod = bindClosureMethod;
        _capturedParameterReplacements = capturedParameterReplacements;
        _nestedDelegates = nestedDelegates;
    }

    internal Expression Placeholder { get; }

    [MemberNotNullWhen( false, nameof( _closureCtorCall ) )]
    [MemberNotNullWhen( false, nameof( _bindClosureMethod ) )]
    internal bool IsStatic => _closureCtorCall is null;

    internal void ReorganizeArgumentAccess(ArgumentAccessReorganizer reorganizer)
    {
        foreach ( var nested in _nestedDelegates )
            nested.ReorganizeArgumentAccess( reorganizer );

        _body = reorganizer.Visit( _body );
    }

    [Pure]
    internal ExpressionReplacement Compile()
    {
        var placeholderReplacements = _nestedDelegates.Length == 0
            ? Array.Empty<ExpressionReplacement>()
            : new ExpressionReplacement[_nestedDelegates.Length];

        for ( var i = 0; i < _nestedDelegates.Length; ++i )
        {
            var @delegate = _nestedDelegates[i];
            placeholderReplacements[i] = @delegate.Compile();
        }

        var placeholderReplacer = new PlaceholderReplacer( placeholderReplacements, _capturedParameterReplacements );
        var body = placeholderReplacer.Visit( _body );

        if ( IsStatic )
        {
            var lambda = Expression.Lambda( body, _parameters );
            return new ExpressionReplacement( Placeholder, Expression.Constant( lambda.Compile() ) );
        }

        var underlyingLambda = Expression.Lambda( body, _parameters );
        var compiledUnderlyingLambda = Expression.Constant( underlyingLambda.Compile() );
        var bindMethodCall = Expression.Call( null, _bindClosureMethod, _closureCtorCall, compiledUnderlyingLambda );
        return new ExpressionReplacement( Placeholder, bindMethodCall );
    }

    private sealed class PlaceholderReplacer : ExpressionVisitor
    {
        private readonly ExpressionReplacement[] _lambdaReplacements;
        private readonly ExpressionReplacement[] _closureParameterReplacements;

        internal PlaceholderReplacer(ExpressionReplacement[] lambdaReplacements, ExpressionReplacement[] closureParameterReplacements)
        {
            _lambdaReplacements = lambdaReplacements;
            _closureParameterReplacements = closureParameterReplacements;
        }

        [Pure]
        [return: NotNullIfNotNull( "node" )]
        public override Expression? Visit(Expression? node)
        {
            if ( ExpressionHelpers.IsLambdaPlaceholder( node ) )
            {
                var lambdaIndex = Array.FindIndex( _lambdaReplacements, x => x.IsMatched( node ) );
                if ( lambdaIndex < 0 )
                    return base.Visit( node );

                var lambdaResult = _lambdaReplacements[lambdaIndex].Replacement;
                return lambdaResult;
            }

            if ( node is not ParameterExpression )
                return base.Visit( node );

            var parameterIndex = Array.FindIndex( _closureParameterReplacements, x => x.IsMatched( node ) );
            if ( parameterIndex < 0 )
                return base.Visit( node );

            var parameterResult = _closureParameterReplacements[parameterIndex].Replacement;
            return parameterResult;
        }
    }
}
