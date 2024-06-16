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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

/// <summary>
/// Represents an if-then-else construct.
/// </summary>
public sealed class ParsedExpressionIf : ParsedExpressionVariadicFunction
{
    /// <inheritdoc />
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsExactly( parameters, count: 3 );

        var test = parameters[0];
        var ifTrue = parameters[1];
        var ifFalse = parameters[2];

        var expectedType = GetExpectedType( ifTrue, ifFalse );
        if ( expectedType is null )
            throw new ArgumentException( Resources.CannotDetermineIfReturnType, nameof( parameters ) );

        ifTrue = ExpressionHelpers.TryUpdateThrowType( ifTrue, expectedType );
        ifFalse = ExpressionHelpers.TryUpdateThrowType( ifFalse, expectedType );

        var result = test is ConstantExpression constantTest
            ? CreateFromConstantTest( constantTest, ifTrue, ifFalse )
            : Expression.Condition( test, ifTrue, ifFalse );

        return result;
    }

    [Pure]
    private static Expression CreateFromConstantTest(ConstantExpression test, Expression ifTrue, Expression ifFalse)
    {
        if ( test.Type != typeof( bool ) )
            throw new ArgumentException( Resources.IfTestMustBeOfBooleanType( test.Type ), nameof( test ) );

        var testValue = DynamicCast.Unbox<bool>( test.Value );
        return testValue ? ifTrue : ifFalse;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Type? GetExpectedType(Expression ifTrue, Expression ifFalse)
    {
        if ( ifTrue.NodeType != ExpressionType.Throw )
            return ifTrue.Type;

        if ( ifFalse.NodeType != ExpressionType.Throw )
            return ifFalse.Type;

        return null;
    }
}
