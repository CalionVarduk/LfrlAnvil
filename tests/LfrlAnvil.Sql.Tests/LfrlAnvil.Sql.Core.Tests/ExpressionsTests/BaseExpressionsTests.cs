using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class BaseExpressionsTests : TestsBase
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
            sut.TargetType.Should().Be( typeof( long ) );
            sut.Node.Should().BeSameAs( node );
            text.Should().Be( $"CAST(({node}) AS System.Int64)" );
        }
    }

    [Fact]
    public void RawExpression_ShouldCreateRawExpressionNode()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
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
            sut.DataSource.Should().BeSameAs( dataSource );
            text.Should().Be( "*" );
        }
    }

    [Fact]
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithNonEmptySelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = new[] { dataSource.From["bar"].As( "x" ), dataSource.From["qux"].AsSelf() }.ToList();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlSelectNode>>>();
        selector.WithAnyArgs( _ => selection );
        var sut = dataSource.Select( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.ToArray().Should().BeEmpty();
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
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithEmptySelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Select();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.ToArray().Should().BeEmpty();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT" );
        }
    }

    [Fact]
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithSingleSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From.GetRawField( "bar", SqlExpressionType.Create<int>() ).AsSelf();
        var sut = dataSource.Select( selection );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.ToArray().Should().BeEmpty();
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
    public void RawQuery_ShouldCreateRawQueryExpressionNode()
    {
        var sql = @"SELECT *
FROM foo
WHERE id = @a AND value > @b";

        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawQuery( sql, parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawQuery );
            sut.Sql.Should().Be( sql );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            sut.Selection.ToArray().Should().BeEmpty();
            text.Should().Be( sql );
        }
    }

    [Fact]
    public void CompoundQuery_ShouldCreateCompoundQueryExpressionNode_WithNonEmptyComponents()
    {
        var query1 = SqlNode.RawQuery(
            @"SELECT a, b
FROM foo
WHERE value > 10" );

        var query2 = SqlNode.RawQuery(
            @"SELECT a, c AS b
FROM qux
WHERE value < 10" );

        var union = query2.ToUnion();
        var sut = query1.CompoundWith( new[] { union }.ToList() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQuery );
            sut.FirstQuery.Should().BeSameAs( query1 );
            sut.FollowingQueries.ToArray().Should().BeSequentiallyEqualTo( union );
            text.Should()
                .Be(
                    @"(
    SELECT a, b
    FROM foo
    WHERE value > 10
)
UNION
(
    SELECT a, c AS b
    FROM qux
    WHERE value < 10
)" );
        }
    }

    [Fact]
    public void CompoundQuery_ShouldThrowArgumentException_WhenComponentsAreEmpty()
    {
        var query = SqlNode.RawQuery(
            @"SELECT *
FROM foo
WHERE value > 10" );

        var action = Lambda.Of( () => query.CompoundWith() );

        action.Should().ThrowExactly<ArgumentException>();
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
    public void Switch_ShouldThrowArgumentException_WhenCasesAreEmpty()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var action = Lambda.Of( () => SqlNode.Switch( Enumerable.Empty<SqlSwitchCaseNode>(), defaultNode ) );
        action.Should().ThrowExactly<ArgumentException>();
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
            sut.Default.Should().BeSameAs( whenFalse );
            sut.Cases.ToArray().Should().HaveCount( 1 );
            (sut.Cases.ToArray().ElementAtOrDefault( 0 )?.Condition).Should().BeSameAs( condition );
            (sut.Cases.ToArray().ElementAtOrDefault( 0 )?.Expression).Should().BeSameAs( whenTrue );
        }
    }

    [Fact]
    public void UnionWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToUnion();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.Union );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"UNION
(
    SELECT * FROM foo
)" );
        }
    }

    [Fact]
    public void UnionAllWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToUnionAll();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.UnionAll );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"UNIONALL
(
    SELECT * FROM foo
)" );
        }
    }

    [Fact]
    public void IntersectWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToIntersect();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.Intersect );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"INTERSECT
(
    SELECT * FROM foo
)" );
        }
    }

    [Fact]
    public void ExceptWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToExcept();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.Except );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"EXCEPT
(
    SELECT * FROM foo
)" );
        }
    }

    [Theory]
    [InlineData( SqlCompoundQueryOperator.Union )]
    [InlineData( SqlCompoundQueryOperator.UnionAll )]
    [InlineData( SqlCompoundQueryOperator.Intersect )]
    [InlineData( SqlCompoundQueryOperator.Except )]
    public void CompoundWith_ShouldCreateSqlCompoundQueryComponentNode(SqlCompoundQueryOperator @operator)
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToCompound( @operator );

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( @operator );
            sut.Query.Should().BeSameAs( query );
        }
    }

    [Fact]
    public void CompoundWith_ShouldThrowArgumentException_WhenOperatorIsUnrecognized()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var action = Lambda.Of( () => query.ToCompound( (SqlCompoundQueryOperator)10 ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void SelectExpression_ShouldCreateSelectExpressionNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From.GetRawField( "bar", SqlExpressionType.Create<int>() ).AsSelf();
        var sut = selection.ToExpression();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectExpression );
            sut.Selection.Should().BeSameAs( selection );
            text.Should().Be( selection.ToString() );
        }
    }

    private sealed class NodeMock : SqlNodeBase
    {
        public NodeMock()
            : base( SqlNodeType.Unknown ) { }
    }
}
