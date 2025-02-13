using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class UnaryOperatorsTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<TArg, TResult>(
        ParsedExpressionUnaryOperator sut,
        ExpressionType expectedNodeType,
        Func<Expression, Expression, Assertion> nodeAssertion)
    {
        var operand = CreateVariableOperand<TArg>( "value" );

        var result = sut.Process( operand );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( operand, result ) )
            .Go();
    }

    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<TArg, TResult>(
        ParsedExpressionUnaryOperator sut,
        ExpressionType expectedNodeType,
        TArg operandValue,
        Func<Expression, Expression, Assertion> nodeAssertion)
    {
        var operand = CreateConstantOperand( operandValue );

        var result = sut.Process( operand );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( operand, result ) )
            .Go();
    }

    protected static void Process_ShouldThrowException_WhenOperatorDoesNotExist<TArg, TException>(ParsedExpressionUnaryOperator sut)
        where TException : Exception
    {
        var operand = CreateVariableOperand<TArg>( "value" );
        var action = Lambda.Of( () => sut.Process( operand ) );
        action.Test( exc => exc.TestType().Exact<TException>() ).Go();
    }

    [Pure]
    protected static Assertion DefaultNodeAssertion(Expression operand, Expression result)
    {
        return result.TestType().AssignableTo<UnaryExpression>( unaryResult => unaryResult.Operand.TestRefEquals( operand ) );
    }
}
