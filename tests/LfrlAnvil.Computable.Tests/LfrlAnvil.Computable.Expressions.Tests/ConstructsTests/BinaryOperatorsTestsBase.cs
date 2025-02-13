using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class BinaryOperatorsTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreVariable<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        Func<Expression, Expression, Expression, Assertion> nodeAssertion)
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateVariableOperand<TRightArg>( "right" );

        var result = sut.Process( left, right );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( left, right, result ) )
            .Go();
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenBothOperandsAreConstant<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TLeftArg leftValue,
        TRightArg rightValue,
        Func<Expression, Expression, Expression, Assertion> nodeAssertion)
    {
        var left = CreateConstantOperand( leftValue );
        var right = CreateConstantOperand( rightValue );

        var result = sut.Process( left, right );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( left, right, result ) )
            .Go();
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenLeftOperandIsConstant<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TLeftArg leftValue,
        Func<Expression, Expression, Expression, Assertion> nodeAssertion)
    {
        var left = CreateConstantOperand( leftValue );
        var right = CreateVariableOperand<TRightArg>( "right" );

        var result = sut.Process( left, right );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( left, right, result ) )
            .Go();
    }

    protected static void Process_ShouldPopTwoOperandsAndPushOneExpression_WhenRightOperandIsConstant<TLeftArg, TRightArg, TResult>(
        ParsedExpressionBinaryOperator sut,
        ExpressionType expectedNodeType,
        TRightArg rightValue,
        Func<Expression, Expression, Expression, Assertion> nodeAssertion)
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateConstantOperand( rightValue );

        var result = sut.Process( left, right );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( left, right, result ) )
            .Go();
    }

    protected static void Process_ShouldThrowException_WhenOperatorDoesNotExist<TLeftArg, TRightArg, TException>(
        ParsedExpressionBinaryOperator sut)
        where TException : Exception
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateVariableOperand<TRightArg>( "right" );

        var action = Lambda.Of( () => sut.Process( left, right ) );

        action.Test( exc => exc.TestType().Exact<TException>() ).Go();
    }

    protected static void Process_ShouldThrowException_WhenAttemptingToResolveRightConstantValue<TLeftArg, TRightArg, TException>(
        ParsedExpressionBinaryOperator sut,
        TRightArg rightValue)
        where TException : Exception
    {
        var left = CreateVariableOperand<TLeftArg>( "left" );
        var right = CreateConstantOperand( rightValue );

        var action = Lambda.Of( () => sut.Process( left, right ) );

        action.Test( exc => exc.TestType().Exact<TException>() ).Go();
    }

    [Pure]
    protected static Assertion DefaultNodeAssertion(Expression left, Expression right, Expression result)
    {
        return Assertion.All(
            "Node",
            result.TestType().AssignableTo<BinaryExpression>(),
            result.TestIf()
                .OfType<BinaryExpression>(
                    binaryResult => Assertion.All(
                        "binaryResult",
                        binaryResult.Left.TestRefEquals( left ),
                        binaryResult.Right.TestRefEquals( right ) ) ) );
    }
}
