using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class ExpressionTraitsTests : TestsBase
{
    [Fact]
    public void DistinctTrait_ShouldCreateDistinctDataSourceTraitNode()
    {
        var sut = SqlNode.DistinctTrait();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DistinctTrait );
            text.Should().Be( "DISTINCT" );
        }
    }

    [Fact]
    public void FilterTrait_ShouldCreateFilterDataSourceTraitNode_AsConjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.FilterTrait( condition, isConjunction: true );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FilterTrait );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            text.Should().Be( "AND WHERE bar > 10" );
        }
    }

    [Fact]
    public void FilterTrait_ShouldCreateFilterDataSourceTraitNode_AsDisjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.FilterTrait( condition, isConjunction: false );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FilterTrait );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeFalse();
            text.Should().Be( "OR WHERE bar > 10" );
        }
    }

    [Fact]
    public void AggregationTrait_ShouldCreateAggregationDataSourceTraitNode()
    {
        var expressions = new SqlExpressionNode[] { SqlNode.RawExpression( "a" ), SqlNode.RawExpression( "b" ) };
        var sut = SqlNode.AggregationTrait( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationTrait );
            sut.Expressions.ToArray().Should().BeSequentiallyEqualTo( expressions );
            text.Should().Be( "GROUP BY (a), (b)" );
        }
    }

    [Fact]
    public void AggregationTrait_ShouldCreateAggregationDataSourceTraitNode_WithEmptyExpressions()
    {
        var sut = SqlNode.AggregationTrait();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationTrait );
            sut.Expressions.ToArray().Should().BeEmpty();
            text.Should().Be( "GROUP BY" );
        }
    }

    [Fact]
    public void AggregationFilterTrait_ShouldCreateAggregationFilterDataSourceTraitNode_AsConjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregationFilterTrait( condition, isConjunction: true );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationFilterTrait );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeTrue();
            text.Should().Be( "AND HAVING bar > 10" );
        }
    }

    [Fact]
    public void AggregationFilterTrait_ShouldCreateAggregationFilterDataSourceTraitNode_AsDisjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregationFilterTrait( condition, isConjunction: false );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregationFilterTrait );
            sut.Filter.Should().BeSameAs( condition );
            sut.IsConjunction.Should().BeFalse();
            text.Should().Be( "OR HAVING bar > 10" );
        }
    }

    [Fact]
    public void SortTrait_ShouldCreateSortQueryTraitNode()
    {
        var ordering = new[] { SqlNode.RawExpression( "a" ).Asc(), SqlNode.RawExpression( "b" ).Desc() };
        var sut = SqlNode.SortTrait( ordering );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SortTrait );
            sut.Ordering.ToArray().Should().BeSequentiallyEqualTo( ordering );
            text.Should().Be( "ORDER BY (a) ASC, (b) DESC" );
        }
    }

    [Fact]
    public void SortTrait_ShouldCreateSortQueryTraitNode_WithEmptyOrdering()
    {
        var sut = SqlNode.SortTrait();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SortTrait );
            sut.Ordering.ToArray().Should().BeEmpty();
            text.Should().Be( "ORDER BY" );
        }
    }

    [Fact]
    public void LimitTrait_ShouldCreateLimitQueryTraitNode()
    {
        var value = SqlNode.Literal( 10 );
        var sut = SqlNode.LimitTrait( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.LimitTrait );
            sut.Value.Should().BeSameAs( value );
            text.Should().Be( "LIMIT (\"10\" : System.Int32)" );
        }
    }

    [Fact]
    public void OffsetTrait_ShouldCreateOffsetQueryTraitNode()
    {
        var value = SqlNode.Literal( 10 );
        var sut = SqlNode.OffsetTrait( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OffsetTrait );
            sut.Value.Should().BeSameAs( value );
            text.Should().Be( "OFFSET (\"10\" : System.Int32)" );
        }
    }

    [Fact]
    public void CommonTableExpressionTrait_ShouldCreateCommonTableExpressionQueryTraitNode()
    {
        var cte = new SqlCommonTableExpressionNode[]
        {
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM foo" ), "A" ),
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM bar" ), "B" )
        };

        var sut = SqlNode.CommonTableExpressionTrait( cte );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CommonTableExpressionTrait );
            sut.CommonTableExpressions.ToArray().Should().BeSequentiallyEqualTo( cte );
            text.Should()
                .Be(
                    @"WITH ORDINAL [A] (
  SELECT * FROM foo
),
ORDINAL [B] (
  SELECT * FROM bar
)" );
        }
    }

    [Fact]
    public void CommonTableExpressionTrait_ShouldCreateCommonTableExpressionQueryTraitNode_WithEmptyTables()
    {
        var sut = SqlNode.CommonTableExpressionTrait();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CommonTableExpressionTrait );
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
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From["bar"].As( "qux" );
        var sut = selection.Asc();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OrderBy );
            sut.Expression.Should().BeEquivalentTo( selection.ToExpression() );
            sut.Ordering.Should().BeSameAs( OrderBy.Asc );
            text.Should().Be( "([qux]) ASC" );
        }
    }

    [Fact]
    public void OrderByDesc_ShouldCreateOrderByNode_FromSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From["bar"].As( "qux" );
        var sut = selection.Desc();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.OrderBy );
            sut.Expression.Should().BeEquivalentTo( selection.ToExpression() );
            sut.Ordering.Should().BeSameAs( OrderBy.Desc );
            text.Should().Be( "([qux]) DESC" );
        }
    }

    [Fact]
    public void Distinct_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
DISTINCT" );
        }
    }

    [Fact]
    public void Distinct_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
DISTINCT" );
        }
    }

    [Fact]
    public void Distinct_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var sut = function.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( function );
            sut.NodeType.Should().Be( function.NodeType );
            sut.FunctionType.Should().Be( function.FunctionType );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( function.Arguments.ToArray() );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"AGG_COUNT((*))
  DISTINCT" );
        }
    }

    [Fact]
    public void AndWhere_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE a > 10" );
        }
    }

    [Fact]
    public void AndWhere_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
AND WHERE a > 10" );
        }
    }

    [Fact]
    public void AndWhere_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
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
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( function.Arguments.ToArray() );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"AGG_COUNT((*))
  AND WHERE a > 10" );
        }
    }

    [Fact]
    public void OrWhere_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
OR WHERE a > 10" );
        }
    }

    [Fact]
    public void OrWhere_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
OR WHERE a > 10" );
        }
    }

    [Fact]
    public void OrWhere_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
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
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( function.Arguments.ToArray() );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"AGG_COUNT((*))
  OR WHERE a > 10" );
        }
    }

    [Fact]
    public void GroupBy_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
GROUP BY ([foo].[a] : ?)" );
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
    public void GroupBy_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
GROUP BY ([foo].[a] : ?)" );
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
    public void AndHaving_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
AND HAVING a > 10" );
        }
    }

    [Fact]
    public void AndHaving_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
AND HAVING a > 10" );
        }
    }

    [Fact]
    public void OrHaving_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
OR HAVING a > 10" );
        }
    }

    [Fact]
    public void OrHaving_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
OR HAVING a > 10" );
        }
    }

    [Fact]
    public void OrderBy_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
ORDER BY ([foo].[a] : ?) ASC" );
        }
    }

    [Fact]
    public void OrderBy_ForSingleDataSource_ShouldReturnDataSource_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.OrderBy( Enumerable.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "FROM [foo]" );
        }
    }

    [Fact]
    public void OrderBy_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
ORDER BY ([foo].[a] : ?) ASC, ([bar].[b] : ?) DESC" );
        }
    }

    [Fact]
    public void OrderBy_ForMultiDataSource_ShouldReturnDataSource_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.OrderBy( Enumerable.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE" );
        }
    }

    [Fact]
    public void With_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
WITH ORDINAL [A] (
  SELECT * FROM bar
)" );
        }
    }

    [Fact]
    public void With_ForSingleDataSource_ShouldReturnDataSource_WithEmptyCte()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.With( Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().BeSameAs( dataSource );
            text.Should().Be( "FROM [foo]" );
        }
    }

    [Fact]
    public void With_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
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
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionTrait );
            text.Should()
                .Be(
                    @"FROM [A]
INNER JOIN [B] AS [C] ON TRUE
WITH ORDINAL [A] (
  SELECT * FROM foo
),
ORDINAL [B] (
  SELECT * FROM bar
)" );
        }
    }

    [Fact]
    public void With_ForMultiDataSource_ShouldReturnDataSource_WithEmptyCte()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.With( Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().BeEmpty();
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE" );
        }
    }

    [Fact]
    public void Limit_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
LIMIT (""10"" : System.Int32)" );
        }
    }

    [Fact]
    public void Limit_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Limit( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
LIMIT (""10"" : System.Int32)" );
        }
    }

    [Fact]
    public void Offset_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
OFFSET (""10"" : System.Int32)" );
        }
    }

    [Fact]
    public void Offset_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Offset( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.Joins.Should().Be( dataSource.Joins );
            sut.RecordSets.Should().BeSameAs( dataSource.RecordSets );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON TRUE
OFFSET (""10"" : System.Int32)" );
        }
    }

    [Fact]
    public void Distinct_ForDataSourceQuery_ShouldReturnQueryWithDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var sut = query.Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.Should().NotBeSameAs( query );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
DISTINCT
SELECT" );
        }
    }

    [Fact]
    public void AndWhere_ForDataSourceQuery_ShouldReturnQueryWithDataSourceWithTrait()
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
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
AND WHERE a > 10
SELECT" );
        }
    }

    [Fact]
    public void OrWhere_ForDataSourceQuery_ShouldReturnQueryWithDataSourceWithTrait()
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
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.FilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
OR WHERE a > 10
SELECT" );
        }
    }

    [Fact]
    public void GroupBy_ForDataSourceQuery_ShouldReturnQueryWithDataSourceWithTrait()
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
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
GROUP BY ([foo].[a] : ?), ([foo].[b] : ?)
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
    public void AndHaving_ForDataSourceQuery_ShouldReturnQueryWithDataSourceWithTrait()
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
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
AND HAVING a > 10
SELECT" );
        }
    }

    [Fact]
    public void OrHaving_ForDataSourceQuery_ShouldReturnQueryWithDataSourceWithTrait()
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
            sut.Selection.Should().Be( query.Selection );
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.AggregationFilterTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
OR HAVING a > 10
SELECT" );
        }
    }

    [Fact]
    public void OrderBy_ForDataSourceQuery_ShouldReturnDataSourceQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
ORDER BY ([foo].[a] : ?) ASC, ([foo].[b] : ?) DESC
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
    public void OrderBy_ForCompoundQuery_ShouldReturnCompoundQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.SortTrait );
            text.Should()
                .Be(
                    @"(
  SELECT * FROM foo
)
UNION
(
  SELECT * FROM bar
)
ORDER BY (a) ASC, (b) DESC" );
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
    public void With_ForDataSourceQuery_ShouldReturnDataSourceQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
WITH ORDINAL [A] (
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
    public void With_ForCompoundQuery_ShouldReturnCompoundQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.CommonTableExpressionTrait );
            text.Should()
                .Be(
                    @"(
  SELECT * FROM foo
)
UNION
(
  SELECT * FROM bar
)
WITH ORDINAL [A] (
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
    public void Limit_ForDataSourceQuery_ShouldReturnDataSourceQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
LIMIT (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Limit_ForCompoundQuery_ShouldReturnCompoundQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.LimitTrait );
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
    public void Offset_ForDataSourceQuery_ShouldReturnDataSourceQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetTrait );
            text.Should()
                .Be(
                    @"FROM [foo]
OFFSET (""10"" : System.Int32)
SELECT" );
        }
    }

    [Fact]
    public void Offset_ForCompoundQuery_ShouldReturnCompoundQueryWithTrait()
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
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.OffsetTrait );
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
