using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public abstract class TypeConvertersTestsBase : ConstructsTestsBase
{
    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsVariable<TTarget, TSourceArg, TResult>(
        ParsedExpressionTypeConverter<TTarget> sut,
        Action<Expression, Expression> nodeAssertion,
        ExpressionType expectedNodeType = ExpressionType.Convert)
    {
        var operand = CreateVariableOperand<TSourceArg>( "value" );
        var stack = CreateStack( operand );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( operand, result );
        }
    }

    protected static void Process_ShouldPopOneOperandAndPushOneExpression_WhenOperandIsConstant<TTarget, TSourceArg, TResult>(
        ParsedExpressionTypeConverter<TTarget> sut,
        TSourceArg operandValue,
        Action<Expression, Expression> nodeAssertion,
        ExpressionType expectedNodeType = ExpressionType.Convert)
    {
        var operand = CreateConstantOperand( operandValue );
        var stack = CreateStack( operand );

        sut.Process( stack );

        using ( new AssertionScope() )
        {
            stack.Count.Should().Be( 1 );
            if ( stack.Count == 0 )
                return;

            var result = stack[0];
            result.NodeType.Should().Be( expectedNodeType );
            result.Type.Should().Be( typeof( TResult ) );
            nodeAssertion( operand, result );
        }
    }

    protected static void Process_ShouldThrowException_WhenConversionDoesNotExist<TTarget, TSourceArg, TException>(
        ParsedExpressionTypeConverter<TTarget> sut,
        Func<TException, bool>? matcher = null)
        where TException : Exception
    {
        var operand = CreateVariableOperand<TSourceArg>( "value" );
        var stack = CreateStack( operand );

        var action = Lambda.Of( () => sut.Process( stack ) );

        action.Should().ThrowExactly<TException>().AndMatch( matcher ?? (_ => true) );
    }

    protected static void DefaultNodeAssertion(Expression operand, Expression result)
    {
        result.Should().BeAssignableTo<UnaryExpression>();
        if ( result is not UnaryExpression unaryResult )
            return;

        unaryResult.Operand.Should().BeSameAs( operand );
    }
}
