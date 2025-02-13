using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class TypeConvertersTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<TTarget, TSourceArg, TResult>(
        ParsedExpressionTypeConverter<TTarget> sut,
        Func<Expression, Expression, Assertion> nodeAssertion,
        ExpressionType expectedNodeType = ExpressionType.Convert)
    {
        var operand = CreateVariableOperand<TSourceArg>( "value" );

        var result = sut.Process( operand );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( operand, result ) )
            .Go();
    }

    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<TTarget, TSourceArg, TResult>(
        ParsedExpressionTypeConverter<TTarget> sut,
        TSourceArg operandValue,
        Func<Expression, Expression, Assertion> nodeAssertion,
        ExpressionType expectedNodeType = ExpressionType.Convert)
    {
        var operand = CreateConstantOperand( operandValue );

        var result = sut.Process( operand );

        Assertion.All(
                result.NodeType.TestEquals( expectedNodeType ),
                result.Type.TestEquals( typeof( TResult ) ),
                nodeAssertion( operand, result ) )
            .Go();
    }

    protected static void Process_ShouldThrowException_WhenConversionDoesNotExist<TTarget, TSourceArg, TException>(
        ParsedExpressionTypeConverter<TTarget> sut,
        Func<TException, bool>? matcher = null)
        where TException : Exception
    {
        var operand = CreateVariableOperand<TSourceArg>( "value" );
        var action = Lambda.Of( () => sut.Process( operand ) );
        action.Test(
                exc => Assertion.All(
                    exc.TestType().Exact<TException>(),
                    exc.TestIf().OfType<TException>( e => (matcher ?? (_ => true))( e ).TestTrue() ) ) )
            .Go();
    }

    [Pure]
    protected static Assertion DefaultNodeAssertion(Expression operand, Expression result)
    {
        return Assertion.All(
            "Node",
            result.TestType().AssignableTo<UnaryExpression>(),
            result.TestIf().OfType<UnaryExpression>( unaryResult => unaryResult.Operand.TestRefEquals( operand ) ) );
    }
}
