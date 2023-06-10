using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class BaseExpressionsTests : TestsBase
{
    [Fact]
    public void BaseToString_ShouldReturnTypeInfo()
    {
        var sut = new NodeMock();
        var result = sut.ToString();
        result.Should().Be( $"{{{sut.GetType().GetDebugString()}}}" );
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void TypeCast_ShouldCreateTypeCastExpressionNode(bool isNullable)
    {
        var node = SqlNode.Parameter<int>( "foo", isNullable );
        var sut = node.CastTo<long>();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.TypeCast );
            sut.Type.Should().Be( SqlExpressionType.Create<long>( isNullable ) );
            sut.TargetType.Should().Be( typeof( long ) );
            sut.Node.Should().BeSameAs( node );
            text.Should().Be( $"CAST(({node}) AS System.Int64)" );
        }
    }

    [Fact]
    public void TypeCast_ShouldCreateTypeCastExpressionNode_WhenNodeTypeIsUnknown()
    {
        var node = SqlNode.Parameter( "foo" );
        var sut = node.CastTo<long>();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.TypeCast );
            sut.Type.Should().BeNull();
            sut.TargetType.Should().Be( typeof( long ) );
            sut.Node.Should().BeSameAs( node );
            text.Should().Be( $"CAST(({node}) AS System.Int64)" );
        }
    }

    [Fact]
    public void RawExpression_ShouldCreateRawExpressionNode()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) }.ToList();
        var sut = SqlNode.RawExpression( "foo(@a, @b, 10) + 15", SqlExpressionType.Create<int>(), parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawExpression );
            sut.Sql.Should().Be( "foo(@a, @b, 10) + 15" );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            text.Should().Be( "foo(@a, @b, 10) + 15" );
        }
    }

    [Fact]
    public void RawExpression_ShouldCreateRawExpressionNode_WithoutType()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawExpression( "foo(@a, @b, 10) + 15", parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawExpression );
            sut.Sql.Should().Be( "foo(@a, @b, 10) + 15" );
            sut.Type.Should().BeNull();
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            text.Should().Be( "foo(@a, @b, 10) + 15" );
        }
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithExpressionAndAlias()
    {
        var expression = SqlNode.Parameter<int>( "foo" );
        var sut = expression.As( "bar" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectField );
            sut.Type.Should().Be( expression.Type );
            sut.Alias.Should().Be( "bar" );
            sut.Expression.Should().BeSameAs( expression );
            sut.FieldName.Should().Be( "bar" );
            text.Should().Be( $"({expression}) AS [bar]" );
        }
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithDataFieldAndAlias()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = dataField.As( "qux" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectField );
            sut.Type.Should().Be( dataField.Type );
            sut.Alias.Should().Be( "qux" );
            sut.Expression.Should().BeSameAs( dataField );
            sut.FieldName.Should().Be( "qux" );
            text.Should().Be( $"({dataField}) AS [qux]" );
        }
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithDataFieldAndWithoutAlias()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = dataField.AsSelf();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectField );
            sut.Type.Should().Be( dataField.Type );
            sut.Alias.Should().BeNull();
            sut.Expression.Should().BeSameAs( dataField );
            sut.FieldName.Should().Be( "bar" );
            text.Should().Be( $"({dataField})" );
        }
    }

    [Fact]
    public void SelectAll_ShouldCreateSelectRecordSetNode_WithRecordSet()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.GetAll();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectRecordSet );
            sut.Type.Should().BeNull();
            sut.RecordSet.Should().BeSameAs( recordSet );
            text.Should().Be( "[foo].*" );
        }
    }

    [Fact]
    public void SelectAll_ShouldCreateSelectAllNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.GetAll();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectAll );
            sut.Type.Should().BeNull();
            sut.DataSource.Should().BeSameAs( dataSource );
            text.Should().Be( "*" );
        }
    }

    [Fact]
    public void Query_ShouldCreateQueryExpressionNode_FromDataSourceNode_WithSelectionCollection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = new[] { dataSource.From.GetField( "bar" ).As( "x" ), dataSource.From.GetField( "qux" ).AsSelf() }.ToList();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlSelectNode>>>();
        selector.WithAnyArgs( _ => selection );
        var sut = dataSource.Select( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeNull();
            sut.Type.Should().BeNull();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( selection );
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT
    ([foo].[bar] : ?) AS [x],
    ([foo].[qux] : ?)" );
        }
    }

    [Fact]
    public void Query_ShouldCreateQueryExpressionNode_FromDataSourceNode_WithEmptySelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Select();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeNull();
            sut.Type.Should().BeNull();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT" );
        }
    }

    [Fact]
    public void Query_ShouldCreateQueryExpressionNode_FromDataSourceNode_WithSingleSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From.GetRawField( "bar", SqlExpressionType.Create<int>() ).AsSelf();
        var sut = dataSource.Select( selection );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeNull();
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( selection );
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT
    ([foo].[bar] : System.Int32)" );
        }
    }

    [Fact]
    public void Query_ShouldCreateQueryExpressionNode_FromDataSourceDecoratorNode_WithSelectionCollection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Where( SqlNode.True() );
        var selection = new[] { dataSource.From.GetField( "bar" ).As( "x" ), dataSource.From.GetField( "qux" ).AsSelf() }.ToList();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlSelectNode>>>();
        selector.WithAnyArgs( _ => selection );
        var sut = decorator.Select( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeSameAs( decorator );
            sut.Type.Should().BeNull();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( selection );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (TRUE)
SELECT
    ([foo].[bar] : ?) AS [x],
    ([foo].[qux] : ?)" );
        }
    }

    [Fact]
    public void Query_ShouldCreateQueryExpressionNode_FromDataSourceDecoratorNode_WithEmptySelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Where( SqlNode.True() );
        var sut = decorator.Select();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeSameAs( decorator );
            sut.Type.Should().BeNull();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (TRUE)
SELECT" );
        }
    }

    [Fact]
    public void Query_ShouldCreateQueryExpressionNode_FromDataSourceDecoratorNode_WithSingleSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Where( SqlNode.True() );
        var selection = dataSource.From.GetRawField( "bar", SqlExpressionType.Create<int>() ).AsSelf();
        var sut = decorator.Select( selection );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeSameAs( decorator );
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( selection );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (TRUE)
SELECT
    ([foo].[bar] : System.Int32)" );
        }
    }

    [Fact]
    public void Query_AndSelect_ShouldCreateQueryExpressionNode_WithExtendedSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var oldSelection = dataSource.From.GetField( "bar" ).AsSelf();
        var newSelection = dataSource.From.GetField( "qux" ).As( "x" );
        var query = dataSource.Select( oldSelection );
        var selector = Substitute.For<Func<SqlDataSourceNode, IEnumerable<SqlSelectNode>>>();
        selector.WithAnyArgs( _ => new[] { newSelection } );
        var sut = query.AndSelect( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeNull();
            sut.Type.Should().BeNull();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( oldSelection, newSelection );
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT
    ([foo].[bar] : ?),
    ([foo].[qux] : ?) AS [x]" );
        }
    }

    [Fact]
    public void Query_AndSelect_WithDecorator_ShouldCreateQueryExpressionNode_WithExtendedSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Where( SqlNode.True() );
        var oldSelection = dataSource.From.GetField( "bar" ).AsSelf();
        var newSelection = dataSource.From.GetField( "qux" ).As( "x" );
        var query = decorator.Select( oldSelection );
        var selector = Substitute.For<Func<SqlDataSourceNode, IEnumerable<SqlSelectNode>>>();
        selector.WithAnyArgs( _ => new[] { newSelection } );
        var sut = query.AndSelect( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.Query );
            sut.Decorator.Should().BeSameAs( decorator );
            sut.Type.Should().BeNull();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( oldSelection, newSelection );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (TRUE)
SELECT
    ([foo].[bar] : ?),
    ([foo].[qux] : ?) AS [x]" );
        }
    }

    [Fact]
    public void Query_AndSelect_ShouldReturnSelf_WhenNewSelectionIsEmpty()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var oldSelection = dataSource.From.GetField( "bar" ).AsSelf();
        var query = dataSource.Select( oldSelection );

        var sut = query.AndSelect();

        sut.Should().BeSameAs( query );
    }

    [Fact]
    public void SwitchCase_ShouldCreateSwitchCaseNode()
    {
        var condition = SqlNode.RawCondition( "@a > 10", SqlNode.Parameter( "a" ) );
        var expression = SqlNode.Literal( 42 );
        var sut = SqlNode.SwitchCase( condition, expression );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SwitchCase );
            sut.Condition.Should().BeSameAs( condition );
            sut.Expression.Should().BeSameAs( expression );
            text.Should()
                .Be(
                    $@"WHEN ({condition})
    THEN ({expression})" );
        }
    }

    [Fact]
    public void Switch_ShouldCreateSwitchExpressionNode()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var firstCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 10" ), SqlNode.Literal( 10 ) );
        var secondCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar < 5" ), SqlNode.Literal( 15 ) );
        var sut = SqlNode.Switch( new[] { firstCase, secondCase }, defaultNode );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Switch );
            sut.Default.Should().BeSameAs( defaultNode );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            sut.Cases.ToArray().Should().BeSequentiallyEqualTo( firstCase, secondCase );
            text.Should()
                .Be(
                    $@"CASE
    WHEN ({firstCase.Condition})
        THEN ({firstCase.Expression})
    WHEN ({secondCase.Condition})
        THEN ({secondCase.Expression})
    ELSE ({defaultNode})
END" );
        }
    }

    [Fact]
    public void Switch_ShouldCreateSwitchExpressionNode_WithNullType_WhenDefaultNodeHasNullType()
    {
        var defaultNode = SqlNode.Parameter( "foo" );
        var @case = SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 10" ), SqlNode.Literal( 10 ) );
        var sut = SqlNode.Switch( new[] { @case }, defaultNode );

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Switch );
            sut.Default.Should().BeSameAs( defaultNode );
            sut.Type.Should().BeNull();
            sut.Cases.ToArray().Should().BeSequentiallyEqualTo( @case );
        }
    }

    [Fact]
    public void Switch_ShouldCreateSwitchExpressionNode_WithNullType_WhenAnyCaseExpressionNodeHasNullType()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var @case = SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 10" ), SqlNode.RawExpression( "10" ) );
        var sut = SqlNode.Switch( new[] { @case }, defaultNode );

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Switch );
            sut.Default.Should().BeSameAs( defaultNode );
            sut.Type.Should().BeNull();
            sut.Cases.ToArray().Should().BeSequentiallyEqualTo( @case );
        }
    }

    [Fact]
    public void Switch_ShouldThrowArgumentException_WhenCasesAreEmpty()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var action = Lambda.Of( () => SqlNode.Switch( Enumerable.Empty<SqlSwitchCaseNode>(), defaultNode ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Switch_ShouldThrowSqlNodeException_WhenTypesAreIncompatible()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var @case = SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 10" ), SqlNode.Literal( "x" ) );

        var action = Lambda.Of( () => SqlNode.Switch( new[] { @case }, defaultNode ) );

        action.Should().ThrowExactly<SqlNodeException>();
    }

    [Fact]
    public void Iif_ShouldCreateSwitchExpressionNode()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var whenTrue = SqlNode.Literal( 10 );
        var whenFalse = SqlNode.Literal( 15 );
        var sut = SqlNode.Iif( condition, whenTrue, whenFalse );

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Switch );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            sut.Default.Should().BeSameAs( whenFalse );
            sut.Cases.ToArray().Should().HaveCount( 1 );
            (sut.Cases.ToArray().ElementAtOrDefault( 0 )?.Condition).Should().BeSameAs( condition );
            (sut.Cases.ToArray().ElementAtOrDefault( 0 )?.Expression).Should().BeSameAs( whenTrue );
        }
    }

    private sealed class NodeMock : SqlNodeBase
    {
        public NodeMock()
            : base( SqlNodeType.Unknown ) { }
    }
}
