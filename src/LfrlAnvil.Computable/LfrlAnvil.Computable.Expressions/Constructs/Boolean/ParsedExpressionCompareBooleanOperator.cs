﻿// Copyright 2024 Łukasz Furlepa
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

using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Boolean;

/// <summary>
/// Represents a <see cref="Boolean"/> binary compare operator construct.
/// </summary>
public sealed class ParsedExpressionCompareBooleanOperator : ParsedExpressionBinaryOperator<bool>
{
    private readonly MethodInfo _compareTo;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionCompareBooleanOperator"/> instance.
    /// </summary>
    public ParsedExpressionCompareBooleanOperator()
    {
        _compareTo = MemberInfoLocator.FindCompareToMethod(
            typeof( bool ),
            typeof( bool ),
            typeof( ParsedExpressionCompareBooleanOperator ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return TryGetArgumentValue( left, out var leftValue ) && TryGetArgumentValue( right, out var rightValue )
            ? Expression.Constant( leftValue.CompareTo( rightValue ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateBinaryExpression(Expression left, Expression right)
    {
        return Expression.Call( left, _compareTo, right );
    }
}
