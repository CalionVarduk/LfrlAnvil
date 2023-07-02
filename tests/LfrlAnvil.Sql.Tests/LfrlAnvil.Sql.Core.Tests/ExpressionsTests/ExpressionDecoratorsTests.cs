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
    public void DistinctDecorator_ShouldCreateDistinctDataSourceDecoratorNode()
    {
        var sut = SqlNode.DistinctDecorator();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DistinctDecorator );
            text.Should().Be( "DISTINCT" );
        }
    }

    [Fact]
    public void DistinctDecorator_ForAggregateFunction_ShouldCreateDistinctAggregateFunctionDecoratorNode()
    {
        var sut = SqlNode.AggregateFunctions.DistinctDecorator();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DistinctDecorator );
            text.Should().Be( "DISTINCT" );
        }
    }

    [Fact]
    public void FilterDecorator_ShouldCreateFilterDataSourceDecoratorNode_AsConjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.FilterDecorator( condition, isConjunction: true );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FilterDecorator );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            text.Should()
                .Be(
                    @"AND WHERE
    (bar > 10)" );
        }
    }

    [Fact]
    public void FilterDecorator_ShouldCreateFilterDataSourceDecoratorNode_AsDisjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.FilterDecorator( condition, isConjunction: false );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FilterDecorator );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeFalse();
            text.Should()
                .Be(
                    @"OR WHERE
    (bar > 10)" );
        }
    }

    [Fact]
    public void FilterDecorator_ForAggregateFunction_ShouldCreateFilterAggregateFunctionDecoratorNode_AsConjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregateFunctions.FilterDecorator( condition, isConjunction: true );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FilterDecorator );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            text.Should()
                .Be(
                    @"AND WHERE
    (bar > 10)" );
        }
    }

    [Fact]
    public void FilterDecorator_ForAggregateFunction_ShouldCreateFilterAggregateFunctionDecoratorNode_AsDisjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregateFunctions.FilterDecorator( condition, isConjunction: false );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FilterDecorator );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeFalse();
            text.Should()
                .Be(
                    @"OR WHERE
    (bar > 10)" );
        }
    }

    [Fact]
    public void AggregationDecorator_ShouldCreateAggregationDataSourceDecoratorNode()
    {
        var expressions = new SqlExpressionNode[] { SqlNode.RawExpression( "a" ), SqlNode.RawExpression( "b" ) };
        var sut = SqlNode.AggregationDecorator( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationDecorator );
            sut.Expressions.ToArray().Should().BeSequentiallyEqualTo( expressions );
            text.Should()
                .Be(
                    @"GROUP BY
    (a),
    (b)" );
        }
    }

    [Fact]
    public void AggregationDecorator_ShouldCreateAggregationDataSourceDecoratorNode_WithEmptyExpressions()
    {
        var sut = SqlNode.AggregationDecorator();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationDecorator );
            sut.Expressions.ToArray().Should().BeEmpty();
            text.Should().Be( "GROUP BY" );
        }
    }

    [Fact]
    public void AggregationFilterDecorator_ShouldCreateAggregationFilterDataSourceDecoratorNode_AsConjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregationFilterDecorator( condition, isConjunction: true );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationFilterDecorator );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            text.Should()
                .Be(
                    @"AND HAVING
    (bar > 10)" );
        }
    }

    [Fact]
    public void AggregationFilterDecorator_ShouldCreateAggregationFilterDataSourceDecoratorNode_AsDisjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregationFilterDecorator( condition, isConjunction: false );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationFilterDecorator );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeFalse();
            text.Should()
                .Be(
                    @"OR HAVING
    (bar > 10)" );
        }
    }

    [Fact]
    public void SortDecorator_ShouldCreateSortQueryDecoratorNode()
    {
        var ordering = new[] { SqlNode.RawExpression( "a" ).Asc(), SqlNode.RawExpression( "b" ).Desc() };
        var sut = SqlNode.SortDecorator( ordering );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SortDecorator );
            sut.Ordering.ToArray().Should().BeSequentiallyEqualTo( ordering );
            text.Should()
                .Be(
                    @"ORDER BY
    (a) ASC,
    (b) DESC" );
        }
    }

    [Fact]
    public void SortDecorator_ShouldCreateSortQueryDecoratorNode_WithEmptyOrdering()
    {
        var sut = SqlNode.SortDecorator();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SortDecorator );
            sut.Ordering.ToArray().Should().BeEmpty();
            text.Should().Be( "ORDER BY" );
        }
    }

    [Fact]
    public void LimitDecorator_ShouldCreateLimitQueryDecoratorNode()
    {
        var value = SqlNode.Literal( 10 );
        var sut = SqlNode.LimitDecorator( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LimitDecorator );
            sut.Value.Should().BeSameAs( value );
            text.Should().Be( "LIMIT (\"10\" : System.Int32)" );
        }
    }

    [Fact]
    public void OffsetDecorator_ShouldCreateOffsetQueryDecoratorNode()
    {
        var value = SqlNode.Literal( 10 );
        var sut = SqlNode.OffsetDecorator( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OffsetDecorator );
            sut.Value.Should().BeSameAs( value );
            text.Should().Be( "OFFSET (\"10\" : System.Int32)" );
        }
    }

    [Fact]
    public void CommonTableExpressionDecorator_ShouldCreateCommonTableExpressionQueryDecoratorNode()
    {
        var cte = new SqlCommonTableExpressionNode[]
        {
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM foo" ), "A" ),
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM bar" ), "B" )
        };

        var sut = SqlNode.CommonTableExpressionDecorator( cte );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CommonTableExpressionDecorator );
            sut.CommonTableExpressions.ToArray().Should().BeSequentiallyEqualTo( cte );
            text.Should()
                .Be(
                    @"WITH
    ORDINAL [A] (
        SELECT * FROM foo
    ),
    ORDINAL [B] (
        SELECT * FROM bar
    )" );
        }
    }

    [Fact]
    public void CommonTableExpressionDecorator_ShouldCreateCommonTableExpressionQueryDecoratorNode_WithEmptyTables()
    {
        var sut = SqlNode.CommonTableExpressionDecorator();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CommonTableExpressionDecorator );
            sut.CommonTableExpressions.ToArray().Should().BeEmpty();
            text.Should().Be( "WITH" );
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
    public void OrderByAsc_ShouldCreateOrderByNode_FromSelection()
    {
        var selection = SqlNode.RawSelect( "foo", "bar" );
        var sut = selection.Asc();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OrderBy );
            sut.Expression.Should().BeEquivalentTo( selection.ToExpression() );
            sut.Ordering.Should().BeSameAs( OrderBy.Asc );
            text.Should().Be( "(([foo] : ?) AS [bar]) ASC" );
        }
    }

    [Fact]
    public void OrderByDesc_ShouldCreateOrderByNode_FromSelection()
    {
        var selection = SqlNode.RawSelect( "foo", "bar" );
        var sut = selection.Desc();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OrderBy );
            sut.Expression.Should().BeEquivalentTo( selection.ToExpression() );
            sut.Ordering.Should().BeSameAs( OrderBy.Desc );
            text.Should().Be( "(([foo] : ?) AS [bar]) DESC" );
        }
    }

    [Fact]
    public void Distinct_ForSingleDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
DISTINCT" );
        }
    }

    [Fact]
    public void Distinct_ForMultiDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
DISTINCT" );
        }
    }

    [Fact]
    public void Distinct_ForAggregateFunction_ShouldReturnDecoratedAggregateFunction()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var sut = function.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( function );
            sut.NodeType.Should().Be( function.NodeType );
            sut.FunctionType.Should().Be( function.FunctionType );
            sut.Type.Should().Be( function.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( function.Arguments.ToArray() );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
            text.Should()
                .Be(
                    @"AGG_COUNT((*))
    DISTINCT" );
        }
    }

    [Fact]
    public void AndWhere_ForSingleDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.AndWhere( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (a > 10)" );
        }
    }

    [Fact]
    public void AndWhere_ForMultiDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.AndWhere( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
AND WHERE
    (a > 10)" );
        }
    }

    [Fact]
    public void AndWhere_ForAggregateFunction_ShouldReturnDecoratedAggregateFunction()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var sut = function.AndWhere( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( function );
            sut.NodeType.Should().Be( function.NodeType );
            sut.FunctionType.Should().Be( function.FunctionType );
            sut.Type.Should().Be( function.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( function.Arguments.ToArray() );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"AGG_COUNT((*))
    AND WHERE
        (a > 10)" );
        }
    }

    [Fact]
    public void OrWhere_ForSingleDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.OrWhere( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
OR WHERE
    (a > 10)" );
        }
    }

    [Fact]
    public void OrWhere_ForMultiDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.OrWhere( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
OR WHERE
    (a > 10)" );
        }
    }

    [Fact]
    public void OrWhere_ForAggregateFunction_ShouldReturnDecoratedAggregateFunction()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var sut = function.OrWhere( filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( function );
            sut.NodeType.Should().Be( function.NodeType );
            sut.FunctionType.Should().Be( function.FunctionType );
            sut.Type.Should().Be( function.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( function.Arguments.ToArray() );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"AGG_COUNT((*))
    OR WHERE
        (a > 10)" );
        }
    }

    [Fact]
    public void GroupBy_ForSingleDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var expressions = new SqlExpressionNode[] { dataSource.From["a"] };
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => expressions );
        var sut = dataSource.GroupBy( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
GROUP BY
    ([foo].[a] : ?)" );
        }
    }

    [Fact]
    public void GroupBy_ForSingleDataSource_ShouldReturnDataSource_WithEmptyExpressions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlExpressionNode>() );
        var sut = dataSource.GroupBy( selector );

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().BeSameAs( dataSource );
        }
    }

    [Fact]
    public void GroupBy_ForMultiDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var expressions = new SqlExpressionNode[] { dataSource.From["a"] };
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => expressions );
        var sut = dataSource.GroupBy( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
GROUP BY
    ([foo].[a] : ?)" );
        }
    }

    [Fact]
    public void GroupBy_ForMultiDataSource_ShouldReturnDataSource_WithEmptyExpressions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlExpressionNode>() );
        var sut = dataSource.GroupBy( selector );

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().BeSameAs( dataSource );
        }
    }

    [Fact]
    public void AndHaving_ForSingleDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.AndHaving( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
AND HAVING
    (a > 10)" );
        }
    }

    [Fact]
    public void AndHaving_ForMultiDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.AndHaving( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
AND HAVING
    (a > 10)" );
        }
    }

    [Fact]
    public void OrHaving_ForSingleDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.OrHaving( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
OR HAVING
    (a > 10)" );
        }
    }

    [Fact]
    public void OrHaving_ForMultiDataSource_ShouldReturnDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = dataSource.OrHaving( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
OR HAVING
    (a > 10)" );
        }
    }

    [Fact]
    public void OrderBy_ForSingleDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var ordering = new[] { dataSource.From["a"].Asc() };
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlOrderByNode>>>();
        selector.WithAnyArgs( _ => ordering );
        var sut = dataSource.OrderBy( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
ORDER BY
    ([foo].[a] : ?) ASC
SELECT" );
        }
    }

    [Fact]
    public void OrderBy_ForSingleDataSource_ShouldReturnDataSourceQuery_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.OrderBy( Enumerable.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT" );
        }
    }

    [Fact]
    public void OrderBy_ForMultiDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var ordering = new[] { dataSource["foo"]["a"].Asc(), dataSource["bar"]["b"].Desc() };
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, IEnumerable<SqlOrderByNode>>>();
        selector.WithAnyArgs( _ => ordering );
        var sut = dataSource.OrderBy( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
ORDER BY
    ([foo].[a] : ?) ASC,
    ([bar].[b] : ?) DESC
SELECT" );
        }
    }

    [Fact]
    public void OrderBy_ForMultiDataSource_ShouldReturnDataSourceQuery_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.OrderBy( Enumerable.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
SELECT" );
        }
    }

    [Fact]
    public void With_ForSingleDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var cte = new[] { SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "A" ) };
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlCommonTableExpressionNode>>>();
        selector.WithAnyArgs( _ => cte );
        var sut = dataSource.With( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
WITH
    ORDINAL [A] (
        SELECT * FROM bar
    )
SELECT" );
        }
    }

    [Fact]
    public void With_ForSingleDataSource_ShouldReturnDataSourceQuery_WithEmptyCte()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.With( Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT" );
        }
    }

    [Fact]
    public void With_ForMultiDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var firstCte = SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "A" );
        var secondCte = SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "B" );
        var dataSource = firstCte.RecordSet.Join( secondCte.RecordSet.As( "C" ).InnerOn( SqlNode.True() ) );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, IEnumerable<SqlCommonTableExpressionNode>>>();
        selector.WithAnyArgs( _ => new[] { firstCte, secondCte } );
        var sut = dataSource.With( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionDecorator );
            text.Should()
                .Be(
                    @"FROM [A]
INNER JOIN [B] AS [C] ON
    (TRUE)
WITH
    ORDINAL [A] (
        SELECT * FROM foo
    ),
    ORDINAL [B] (
        SELECT * FROM bar
    )
SELECT" );
        }
    }

    [Fact]
    public void With_ForMultiDataSource_ShouldReturnDataSourceQuery_WithEmptyCte()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.With( Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
SELECT" );
        }
    }

    [Fact]
    public void Limit_ForSingleDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
LIMIT (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Limit_ForMultiDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
LIMIT (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Offset_ForSingleDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
OFFSET (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Offset_ForMultiDataSource_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            sut.Type.Should().BeNull();
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
OFFSET (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Distinct_ForDataSourceQuery_ShouldReturnQueryWithDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var sut = query.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.Should().BeSequentiallyEqualTo( query.Decorators );
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().NotBeSameAs( dataSource );
            sut.DataSource.From.Should().BeSameAs( dataSource.From );
            sut.DataSource.Joins.Should().Be( dataSource.Joins );
            sut.DataSource.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.DataSource.Decorators.Should().HaveCount( 1 );
            (sut.DataSource.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
DISTINCT
SELECT" );
        }
    }

    [Fact]
    public void AndWhere_ForDataSourceQuery_ShouldReturnQueryWithDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = query.AndWhere( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.Should().BeSequentiallyEqualTo( query.Decorators );
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().NotBeSameAs( dataSource );
            sut.DataSource.From.Should().BeSameAs( dataSource.From );
            sut.DataSource.Joins.Should().Be( dataSource.Joins );
            sut.DataSource.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.DataSource.Decorators.Should().HaveCount( 1 );
            (sut.DataSource.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE
    (a > 10)
SELECT" );
        }
    }

    [Fact]
    public void OrWhere_ForDataSourceQuery_ShouldReturnQueryWithDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = query.OrWhere( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.Should().BeSequentiallyEqualTo( query.Decorators );
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().NotBeSameAs( dataSource );
            sut.DataSource.From.Should().BeSameAs( dataSource.From );
            sut.DataSource.Joins.Should().Be( dataSource.Joins );
            sut.DataSource.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.DataSource.Decorators.Should().HaveCount( 1 );
            (sut.DataSource.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
OR WHERE
    (a > 10)
SELECT" );
        }
    }

    [Fact]
    public void GroupBy_ForDataSourceQuery_ShouldReturnQueryWithDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var expressions = new[] { dataSource.From["a"], dataSource.From["b"] };
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => expressions );
        var sut = query.GroupBy( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.Should().BeSequentiallyEqualTo( query.Decorators );
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().NotBeSameAs( dataSource );
            sut.DataSource.From.Should().BeSameAs( dataSource.From );
            sut.DataSource.Joins.Should().Be( dataSource.Joins );
            sut.DataSource.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.DataSource.Decorators.Should().HaveCount( 1 );
            (sut.DataSource.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
GROUP BY
    ([foo].[a] : ?),
    ([foo].[b] : ?)
SELECT" );
        }
    }

    [Fact]
    public void GroupBy_ForDataSourceQuery_ShouldReturnQuery_WhenExpressionsAreEmpty()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlExpressionNode>() );
        var sut = query.GroupBy( selector );

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().BeSameAs( query );
        }
    }

    [Fact]
    public void AndHaving_ForDataSourceQuery_ShouldReturnQueryWithDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = query.AndHaving( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.Should().BeSequentiallyEqualTo( query.Decorators );
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().NotBeSameAs( dataSource );
            sut.DataSource.From.Should().BeSameAs( dataSource.From );
            sut.DataSource.Joins.Should().Be( dataSource.Joins );
            sut.DataSource.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.DataSource.Decorators.Should().HaveCount( 1 );
            (sut.DataSource.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
AND HAVING
    (a > 10)
SELECT" );
        }
    }

    [Fact]
    public void OrHaving_ForDataSourceQuery_ShouldReturnQueryWithDecoratedDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var filter = SqlNode.RawCondition( "a > 10" );
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, SqlConditionNode>>();
        selector.WithAnyArgs( _ => filter );
        var sut = query.OrHaving( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Decorators.Should().BeSequentiallyEqualTo( query.Decorators );
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().NotBeSameAs( dataSource );
            sut.DataSource.From.Should().BeSameAs( dataSource.From );
            sut.DataSource.Joins.Should().Be( dataSource.Joins );
            sut.DataSource.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.DataSource.Decorators.Should().HaveCount( 1 );
            (sut.DataSource.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
OR HAVING
    (a > 10)
SELECT" );
        }
    }

    [Fact]
    public void OrderBy_ForDataSourceQuery_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var ordering = new[] { dataSource.From["a"].Asc(), dataSource.From["b"].Desc() };
        var selector = Substitute.For<
            Func<SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlRawRecordSetNode>>, IEnumerable<SqlOrderByNode>>>();

        selector.WithAnyArgs( _ => ordering );
        var sut = query.OrderBy( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( query.DataSource );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
ORDER BY
    ([foo].[a] : ?) ASC,
    ([foo].[b] : ?) DESC
SELECT" );
        }
    }

    [Fact]
    public void OrderBy_ForDataSourceQuery_ShouldReturnDataSourceQuery_WhenOrderingIsEmpty()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var selector = Substitute.For<
            Func<SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlRawRecordSetNode>>, IEnumerable<SqlOrderByNode>>>();

        selector.WithAnyArgs( _ => Enumerable.Empty<SqlOrderByNode>() );
        var sut = query.OrderBy( selector );

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().BeSameAs( query );
        }
    }

    [Fact]
    public void OrderBy_ForCompoundQuery_ShouldReturnDecoratedCompoundQuery()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var ordering = new[] { SqlNode.RawExpression( "a" ).Asc(), SqlNode.RawExpression( "b" ).Desc() };
        var selector = Substitute.For<Func<SqlCompoundQueryExpressionNode, IEnumerable<SqlOrderByNode>>>();
        selector.WithAnyArgs( _ => ordering );
        var sut = query.OrderBy( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.CompoundQuery );
            sut.FirstQuery.Should().BeSameAs( query.FirstQuery );
            sut.FollowingQueries.Should().Be( query.FollowingQueries );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortDecorator );
            text.Should()
                .Be(
                    @"(
    SELECT * FROM foo
)
UNION
(
    SELECT * FROM bar
)
ORDER BY
    (a) ASC,
    (b) DESC" );
        }
    }

    [Fact]
    public void OrderBy_ForCompoundQuery_ShouldReturnCompoundQuery_WhenOrderingIsEmpty()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var selector = Substitute.For<Func<SqlCompoundQueryExpressionNode, IEnumerable<SqlOrderByNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlOrderByNode>() );
        var sut = query.OrderBy( selector );

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().BeSameAs( query );
        }
    }

    [Fact]
    public void With_ForDataSourceQuery_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var cte = new[] { SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "A" ) };
        var selector = Substitute.For<
            Func<SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlRawRecordSetNode>>,
                IEnumerable<SqlCommonTableExpressionNode>>>();

        selector.WithAnyArgs( _ => cte );
        var sut = query.With( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( query.DataSource );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
WITH
    ORDINAL [A] (
        SELECT * FROM bar
    )
SELECT" );
        }
    }

    [Fact]
    public void With_ForDataSourceQuery_ShouldReturnDataSourceQuery_WhenCteAreEmpty()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var selector = Substitute.For<
            Func<SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlRawRecordSetNode>>,
                IEnumerable<SqlCommonTableExpressionNode>>>();

        selector.WithAnyArgs( _ => Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var sut = query.With( selector );

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().BeSameAs( query );
        }
    }

    [Fact]
    public void With_ForCompoundQuery_ShouldReturnDecoratedCompoundQuery()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var cte = new[] { SqlNode.RawQuery( "SELECT * FROM qux" ).ToCte( "A" ) };
        var selector = Substitute.For<Func<SqlCompoundQueryExpressionNode, IEnumerable<SqlCommonTableExpressionNode>>>();
        selector.WithAnyArgs( _ => cte );
        var sut = query.With( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.CompoundQuery );
            sut.FirstQuery.Should().BeSameAs( query.FirstQuery );
            sut.FollowingQueries.Should().Be( query.FollowingQueries );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionDecorator );
            text.Should()
                .Be(
                    @"(
    SELECT * FROM foo
)
UNION
(
    SELECT * FROM bar
)
WITH
    ORDINAL [A] (
        SELECT * FROM qux
    )" );
        }
    }

    [Fact]
    public void With_ForCompoundQuery_ShouldReturnCompoundQuery_WhenCteAreEmpty()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var selector = Substitute.For<Func<SqlCompoundQueryExpressionNode, IEnumerable<SqlCommonTableExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var sut = query.With( selector );

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( query );
            sut.Should().BeSameAs( query );
        }
    }

    [Fact]
    public void Limit_ForDataSourceQuery_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var value = SqlNode.Literal( 10 );
        var sut = query.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( query.DataSource );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
LIMIT (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Limit_ForCompoundQuery_ShouldReturnDecoratedCompoundQuery()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var value = SqlNode.Literal( 10 );
        var sut = query.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.CompoundQuery );
            sut.FirstQuery.Should().BeSameAs( query.FirstQuery );
            sut.FollowingQueries.Should().Be( query.FollowingQueries );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitDecorator );
            text.Should()
                .Be(
                    @"(
    SELECT * FROM foo
)
UNION
(
    SELECT * FROM bar
)
LIMIT (""10"" : System.Int32)" );
        }
    }

    [Fact]
    public void Offset_ForDataSourceQuery_ShouldReturnDecoratedDataSourceQuery()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var value = SqlNode.Literal( 10 );
        var sut = query.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( query.DataSource );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetDecorator );
            text.Should()
                .Be(
                    @"FROM [foo]
OFFSET (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Offset_ForCompoundQuery_ShouldReturnDecoratedCompoundQuery()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var value = SqlNode.Literal( 10 );
        var sut = query.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.CompoundQuery );
            sut.FirstQuery.Should().BeSameAs( query.FirstQuery );
            sut.FollowingQueries.Should().Be( query.FollowingQueries );
            sut.Selection.Should().Be( query.Selection );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetDecorator );
            text.Should()
                .Be(
                    @"(
    SELECT * FROM foo
)
UNION
(
    SELECT * FROM bar
)
OFFSET (""10"" : System.Int32)" );
        }
    }
}
