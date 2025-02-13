using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionDelegateTests;

public class ParsedExpressionDelegateTests : TestsBase
{
    [Fact]
    public void Delegate_ShouldBeCreatedWithUnboundArgumentsFromExpression()
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );

        var sut = expression.Compile();

        sut.Arguments.TestRefEquals( expression.UnboundArguments ).Go();
    }

    [Fact]
    public void Invoke_ShouldReturnCorrectResult()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        var result = sut.Invoke( aValue, bValue );

        result.TestEquals( expected ).Go();
    }

    [Theory]
    [InlineData( 0 )]
    [InlineData( 1 )]
    [InlineData( 3 )]
    public void Invoke_ShouldThrowInvalidMathExpressionArgumentCountException_WhenArgumentCountIsDifferentFromLengthOfProvidedValuesArray(
        int valuesCount)
    {
        var values = new decimal[valuesCount];
        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        var action = Lambda.Of( () => sut.Invoke( values ) );

        action.Test(
                exc => exc.TestType()
                    .Exact<InvalidParsedExpressionArgumentCountException>(
                        e => Assertion.All( e.Actual.TestEquals( valuesCount ), e.Expected.TestEquals( sut.Arguments.Count ) ) ) )
            .Go();
    }
}
