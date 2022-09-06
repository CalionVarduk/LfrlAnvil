using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionTests;

public class ParsedExpressionTests : TestsBase
{
    [Fact]
    public void Expression_ShouldBeCreatedWithoutBoundArguments()
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();

        var sut = factory.Create<decimal, decimal>( input );

        using ( new AssertionScope() )
        {
            sut.Input.Should().Be( input );
            sut.BoundArguments.Should().BeEmpty();
            sut.DiscardedArguments.Should().BeEmpty();
            sut.UnboundArguments.Should().HaveCount( 2 );
            sut.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            sut.UnboundArguments.GetIndex( "b" ).Should().Be( 1 );
        }
    }

    [Fact]
    public void Expression_ShouldBeCreatedWithDiscardedArguments_WhenSomeArgumentsHaveBeenOptimizedAwayByConstructs()
    {
        var input = "0 * a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var factory = builder.Build();

        var sut = factory.Create<decimal, decimal>( input );

        using ( new AssertionScope() )
        {
            sut.Input.Should().Be( input );
            sut.BoundArguments.Should().BeEmpty();
            sut.DiscardedArguments.Select( n => n.ToString() ).Should().BeEquivalentTo( "a" );
            sut.UnboundArguments.Should().HaveCount( 1 );
            sut.UnboundArguments.GetIndex( "b" ).Should().Be( 0 );
        }
    }

    [Fact]
    public void ToString_ShouldReturnInfoAboutGenericArgumentsAndInput()
    {
        var input = "a + 12.34 + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddPrefixTypeConverter( "[double]", new ParsedExpressionTypeConverter<double>() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetPrefixUnaryConstructPrecedence( "[double]", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, double>( input );

        var result = sut.ToString();

        result.Should().Be( "[System.Decimal => System.Double] a + 12.34 + b" );
    }

    [Fact]
    public void BindArguments_WithStringKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        using ( new AssertionScope() )
        {
            result.UnboundArguments.Should().HaveCount( 2 );
            result.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            result.UnboundArguments.GetIndex( "d" ).Should().Be( 1 );
            result.BoundArguments.Should().HaveCount( 3 );
            result.BoundArguments.TryGetValue( "b", out var actualB ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "c", out var actualC ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "e", out var actualE ).Should().BeTrue();
            actualB.Should().Be( bValue );
            actualC.Should().Be( cValue );
            actualE.Should().Be( eValue );
        }
    }

    [Fact]
    public void BindArguments_WithStringSliceKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b".AsSlice(), bValue ),
            KeyValuePair.Create( "c".AsSlice(), cValue ),
            KeyValuePair.Create( "e".AsSlice(), eValue ) );

        using ( new AssertionScope() )
        {
            result.UnboundArguments.Should().HaveCount( 2 );
            result.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            result.UnboundArguments.GetIndex( "d" ).Should().Be( 1 );
            result.BoundArguments.Should().HaveCount( 3 );
            result.BoundArguments.TryGetValue( "b", out var actualB ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "c", out var actualC ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "e", out var actualE ).Should().BeTrue();
            actualB.Should().Be( bValue );
            actualC.Should().Be( cValue );
            actualE.Should().Be( eValue );
        }
    }

    [Fact]
    public void BindArguments_WithIndexKey_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( 1, bValue ),
            KeyValuePair.Create( 2, cValue ),
            KeyValuePair.Create( 4, eValue ) );

        using ( new AssertionScope() )
        {
            result.UnboundArguments.Should().HaveCount( 2 );
            result.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            result.UnboundArguments.GetIndex( "d" ).Should().Be( 1 );
            result.BoundArguments.Should().HaveCount( 3 );
            result.BoundArguments.TryGetValue( "b", out var actualB ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "c", out var actualC ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "e", out var actualE ).Should().BeTrue();
            actualB.Should().Be( bValue );
            actualC.Should().Be( cValue );
            actualE.Should().Be( eValue );
        }
    }

    [Fact]
    public void BindArguments_ShouldReturnThis_WhenParameterCollectionIsMaterializedAndEmpty()
    {
        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( Array.Empty<KeyValuePair<int, decimal>>() );

        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void BindArguments_ShouldReturnThis_WhenParameterCollectionIsNotMaterializedAndEmpty()
    {
        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            new[]
                {
                    KeyValuePair.Create( 1, 0m ),
                    KeyValuePair.Create( 2, 0m ),
                    KeyValuePair.Create( 4, 0m )
                }
                .Where( _ => false ) );

        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void BindArguments_CalledInChain_ShouldBindExpressionArgumentsCorrectly()
    {
        var (bValue, cValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut
            .BindArguments( KeyValuePair.Create( "b", bValue ) )
            .BindArguments( KeyValuePair.Create( "c", cValue ) )
            .BindArguments( KeyValuePair.Create( "e", eValue ) );

        using ( new AssertionScope() )
        {
            result.UnboundArguments.Should().HaveCount( 2 );
            result.UnboundArguments.GetIndex( "a" ).Should().Be( 0 );
            result.UnboundArguments.GetIndex( "d" ).Should().Be( 1 );
            result.BoundArguments.Should().HaveCount( 3 );
            result.BoundArguments.TryGetValue( "b", out var actualB ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "c", out var actualC ).Should().BeTrue();
            result.BoundArguments.TryGetValue( "e", out var actualE ).Should().BeTrue();
            actualB.Should().Be( bValue );
            actualC.Should().Be( cValue );
            actualE.Should().Be( eValue );
        }
    }

    [Fact]
    public void BindArguments_ShouldMarkArgumentsAsDiscardedCorrectlyWhenSomeOfThemGetOptimizedAwayDueToBinding()
    {
        var input = "a * b + c";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", 0m ) );

        using ( new AssertionScope() )
        {
            result.DiscardedArguments.Select( n => n.ToString() ).Should().BeEquivalentTo( "b" );
            result.UnboundArguments.Should().HaveCount( 1 );
            result.UnboundArguments.GetIndex( "c" ).Should().Be( 0 );
            result.BoundArguments.Should().HaveCount( 1 );
            result.BoundArguments.TryGetValue( "a", out var actualA ).Should().BeTrue();
            actualA.Should().Be( 0m );
        }
    }

    [Fact]
    public void BindArguments_ShouldAddDiscardedArgumentsCorrectlyWhenSomeOfThemGetOptimizedAwayDueToBindingAndDueToOriginalParsing()
    {
        var input = "(0 * a) + (b * c) + d";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .AddBinaryOperator( "*", new ParsedExpressionMultiplyDecimalOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 )
            .SetBinaryOperatorPrecedence( "*", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "b", 0m ) );

        using ( new AssertionScope() )
        {
            result.DiscardedArguments.Select( n => n.ToString() ).Should().BeEquivalentTo( "a", "c" );
            result.UnboundArguments.Should().HaveCount( 1 );
            result.UnboundArguments.GetIndex( "d" ).Should().Be( 0 );
            result.BoundArguments.Should().HaveCount( 1 );
            result.BoundArguments.TryGetValue( "b", out var actualB ).Should().BeTrue();
            actualB.Should().Be( 0m );
        }
    }

    [Fact]
    public void
        BindArguments_ShouldThrowMathExpressionArgumentBindingException_WhenAnyArgumentToBindDoesNotExistInUnboundArgumentsCollection()
    {
        var (bValue, cValue, fValue) = Fixture.CreateDistinctCollection<decimal>( count: 3 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var action = Lambda.Of(
            () => sut
                .BindArguments( KeyValuePair.Create( "b", bValue ) )
                .BindArguments( KeyValuePair.Create( "c", cValue ) )
                .BindArguments( KeyValuePair.Create( "f", fValue ) ) );

        action.Should().ThrowExactly<ParsedExpressionArgumentBindingException>();
    }

    [Fact]
    public void BindArguments_ShouldThrowParsedExpressionCreationException_WhenArgumentBindingCausesAnErrorDuringExpressionParsing()
    {
        var input = "a / b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "/", new ParsedExpressionDivideDecimalOperator() )
            .SetBinaryOperatorPrecedence( "/", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var action = Lambda.Of( () => sut.BindArguments( KeyValuePair.Create( "b", 0m ) ) );

        action.Should().ThrowExactly<ParsedExpressionCreationException>();
    }

    [Fact]
    public void BindArguments_ShouldCreateExpressionThatCompilesToCorrectDelegate()
    {
        var (aValue, bValue, cValue, dValue, eValue) = Fixture.CreateDistinctCollection<decimal>( count: 5 );

        var input = "a + b + c + d + e";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments(
            KeyValuePair.Create( "b", bValue ),
            KeyValuePair.Create( "c", cValue ),
            KeyValuePair.Create( "e", eValue ) );

        var unboundDelegate = sut.Compile();
        var boundDelegate = result.Compile();

        var unboundResult = unboundDelegate.Invoke( aValue, bValue, cValue, dValue, eValue );
        var boundResult = boundDelegate.Invoke( aValue, dValue );

        unboundResult.Should().Be( boundResult );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableStringKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( "a", aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsStringKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a", aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableStringSliceKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( "a".AsSlice(), aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsStringSliceKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( "a".AsSlice(), aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithEnumerableIndexKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( new[] { KeyValuePair.Create( 0, aValue ) }.AsEnumerable() );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }

    [Fact]
    public void IMathExpressionBindArguments_WithParamsIndexKey_ShouldBeEquivalentToBindArguments()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        IParsedExpression<decimal, decimal> sut = factory.Create<decimal, decimal>( input );

        var result = sut.BindArguments( KeyValuePair.Create( 0, aValue ) );
        var @delegate = result.Compile();
        var resultValue = @delegate.Invoke( bValue );

        resultValue.Should().Be( expected );
    }
}
