using System.Linq;
using System.Linq.Expressions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldReturnSwitchCaseWithLastParameterAsBody()
    {
        var parameters = new Expression[]
        {
            Expression.Constant( 0 ),
            Expression.Constant( 1 ),
            Expression.Constant( "foo" )
        };

        var sut = new ParsedExpressionSwitchCase();

        var result = sut.Process( parameters );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Constant );
            if ( result is not ConstantExpression constant )
                return;

            constant.Value.Should().BeOfType<SwitchCase>();
            if ( constant.Value is not SwitchCase switchCase )
                return;

            switchCase.Body.Should().BeSameAs( parameters[^1] );
            switchCase.TestValues.Should().BeSequentiallyEqualTo( parameters[0], parameters[1] );
        }
    }
}
