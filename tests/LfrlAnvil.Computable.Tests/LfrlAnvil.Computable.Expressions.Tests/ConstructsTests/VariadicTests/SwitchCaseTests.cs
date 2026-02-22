using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests.VariadicTests;

public class SwitchCaseTests : TestsBase
{
    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    public void Process_ShouldThrowArgumentException_WhenParameterCountIsLessThanTwo(int count)
    {
        var parameters = Enumerable.Range( 0, count ).Select( _ => Expression.Constant( true ) ).ToArray();
        var sut = new ParsedExpressionSwitchCase();

        var action = Lambda.Of( () => sut.Process( parameters ) );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Process_ShouldReturnSwitchCaseWithLastParameterAsBody()
    {
        var parameters = new Expression[] { Expression.Constant( 0 ), Expression.Constant( 1 ), Expression.Constant( "foo" ) };

        var sut = new ParsedExpressionSwitchCase();

        var result = sut.Process( parameters );

        Assertion.All(
                result.NodeType.TestEquals( ExpressionType.Constant ),
                result.TestType()
                    .AssignableTo<ConstantExpression>( constant => Assertion.All(
                        "constant",
                        constant.Value.TestType().AssignableTo<SwitchCase>(),
                        constant.Value.TestType()
                            .AssignableTo<SwitchCase>( switchCase => Assertion.All(
                                "switchCase",
                                switchCase.Body.TestRefEquals( parameters[^1] ),
                                switchCase.TestValues.TestSequence( [ parameters[0], parameters[1] ] ) ) ) ) ) )
            .Go();
    }
}
