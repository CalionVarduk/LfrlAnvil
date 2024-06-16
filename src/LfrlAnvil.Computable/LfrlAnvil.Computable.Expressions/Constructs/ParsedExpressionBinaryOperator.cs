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

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a binary operator construct.
/// </summary>
public abstract class ParsedExpressionBinaryOperator
{
    [Pure]
    internal Expression Process(Expression leftOperand, Expression rightOperand)
    {
        var result = CreateResult( leftOperand, rightOperand );
        return result;
    }

    /// <summary>
    /// Attempts to create an expression from two constants.
    /// </summary>
    /// <param name="left">Left constant argument.</param>
    /// <param name="right">Right constant argument.</param>
    /// <returns>New <see cref="Expression"/> or null when it could not be created.</returns>
    [Pure]
    protected virtual Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
    {
        return null;
    }

    /// <summary>
    /// Attempts to create an expression from one constant.
    /// </summary>
    /// <param name="left">Left constant argument.</param>
    /// <param name="right">Right argument.</param>
    /// <returns>New <see cref="Expression"/> or null when it could not be created.</returns>
    [Pure]
    protected virtual Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
    {
        return null;
    }

    /// <summary>
    /// Attempts to create an expression from one constant.
    /// </summary>
    /// <param name="left">Left argument.</param>
    /// <param name="right">Right constant argument.</param>
    /// <returns>New <see cref="Expression"/> or null when it could not be created.</returns>
    [Pure]
    protected virtual Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
    {
        return null;
    }

    /// <summary>
    /// Creates an expression.
    /// </summary>
    /// <param name="left">Left argument.</param>
    /// <param name="right">Right argument.</param>
    /// <returns>New <see cref="Expression"/>.</returns>
    [Pure]
    protected abstract Expression CreateBinaryExpression(Expression left, Expression right);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression left, Expression right)
    {
        if ( left is ConstantExpression constantLeft )
        {
            if ( right is ConstantExpression andConstantRight )
                return TryCreateFromTwoConstants( constantLeft, andConstantRight ) ?? CreateBinaryExpression( left, right );

            return TryCreateFromOneConstant( constantLeft, right ) ?? CreateBinaryExpression( left, right );
        }

        if ( right is ConstantExpression constantRight )
            return TryCreateFromOneConstant( left, constantRight ) ?? CreateBinaryExpression( left, right );

        return CreateBinaryExpression( left, right );
    }
}

/// <summary>
/// Represents a binary operator construct.
/// </summary>
/// <typeparam name="TLeftArg">Left argument's type.</typeparam>
/// <typeparam name="TRightArg">Right argument's type.</typeparam>
public abstract class ParsedExpressionBinaryOperator<TLeftArg, TRightArg> : ParsedExpressionTypedBinaryOperator
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBinaryOperator{TLeftArg,TRightArg}"/> instance.
    /// </summary>
    protected ParsedExpressionBinaryOperator()
        : base( typeof( TLeftArg ), typeof( TRightArg ) ) { }

    /// <summary>
    /// Attempts to extract a constant value of a left argument.
    /// </summary>
    /// <param name="expression">Source constant expression.</param>
    /// <param name="result"><b>out</b> parameter that returns the underlying value.</param>
    /// <returns><b>true</b> if value was extracted successfully, otherwise <b>false</b>.</returns>
    protected static bool TryGetLeftArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TLeftArg result)
    {
        return expression.TryGetValue( out result );
    }

    /// <summary>
    /// Attempts to extract a constant value of a right argument.
    /// </summary>
    /// <param name="expression">Source constant expression.</param>
    /// <param name="result"><b>out</b> parameter that returns the underlying value.</param>
    /// <returns><b>true</b> if value was extracted successfully, otherwise <b>false</b>.</returns>
    protected static bool TryGetRightArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TRightArg result)
    {
        return expression.TryGetValue( out result );
    }
}

/// <summary>
/// Represents a binary operator construct.
/// </summary>
/// <typeparam name="TArg">Argument's type.</typeparam>
public abstract class ParsedExpressionBinaryOperator<TArg> : ParsedExpressionTypedBinaryOperator
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBinaryOperator{TArg}"/> instance.
    /// </summary>
    protected ParsedExpressionBinaryOperator()
        : base( typeof( TArg ), typeof( TArg ) ) { }

    /// <summary>
    /// Attempts to extract a constant value of an argument.
    /// </summary>
    /// <param name="expression">Source constant expression.</param>
    /// <param name="result"><b>out</b> parameter that returns the underlying value.</param>
    /// <returns><b>true</b> if value was extracted successfully, otherwise <b>false</b>.</returns>
    protected static bool TryGetArgumentValue(ConstantExpression expression, [MaybeNullWhen( false )] out TArg result)
    {
        return expression.TryGetValue( out result );
    }
}
