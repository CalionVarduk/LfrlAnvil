using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using LfrlAnvil.Functional;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Exceptions;
using LfrlAnvil.Mathematical.Expressions.Extensions;
using LfrlAnvil.TestExtensions;
using LfrlAnvil.TestExtensions.FluentAssertions;
using Xunit;

namespace LfrlAnvil.Mathematical.Expressions.Tests.MathExpressionBufferedDelegateTests;

public class MathExpressionBufferedDelegateTests : TestsBase
{
    [Fact]
    public void ToBuffered_ShouldCreateDelegateWithDefaultValuesInBuffer()
    {
        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
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
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
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
    public void SetArgumentValue_WithStringKey_ShouldUpdateUnderlyingBufferCorrectly()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );

        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
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

    [Fact]
    public void SetArgumentValue_WithReadOnlyMemoryKey_ShouldUpdateUnderlyingBufferCorrectly()
    {
        var (aValue, bValue) = Fixture.CreateDistinctCollection<decimal>( count: 2 );

        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        sut.SetArgumentValue( "a".AsMemory(), aValue );
        var result = sut.SetArgumentValue( "b".AsMemory(), bValue );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.GetArgumentValue( "a".AsMemory() ).Should().Be( aValue );
            sut.GetArgumentValue( "b".AsMemory() ).Should().Be( bValue );
        }
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void SetArgumentValue_WithIndexKey_ShouldThrowIndexOutOfRangeException_WhenArgumentWithIndexDoesNotExist(int index)
    {
        var value = Fixture.Create<decimal>();

        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.SetArgumentValue( index, value ) );

        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void SetArgumentValue_WithStringKey_ShouldThrowInvalidMathExpressionArgumentsException_WhenArgumentNameDoesNotExist()
    {
        var value = Fixture.Create<decimal>();

        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.SetArgumentValue( "c", value ) );

        action.Should()
            .ThrowExactly<InvalidMathExpressionArgumentsException>()
            .AndMatch( e => e.ArgumentNames.Select( n => n.ToString() ).SequenceEqual( new[] { "c" } ) );
    }

    [Fact]
    public void SetArgumentValue_WithReadOnlyMemoryKey_ShouldThrowInvalidMathExpressionArgumentsException_WhenArgumentNameDoesNotExist()
    {
        var value = Fixture.Create<decimal>();

        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.SetArgumentValue( "c".AsMemory(), value ) );

        action.Should()
            .ThrowExactly<InvalidMathExpressionArgumentsException>()
            .AndMatch( e => e.ArgumentNames.Select( n => n.ToString() ).SequenceEqual( new[] { "c" } ) );
    }

    [Theory]
    [InlineData( -1 )]
    [InlineData( 2 )]
    public void GetArgumentValue_WithIndexKey_ShouldThrowIndexOutOfRangeException_WhenArgumentWithIndexDoesNotExist(int index)
    {
        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.GetArgumentValue( index ) );

        action.Should().ThrowExactly<IndexOutOfRangeException>();
    }

    [Fact]
    public void GetArgumentValue_WithStringKey_ShouldThrowInvalidMathExpressionArgumentsException_WhenArgumentNameDoesNotExist()
    {
        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.GetArgumentValue( "c" ) );

        action.Should()
            .ThrowExactly<InvalidMathExpressionArgumentsException>()
            .AndMatch( e => e.ArgumentNames.Select( n => n.ToString() ).SequenceEqual( new[] { "c" } ) );
    }

    [Fact]
    public void GetArgumentValue_WithReadOnlyMemoryKey_ShouldThrowInvalidMathExpressionArgumentsException_WhenArgumentNameDoesNotExist()
    {
        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
            .SetBinaryOperatorPrecedence( "+", 1 );

        var factory = builder.Build();
        var expression = factory.Create<decimal, decimal>( input );
        var @delegate = expression.Compile();
        var sut = @delegate.ToBuffered();

        var action = Lambda.Of( () => sut.GetArgumentValue( "c".AsMemory() ) );

        action.Should()
            .ThrowExactly<InvalidMathExpressionArgumentsException>()
            .AndMatch( e => e.ArgumentNames.Select( n => n.ToString() ).SequenceEqual( new[] { "c" } ) );
    }

    [Fact]
    public void ClearArgumentValues_ShouldSetProvidedDefaultValueForAllArguments()
    {
        var defaultValue = Fixture.Create<decimal>();

        var input = "a + b";
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
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
        var builder = new MathExpressionFactoryBuilder()
            .AddBinaryOperator( "+", new MathExpressionAddOperator() )
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
