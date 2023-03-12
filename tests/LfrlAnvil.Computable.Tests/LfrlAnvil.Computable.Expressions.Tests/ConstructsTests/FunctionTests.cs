using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Functional;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Computable.Expressions.Tests.ConstructsTests;

public class FunctionTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldThrowArgumentException_WhenExpressionHasVoidReturnType()
    {
        var voidExpression = Expression.Lambda<Action>( Expression.Block() );
        var action = Lambda.Of( () => new ParsedExpressionFunction( voidExpression ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Process_ShouldReturnLambdaBody_WhenExpressionDoesNotHaveAnyParameters()
    {
        var expression = Lambda.ExpressionOf( () => Fixture.Create<int>() );
        var sut = new ParsedExpressionFunction( expression );

        var result = sut.Process( Array.Empty<Expression>() );

        result.Should().BeSameAs( expression.Body );
    }

    [Fact]
    public void Process_ShouldReturnLambdaBodyWithReplacedParameters_WhenExpressionHasAllNamedParameters()
    {
        var expression = Lambda.ExpressionOf( (int a, int b) => a + b + 10 );
        var sut = new ParsedExpressionFunction( expression );

        var v1 = Expression.Constant( 1 );
        var v2 = Expression.Constant( 2 );

        var result = sut.Process( new[] { v1, v2 } );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Add );
            if ( result is not BinaryExpression binaryResult )
                return;

            binaryResult.Left.NodeType.Should().Be( ExpressionType.Add );
            binaryResult.Right.NodeType.Should().Be( ExpressionType.Constant );
            if ( binaryResult.Left is not BinaryExpression parametersAdd || binaryResult.Right is not ConstantExpression constant )
                return;

            parametersAdd.Left.Should().BeSameAs( v1 );
            parametersAdd.Right.Should().BeSameAs( v2 );
            constant.Value.Should().Be( 10 );
        }
    }

    [Fact]
    public void Process_ShouldReturnLambdaInvocation_WhenExpressionHasParametersAndAtLeastOneIsNotNamed()
    {
        var p1 = Expression.Parameter( typeof( int ), "p1" );
        var p2 = Expression.Parameter( typeof( int ), "p2" );
        var p3 = Expression.Parameter( typeof( int ) );
        var body = Expression.Add( p1, Expression.Add( p2, p3 ) );
        var expression = Expression.Lambda<Func<int, int, int, int>>( body, p1, p2, p3 );
        var sut = new ParsedExpressionFunction( expression );

        var v1 = Expression.Constant( 1 );
        var v2 = Expression.Constant( 2 );
        var v3 = Expression.Constant( 3 );

        var result = sut.Process( new[] { v1, v2, v3 } );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Invoke );
            if ( result is not InvocationExpression invocation )
                return;

            invocation.Expression.Should().BeSameAs( expression );
            invocation.Arguments.Should().BeSequentiallyEqualTo( v1, v2, v3 );
        }
    }

    [Fact]
    public void Process_ShouldReturnLambdaInvocation_WhenExpressionCanBeInlinedButInlineIfPossibleIsSetToFalse()
    {
        var value = Fixture.Create<int>();
        var expression = Expression.Lambda<Func<int>>( Expression.Constant( value ) );
        var sut = new ParsedExpressionFunction( expression, inlineIfPossible: false );

        var result = sut.Process( Array.Empty<Expression>() );

        using ( new AssertionScope() )
        {
            result.NodeType.Should().Be( ExpressionType.Invoke );
            if ( result is not InvocationExpression invocation )
                return;

            invocation.Expression.Should().BeSameAs( expression );
        }
    }

    [Fact]
    public void Create_Func0_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( () => Fixture.Create<int>() );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }

    [Fact]
    public void Create_Func1_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( (int a) => a );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }

    [Fact]
    public void Create_Func2_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( (int a, int b) => a + b );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }

    [Fact]
    public void Create_Func3_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( (int a, int b, int c) => a + b + c );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }

    [Fact]
    public void Create_Func4_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( (int a, int b, int c, int d) => a + b + c + d );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }

    [Fact]
    public void Create_Func5_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( (int a, int b, int c, int d, int e) => a + b + c + d + e );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }

    [Fact]
    public void Create_Func6_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( (int a, int b, int c, int d, int e, int f) => a + b + c + d + e + f );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }

    [Fact]
    public void Create_Func7_ShouldContainCorrectLambda()
    {
        var expression = Lambda.ExpressionOf( (int a, int b, int c, int d, int e, int f, int g) => a + b + c + d + e + f + g );
        var sut = ParsedExpressionFunction.Create( expression );
        sut.Lambda.Should().BeSameAs( expression );
    }
}
