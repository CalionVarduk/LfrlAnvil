using FluentAssertions.Execution;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public class ConstantTests : TestsBase
{
    [Fact]
    public void Create_ShouldCreateWithCorrectExpression()
    {
        var value = Fixture.Create<int>();
        var sut = ParsedExpressionConstant.Create( value );

        using ( new AssertionScope() )
        {
            sut.Expression.Value.Should().Be( value );
            sut.Expression.Type.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectExpression()
    {
        var value = Fixture.Create<int>();
        var sut = new ParsedExpressionConstant<int>( value );

        using ( new AssertionScope() )
        {
            sut.Expression.Value.Should().Be( value );
            sut.Expression.Type.Should().Be( typeof( int ) );
        }
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectExpression_WhenValueIsNull()
    {
        var sut = new ParsedExpressionConstant( typeof( string ), null );

        using ( new AssertionScope() )
        {
            sut.Expression.Value.Should().BeNull();
            sut.Expression.Type.Should().Be( typeof( string ) );
        }
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentException_WhenTypeAndValueAreIncompatible()
    {
        var value = Fixture.Create<int>();
        var action = Lambda.Of( () => new ParsedExpressionConstant( typeof( string ), value ) );
        action.Should().ThrowExactly<ArgumentException>();
    }
}
