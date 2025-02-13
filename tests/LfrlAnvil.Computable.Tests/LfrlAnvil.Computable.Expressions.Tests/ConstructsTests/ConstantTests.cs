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

        Assertion.All(
                sut.Expression.Value.TestEquals( value ),
                sut.Expression.Type.TestEquals( typeof( int ) ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectExpression()
    {
        var value = Fixture.Create<int>();
        var sut = new ParsedExpressionConstant<int>( value );

        Assertion.All(
                sut.Expression.Value.TestEquals( value ),
                sut.Expression.Type.TestEquals( typeof( int ) ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectExpression_WhenValueIsNull()
    {
        var sut = new ParsedExpressionConstant( typeof( string ), null );

        Assertion.All(
                sut.Expression.Value.TestNull(),
                sut.Expression.Type.TestEquals( typeof( string ) ) )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldThrowArgumentException_WhenTypeAndValueAreIncompatible()
    {
        var value = Fixture.Create<int>();
        var action = Lambda.Of( () => new ParsedExpressionConstant( typeof( string ), value ) );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }
}
