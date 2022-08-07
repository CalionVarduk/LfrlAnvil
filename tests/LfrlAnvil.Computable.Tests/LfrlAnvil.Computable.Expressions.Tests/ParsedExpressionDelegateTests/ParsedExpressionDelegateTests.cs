using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

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

        using ( new AssertionScope() )
        {
            sut.GetArgumentCount().Should().Be( 2 );
            sut.GetArgumentNames().Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "b" );
        }
    }

    [Theory]
    [InlineData( "a", true )]
    [InlineData( "b", true )]
    [InlineData( "c", false )]
    public void ContainsArgument_ShouldReturnTrueIfArgumentWithNameExists(string name, bool expected)
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        using ( new AssertionScope() )
        {
            sut.ContainsArgument( name ).Should().Be( expected );
            sut.ContainsArgument( name.AsMemory() ).Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( "a", 0 )]
    [InlineData( "b", 1 )]
    [InlineData( "c", -1 )]
    public void GetArgumentIndex_ShouldReturnCorrectResult(string name, int expected)
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        using ( new AssertionScope() )
        {
            sut.GetArgumentIndex( name ).Should().Be( expected );
            sut.GetArgumentIndex( name.AsMemory() ).Should().Be( expected );
        }
    }

    [Theory]
    [InlineData( 0, "a" )]
    [InlineData( 1, "b" )]
    [InlineData( -1, "" )]
    [InlineData( 2, "" )]
    public void GetArgumentName_ShouldReturnCorrectResult(int index, string expected)
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        sut.GetArgumentName( index ).ToString().Should().Be( expected );
    }

    [Fact]
    public void Invoke_ShouldReturnCorrectResult()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var sut = expression.Compile();

        var result = sut.Invoke( aValue, bValue );

        result.Should().Be( expected );
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

        action.Should()
            .ThrowExactly<InvalidParsedExpressionArgumentCountException>()
            .AndMatch( e => e.Actual == valuesCount && e.Expected == sut.GetArgumentCount() );
    }
}
