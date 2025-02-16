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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Negate ),
                sut.Value.TestRefEquals( node ),
                text.TestEquals( $"-({node})" ) )
            .Go();
    }

    [Fact]
    public void BitwiseNot_ShouldCreateBitwiseNotExpressionNode()
    {
        var node = SqlNode.Literal( 42 );
        var sut = ~node;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.BitwiseNot ),
                sut.Value.TestRefEquals( node ),
                text.TestEquals( $"~({node})" ) )
            .Go();
    }

    [Fact]
    public void Add_ShouldCreateAddExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left + right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Add ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) + ({right})" ) )
            .Go();
    }

    [Fact]
    public void Concat_ShouldCreateConcatExpressionNode()
    {
        var left = SqlNode.Literal( "x" );
        var right = SqlNode.Parameter<string>( "foo", isNullable: true );
        var sut = left.Concat( right );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Concat ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) || ({right})" ) )
            .Go();
    }

    [Fact]
    public void Subtract_ShouldCreateSubtractExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left - right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Subtract ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) - ({right})" ) )
            .Go();
    }

    [Fact]
    public void Multiply_ShouldCreateMultiplyExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left * right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Multiply ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) * ({right})" ) )
            .Go();
    }

    [Fact]
    public void Divide_ShouldCreateDivideExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left / right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Divide ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) / ({right})" ) )
            .Go();
    }

    [Fact]
    public void Modulo_ShouldCreateModuloExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left % right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Modulo ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) % ({right})" ) )
            .Go();
    }

    [Fact]
    public void BitwiseAnd_ShouldCreateBitwiseAndExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left & right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.BitwiseAnd ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) & ({right})" ) )
            .Go();
    }

    [Fact]
    public void BitwiseOr_ShouldCreateBitwiseOrExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left | right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.BitwiseOr ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) | ({right})" ) )
            .Go();
    }

    [Fact]
    public void BitwiseXor_ShouldCreateBitwiseXorExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left ^ right;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.BitwiseXor ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) ^ ({right})" ) )
            .Go();
    }

    [Fact]
    public void BitwiseLeftShift_ShouldCreateBitwiseLeftShiftExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left.BitwiseLeftShift( right );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.BitwiseLeftShift ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) << ({right})" ) )
            .Go();
    }

    [Fact]
    public void BitwiseRightShift_ShouldCreateBitwiseRightShiftExpressionNode()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<int>( "foo", isNullable: true );
        var sut = left.BitwiseRightShift( right );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.BitwiseRightShift ),
                sut.Left.TestRefEquals( left ),
                sut.Right.TestRefEquals( right ),
                text.TestEquals( $"({left}) >> ({right})" ) )
            .Go();
    }
}
