using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
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
            sut.Type.Should().Be( node.Type );
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
            sut.Type.Should().Be( node.Type );
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) + ({right})" );
        }
    }

    [Fact]
    public void Add_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left + right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<string>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) || ({right})" );
        }
    }

    [Fact]
    public void Concat_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left.Concat( right ) );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) - ({right})" );
        }
    }

    [Fact]
    public void Subtract_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left - right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) * ({right})" );
        }
    }

    [Fact]
    public void Multiply_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left * right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) / ({right})" );
        }
    }

    [Fact]
    public void Divide_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left / right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) % ({right})" );
        }
    }

    [Fact]
    public void Modulo_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left % right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) & ({right})" );
        }
    }

    [Fact]
    public void bitwiseAnd_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left & right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) | ({right})" );
        }
    }

    [Fact]
    public void BitwiseOr_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left | right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) ^ ({right})" );
        }
    }

    [Fact]
    public void BitwiseXor_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left ^ right );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) << ({right})" );
        }
    }

    [Fact]
    public void BitwiseLeftShift_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left.BitwiseLeftShift( right ) );

        action.Should().ThrowExactly<SqlNodeException>();
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
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Left.Should().BeSameAs( left );
            sut.Right.Should().BeSameAs( right );
            text.Should().Be( $"({left}) >> ({right})" );
        }
    }

    [Fact]
    public void BitwiseRightShift_ShouldThrowSqlNodeException_WhenOperandTypesAreIncompatible()
    {
        var left = SqlNode.Literal( 42 );
        var right = SqlNode.Parameter<string>( "foo" );

        var action = Lambda.Of( () => left.BitwiseRightShift( right ) );

        action.Should().ThrowExactly<SqlNodeException>();
    }
}
