using System.Linq;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Extensions;
using LfrlAnvil.Functional;

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

        Assertion.All(
                sut.Base.TestRefEquals( @delegate ),
                sut.GetArgumentValue( 0 ).TestEquals( default ),
                sut.GetArgumentValue( 1 ).TestEquals( default ) )
            .Go();
    }

    [Fact]
    public void SetArgumentValue_WithIndexKey_ShouldUpdateUnderlyingBufferCorrectly()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );

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

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetArgumentValue( 0 ).TestEquals( aValue ),
                sut.GetArgumentValue( 1 ).TestEquals( bValue ) )
            .Go();
    }

    [Fact]
    public void SetArgumentValue_ShouldUpdateUnderlyingBufferCorrectly()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );

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

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetArgumentValue( "a" ).TestEquals( aValue ),
                sut.GetArgumentValue( "b" ).TestEquals( bValue ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<IndexOutOfRangeException>() ).Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<InvalidParsedExpressionArgumentsException>(
                        e => e.ArgumentNames.Select( n => n.ToString() ).TestSequence( [ "c" ] ) ) )
            .Go();
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

        action.Test( exc => exc.TestType().Exact<IndexOutOfRangeException>() ).Go();
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

        action.Test(
                exc => exc.TestType()
                    .Exact<InvalidParsedExpressionArgumentsException>(
                        e => e.ArgumentNames.Select( n => n.ToString() ).TestSequence( [ "c" ] ) ) )
            .Go();
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

        Assertion.All(
                result.TestRefEquals( sut ),
                sut.GetArgumentValue( 0 ).TestEquals( defaultValue ),
                sut.GetArgumentValue( 1 ).TestEquals( defaultValue ) )
            .Go();
    }

    [Fact]
    public void Invoke_ShouldInvokeBaseDelegateWithBufferedArgumentValues()
    {
        var (aValue, bValue) = Fixture.CreateManyDistinct<decimal>( count: 2 );
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

        result.TestEquals( expected ).Go();
    }
}
