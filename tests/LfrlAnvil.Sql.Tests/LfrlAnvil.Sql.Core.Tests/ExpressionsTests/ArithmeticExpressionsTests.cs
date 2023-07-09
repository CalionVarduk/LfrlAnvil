using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class ArithmeticExpressionsTests : TestsBase
{
    [Fact]
    public void Negate_ShouldCreateNegateExpressionNode()
    {
        var node = SqlNode.Literal( 42 );
        var sut = -node;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Negate );
            sut.Value.Should().BeSameAs( node );
            text.Should().Be( $"-({node})" );
        }
    }

    [Fact]
    public void BitwiseNot_ShouldCreateBitwiseNotExpressionNode()
    {
        var node = SqlNode.Literal( 42 );
        var sut = ~node;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.BitwiseNot );
            sut.Value.Should().BeSameAs( node );
            text.Should().Be( $"~({node})" );
        }
    }

    [Fact]
    public void Add_ShouldCreateAddExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left + right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Add );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) + ({right})" );
        }
    }

    [Fact]
    public void Concat_ShouldCreateConcatExpressionNode()
    {
        var left = SqlNode.Literal( "x" );
        var right = SqlNode.Parameter<string>( "foo", isNullable: true );
        var sut = left.Concat( right );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Concat );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) || ({right})" );
        }
    }

    [Fact]
    public void Subtract_ShouldCreateSubtractExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left - right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Subtract );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) - ({right})" );
        }
    }

    [Fact]
    public void Multiply_ShouldCreateMultiplyExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left * right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Multiply );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) * ({right})" );
        }
    }

    [Fact]
    public void Divide_ShouldCreateDivideExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left / right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Divide );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) / ({right})" );
        }
    }

    [Fact]
    public void Modulo_ShouldCreateModuloExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left % right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Modulo );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) % ({right})" );
        }
    }

    [Fact]
    public void BitwiseAnd_ShouldCreateBitwiseAndExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left & right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.BitwiseAnd );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) & ({right})" );
        }
    }

    [Fact]
    public void BitwiseOr_ShouldCreateBitwiseOrExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left | right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.BitwiseOr );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) | ({right})" );
        }
    }

    [Fact]
    public void BitwiseXor_ShouldCreateBitwiseXorExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left ^ right;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.BitwiseXor );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) ^ ({right})" );
        }
    }

    [Fact]
    public void BitwiseLeftShift_ShouldCreateBitwiseLeftShiftExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left.BitwiseLeftShift( right );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.BitwiseLeftShift );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) << ({right})" );
        }
    }

    [Fact]
    public void BitwiseRightShift_ShouldCreateBitwiseRightShiftExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left.BitwiseRightShift( right );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.BitwiseRightShift );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) >> ({right})" );
        }
    }
}
