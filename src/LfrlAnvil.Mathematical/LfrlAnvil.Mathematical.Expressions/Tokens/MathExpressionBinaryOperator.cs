using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mathematical.Expressions.Tokens
{
    public abstract class MathExpressionBinaryOperator : MathExpressionOperator
    {
        protected MathExpressionBinaryOperator(string symbol)
            : base( symbol ) { }

        public sealed override void Process(Stack<Expression> operandStack)
        {
            if ( ! operandStack.TryPop( out var rightOperand ) )
                throw new Exception(); // TODO

            if ( ! operandStack.TryPop( out var leftOperand ) )
                throw new Exception(); // TODO

            var result = CreateResult( leftOperand, rightOperand );
            operandStack.Push( result );
        }

        protected virtual Expression? TryCreateFromTwoConstants(ConstantExpression left, ConstantExpression right)
        {
            return null;
        }

        protected virtual Expression? TryCreateFromOneConstant(ConstantExpression left, Expression right)
        {
            return null;
        }

        protected virtual Expression? TryCreateFromOneConstant(Expression left, ConstantExpression right)
        {
            return null;
        }

        protected abstract Expression CreateBinaryExpression(Expression left, Expression right);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private Expression CreateResult(Expression left, Expression right)
        {
            // TODO: try convert parameters to the same type? let's actually see, if expressions automatically try to convert
            // primitive number types
            // if not, then convert based on some type conversion precedence settings (e.g. double is more important then int)
            // these conversions can be explicit and/or implicit (if implicit conversions are enabled)
            // if precedence is the same, then simply convert right operand to the left operand's type

            // another way of dealing with this would be to add MathExpressionConversionOperator<TDest> : MathExpressionUnaryOperator
            // this way, we could allow to easily specify available conversions
            // this operator could contain a dictionary of MathExpressionOperandConverter<TSource, TDest> instances
            // that handle specific cases (no switches etc.)
            // I like this solution more, seems easier to implement & more flexible
            // automatic implicit conversions would go nice with that

            if ( left.NodeType == ExpressionType.Constant )
            {
                if ( right.NodeType == ExpressionType.Constant )
                {
                    return TryCreateFromTwoConstants( (ConstantExpression)left, (ConstantExpression)right ) ??
                        CreateBinaryExpression( left, right );
                }

                return TryCreateFromOneConstant( (ConstantExpression)left, right ) ?? CreateBinaryExpression( left, right );
            }

            if ( right.NodeType == ExpressionType.Constant )
                return TryCreateFromOneConstant( left, (ConstantExpression)right ) ?? CreateBinaryExpression( left, right );

            return CreateBinaryExpression( left, right );
        }
    }
}
