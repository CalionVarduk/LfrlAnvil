using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ParsedExpressionBufferedDelegateTests;

public class ParsedExpressionBufferedDelegateTests : TestsBase
{
    [Fact]
    public void ToBuffered_ShouldCreateDelegateWithDefaultValuesInBuffer()
    {
        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();

        var sut = @delegate.ToBuffered();

        using ( new AssertionScope() )
        {
            sut.Base.Should().BeSameAs( @delegate );
            sut.GetArgumentValue( 0 ).Should().Be( default );
            sut.GetArgumentValue( 1 ).Should().Be( default );
        }
    }

    [Fact]
    public void SetArgumentValue_WithIndexKey_ShouldUpdateUnderlyingBufferCorrectly()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        sut.SetArgumentValue( 0, aValue );
        var result = sut.SetArgumentValue( 1, bValue );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetArgumentValue( 0 ).Should().Be( aValue );
            sut.GetArgumentValue( 1 ).Should().Be( bValue );
        }
    }

    [Fact]
    public void SetArgumentValue_ShouldUpdateUnderlyingBufferCorrectly()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        sut.SetArgumentValue( "a", aValue );
        var result = sut.SetArgumentValue( "b", bValue );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetArgumentValue( "a" ).Should().Be( aValue );
            sut.GetArgumentValue( "b" ).Should().Be( bValue );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void SetArgumentValue_WithIndexKey_ShouldThrowIndexOutOfRangeException_WhenArgumentWithIndexDoesNotExist(int index)
    {
        var value = Fixture.Create<decimal>();

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.SetArgumentValue( index, value ) );

        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void SetArgumentValue_ShouldThrowInvalidMathExpressionArgumentsException_WhenArgumentNameDoesNotExist()
    {
        var value = Fixture.Create<decimal>();

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.SetArgumentValue( "c", value ) );

        action.Should()
            .ThrowExactly<InvalidParsedExpressionArgumentsException>()
            .AndMatch( e => e.ArgumentNames.Select( n => n.ToString() ).SequenceEqual( new[] { "c" } ) );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void GetArgumentValue_WithIndexKey_ShouldThrowIndexOutOfRangeException_WhenArgumentWithIndexDoesNotExist(int index)
    {
        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.GetArgumentValue( index ) );

        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void GetArgumentValue_ShouldThrowInvalidMathExpressionArgumentsException_WhenArgumentNameDoesNotExist()
    {
        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.GetArgumentValue( "c" ) );

        action.Should()
            .ThrowExactly<InvalidParsedExpressionArgumentsException>()
            .AndMatch( e => e.ArgumentNames.Select( n => n.ToString() ).SequenceEqual( new[] { "c" } ) );
    }

    [Fact]
    public void ClearArgumentValues_ShouldSetProvidedDefaultValueForAllArguments()
    {
        var defaultValue = Fixture.Create<decimal>();

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var result = sut.ClearArgumentValues( defaultValue );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetArgumentValue( 0 ).Should().Be( defaultValue );
            sut.GetArgumentValue( 1 ).Should().Be( defaultValue );
        }
    }

    [Fact]
    public void Invoke_ShouldInvokeBaseDelegateWithBufferedArgumentValues()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );
        var expected = aValue + bValue;

        var input = "a + b";
        var builder = new ParsedExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new ParsedExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        sut.SetArgumentValue( 0, aValue );
        sut.SetArgumentValue( 1, bValue );

        var result = sut.Invoke();

        result.Should().Be( expected );
    }
}
