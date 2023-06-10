using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Decorators;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class ExpressionDecoratorsTests : TestsBase
{
    [Fact]
    public void Filtered_ShouldCreateFilterDataSourceDecoratorNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var condition = SqlNode.RawCondition( "bar > 10" );
        var filter = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        filter.WithAnyArgs( _ => condition );
        var sut = dataSource.Where( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            filter.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.FilterDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should()
                .Be(
                    @"AND WHERE
    (bar > 10)" );
        }
    }

    [Fact]
    public void AndFiltered_ShouldCreateFilterDataSourceDecoratorNode_WithDecorator()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var condition = SqlNode.RawCondition( "bar > 10" );
        var filter = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        filter.WithAnyArgs( _ => condition );
        var sut = decorator.AndWhere( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            filter.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.FilterDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"AND WHERE
    (bar > 10)" );
        }
    }

    [Fact]
    public void OrFiltered_ShouldCreateFilterDataSourceDecoratorNode_WithDecorator()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var condition = SqlNode.RawCondition( "bar > 10" );
        var filter = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        filter.WithAnyArgs( _ => condition );
        var sut = decorator.OrWhere( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            filter.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.FilterDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeFalse();
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"OR WHERE
    (bar > 10)" );
        }
    }

    [Fact]
    public void OrderByAsc_ShouldCreateOrderByNode()
    {
        var expression = SqlNode.Parameter<int>( "a" );
        var sut = expression.Asc();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OrderBy );
            sut.Expression.Should().BeSameAs( expression );
            sut.Ordering.Should().BeSameAs( OrderBy.Asc );
            text.Should().Be( "(@a : System.Int32) ASC" );
        }
    }

    [Fact]
    public void OrderByDesc_ShouldCreateOrderByNode()
    {
        var expression = SqlNode.Parameter<int>( "a" );
        var sut = expression.Desc();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OrderBy );
            sut.Expression.Should().BeSameAs( expression );
            sut.Ordering.Should().BeSameAs( OrderBy.Desc );
            text.Should().Be( "(@a : System.Int32) DESC" );
        }
    }

    [Fact]
    public void Ordered_ShouldCreateSortDataSourceDecoratorNode_WithDataSourceAndOrderingCollection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var ordering = new[] { dataSource.From.GetField( "a" ).Asc(), dataSource.From.GetField( "b" ).Desc() }.ToList();
        var order = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlOrderByNode>>>();
        order.WithAnyArgs( _ => ordering );
        var sut = dataSource.OrderBy( order );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            order.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.SortDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Ordering.ToArray().Should().BeSequentiallyEqualTo( ordering );
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should()
                .Be(
                    @"ORDER BY
    ([foo].[a] : ?) ASC,
    ([foo].[b] : ?) DESC" );
        }
    }

    [Fact]
    public void Ordered_ShouldCreateSortDataSourceDecoratorNode_WithDataSourceAndOrderingArray()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var ordering = new[] { dataSource.From.GetField( "a" ).Asc(), dataSource.From.GetField( "b" ).Desc() };
        var sut = dataSource.OrderBy( ordering );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SortDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Ordering.ToArray().Should().BeSequentiallyEqualTo( ordering );
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should()
                .Be(
                    @"ORDER BY
    ([foo].[a] : ?) ASC,
    ([foo].[b] : ?) DESC" );
        }
    }

    [Fact]
    public void Ordered_ShouldCreateSortDataSourceDecoratorNode_WithDecoratorAndOrderingCollection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var ordering = new[] { dataSource.From.GetField( "a" ).Asc(), dataSource.From.GetField( "b" ).Desc() }.ToList();
        var order = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlOrderByNode>>>();
        order.WithAnyArgs( _ => ordering );
        var sut = decorator.OrderBy( order );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            order.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.SortDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Ordering.ToArray().Should().BeSequentiallyEqualTo( ordering );
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"ORDER BY
    ([foo].[a] : ?) ASC,
    ([foo].[b] : ?) DESC" );
        }
    }

    [Fact]
    public void Ordered_ShouldCreateSortDataSourceDecoratorNode_WithDecoratorAndOrderingArray()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var ordering = new[] { dataSource.From.GetField( "a" ).Asc(), dataSource.From.GetField( "b" ).Desc() };
        var sut = decorator.OrderBy( ordering );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SortDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Ordering.ToArray().Should().BeSequentiallyEqualTo( ordering );
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"ORDER BY
    ([foo].[a] : ?) ASC,
    ([foo].[b] : ?) DESC" );
        }
    }

    [Fact]
    public void Ordered_ShouldCreateSortDataSourceDecoratorNode_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.OrderBy();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SortDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Ordering.ToArray().Should().BeEmpty();
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should().Be( "ORDER BY" );
        }
    }

    [Fact]
    public void Aggregated_ShouldCreateAggregationDataSourceDecoratorNode_WithDataSourceAndExpressionsCollection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var expressions = new SqlExpressionNode[] { dataSource.From.GetField( "a" ), dataSource.From.GetField( "b" ) }.ToList();
        var expr = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        expr.WithAnyArgs( _ => expressions );
        var sut = dataSource.GroupBy( expr );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            expr.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.AggregationDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Expressions.ToArray().Should().BeSequentiallyEqualTo( expressions );
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should()
                .Be(
                    @"GROUP BY
    ([foo].[a] : ?),
    ([foo].[b] : ?)" );
        }
    }

    [Fact]
    public void Aggregated_ShouldCreateAggregationDataSourceDecoratorNode_WithDataSourceAndExpressionsArray()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var expressions = new SqlExpressionNode[] { dataSource.From.GetField( "a" ), dataSource.From.GetField( "b" ) };
        var sut = dataSource.GroupBy( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Expressions.ToArray().Should().BeSequentiallyEqualTo( expressions );
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should()
                .Be(
                    @"GROUP BY
    ([foo].[a] : ?),
    ([foo].[b] : ?)" );
        }
    }

    [Fact]
    public void Aggregated_ShouldCreateAggregationDataSourceDecoratorNode_WithDecoratorAndExpressionsCollection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var expressions = new SqlExpressionNode[] { dataSource.From.GetField( "a" ), dataSource.From.GetField( "b" ) }.ToList();
        var expr = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        expr.WithAnyArgs( _ => expressions );
        var sut = decorator.GroupBy( expr );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            expr.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.AggregationDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Expressions.ToArray().Should().BeSequentiallyEqualTo( expressions );
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"GROUP BY
    ([foo].[a] : ?),
    ([foo].[b] : ?)" );
        }
    }

    [Fact]
    public void Aggregated_ShouldCreateAggregationDataSourceDecoratorNode_WithDecoratorAndExpressionsArray()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var expressions = new SqlExpressionNode[] { dataSource.From.GetField( "a" ), dataSource.From.GetField( "b" ) };
        var sut = decorator.GroupBy( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Expressions.ToArray().Should().BeSequentiallyEqualTo( expressions );
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"GROUP BY
    ([foo].[a] : ?),
    ([foo].[b] : ?)" );
        }
    }

    [Fact]
    public void Aggregated_ShouldCreateAggregationDataSourceDecoratorNode_WithEmptyExpressions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.GroupBy();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Expressions.ToArray().Should().BeEmpty();
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should().Be( "GROUP BY" );
        }
    }

    [Fact]
    public void AggregationFiltered_ShouldCreateAggregationFilterDataSourceDecoratorNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var condition = SqlNode.RawCondition( "bar > 10" );
        var filter = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        filter.WithAnyArgs( _ => condition );
        var sut = dataSource.Having( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            filter.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.AggregationFilterDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should()
                .Be(
                    @"AND HAVING
    (bar > 10)" );
        }
    }

    [Fact]
    public void AndAggregationFiltered_ShouldCreateAggregationFilterDataSourceDecoratorNode_WithDecorator()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var condition = SqlNode.RawCondition( "bar > 10" );
        var filter = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        filter.WithAnyArgs( _ => condition );
        var sut = decorator.AndHaving( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            filter.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.AggregationFilterDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"AND HAVING
    (bar > 10)" );
        }
    }

    [Fact]
    public void OrAggregationFiltered_ShouldCreateAggregationFilterDataSourceDecoratorNode_WithDecorator()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Limit( SqlNode.Literal( 10 ) );
        var condition = SqlNode.RawCondition( "bar > 10" );
        var filter = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        filter.WithAnyArgs( _ => condition );
        var sut = decorator.OrHaving( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            filter.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.AggregationFilterDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeFalse();
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should()
                .Be(
                    @"OR HAVING
    (bar > 10)" );
        }
    }

    [Fact]
    public void Distinct_ShouldCreateDistinctDataSourceDecoratorNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DistinctDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should().Be( "DISTINCT" );
        }
    }

    [Fact]
    public void Distinct_ShouldCreateDistinctDataSourceDecoratorNode_WithDecorator()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Where( SqlNode.True() );
        var sut = decorator.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DistinctDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should().Be( "DISTINCT" );
        }
    }

    [Fact]
    public void Limit_ShouldCreateLimitDataSourceDecoratorNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LimitDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should().Be( "LIMIT (\"10\" : System.Int32)" );
        }
    }

    [Fact]
    public void Limit_ShouldCreateLimitDataSourceDecoratorNode_WithDecorator()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Where( SqlNode.True() );
        var value = SqlNode.Literal( 10 );
        var sut = decorator.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LimitDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should().Be( "LIMIT (\"10\" : System.Int32)" );
        }
    }

    [Fact]
    public void Offset_ShouldCreateOffsetDataSourceDecoratorNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OffsetDecorator );
            sut.Base.Should().BeNull();
            sut.Level.Should().Be( 1 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Reduce().Should().BeSequentiallyEqualTo( sut );
            text.Should().Be( "OFFSET (\"10\" : System.Int32)" );
        }
    }

    [Fact]
    public void Offset_ShouldCreateOffsetDataSourceDecoratorNode_WithDecorator()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var decorator = dataSource.Where( SqlNode.True() );
        var value = SqlNode.Literal( 10 );
        var sut = decorator.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OffsetDecorator );
            sut.Base.Should().BeSameAs( decorator );
            sut.Level.Should().Be( 2 );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Reduce().Should().BeSequentiallyEqualTo( decorator, sut );
            text.Should().Be( "OFFSET (\"10\" : System.Int32)" );
        }
    }
}
