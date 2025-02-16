using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class ExpressionTraitsTests : TestsBase
{
    [Fact]
    public void DistinctTrait_ShouldCreateDistinctDataSourceTraitNode()
    {
        var sut = SqlNode.DistinctTrait();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals( "DISTINCT" ) )
            .Go();
    }

    [Fact]
    public void FilterTrait_ShouldCreateFilterDataSourceTraitNode_AsConjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.FilterTrait( condition, isConjunction: true );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FilterTrait ),
                sut.Filter.TestRefEquals( condition ),
                sut.IsConjunction.TestTrue(),
                text.TestEquals( "AND WHERE bar > 10" ) )
            .Go();
    }

    [Fact]
    public void FilterTrait_ShouldCreateFilterDataSourceTraitNode_AsDisjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.FilterTrait( condition, isConjunction: false );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FilterTrait ),
                sut.Filter.TestRefEquals( condition ),
                sut.IsConjunction.TestFalse(),
                text.TestEquals( "OR WHERE bar > 10" ) )
            .Go();
    }

    [Fact]
    public void AggregationTrait_ShouldCreateAggregationDataSourceTraitNode()
    {
        var expressions = new SqlExpressionNode[] { SqlNode.RawExpression( "a" ), SqlNode.RawExpression( "b" ) };
        var sut = SqlNode.AggregationTrait( expressions );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregationTrait ),
                sut.Expressions.ToArray().TestSequence( expressions ),
                text.TestEquals( "GROUP BY (a), (b)" ) )
            .Go();
    }

    [Fact]
    public void AggregationTrait_ShouldCreateAggregationDataSourceTraitNode_WithEmptyExpressions()
    {
        var sut = SqlNode.AggregationTrait();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregationTrait ),
                sut.Expressions.ToArray().TestEmpty(),
                text.TestEquals( "GROUP BY" ) )
            .Go();
    }

    [Fact]
    public void AggregationFilterTrait_ShouldCreateAggregationFilterDataSourceTraitNode_AsConjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregationFilterTrait( condition, isConjunction: true );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ),
                sut.Filter.TestRefEquals( condition ),
                sut.IsConjunction.TestTrue(),
                text.TestEquals( "AND HAVING bar > 10" ) )
            .Go();
    }

    [Fact]
    public void AggregationFilterTrait_ShouldCreateAggregationFilterDataSourceTraitNode_AsDisjunction()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var sut = SqlNode.AggregationFilterTrait( condition, isConjunction: false );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ),
                sut.Filter.TestRefEquals( condition ),
                sut.IsConjunction.TestFalse(),
                text.TestEquals( "OR HAVING bar > 10" ) )
            .Go();
    }

    [Fact]
    public void SortTrait_ShouldCreateSortTraitNode()
    {
        var ordering = new[] { SqlNode.RawExpression( "a" ).Asc(), SqlNode.RawExpression( "b" ).Desc() };
        var sut = SqlNode.SortTrait( ordering );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SortTrait ),
                sut.Ordering.ToArray().TestSequence( ordering ),
                text.TestEquals( "ORDER BY (a) ASC, (b) DESC" ) )
            .Go();
    }

    [Fact]
    public void SortTrait_ShouldCreateSortTraitNode_WithEmptyOrdering()
    {
        var sut = SqlNode.SortTrait();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SortTrait ),
                sut.Ordering.ToArray().TestEmpty(),
                text.TestEquals( "ORDER BY" ) )
            .Go();
    }

    [Fact]
    public void LimitTrait_ShouldCreateLimitTraitNode()
    {
        var value = SqlNode.Literal( 10 );
        var sut = SqlNode.LimitTrait( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.LimitTrait ),
                sut.Value.TestRefEquals( value ),
                text.TestEquals( "LIMIT (\"10\" : System.Int32)" ) )
            .Go();
    }

    [Fact]
    public void OffsetTrait_ShouldCreateOffsetTraitNode()
    {
        var value = SqlNode.Literal( 10 );
        var sut = SqlNode.OffsetTrait( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.OffsetTrait ),
                sut.Value.TestRefEquals( value ),
                text.TestEquals( "OFFSET (\"10\" : System.Int32)" ) )
            .Go();
    }

    [Fact]
    public void CommonTableExpressionTrait_ShouldCreateCommonTableExpressionTraitNode()
    {
        var cte = new SqlCommonTableExpressionNode[]
        {
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM foo" ), "A" ),
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM bar" ), "B" )
        };

        var sut = SqlNode.CommonTableExpressionTrait( cte );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CommonTableExpressionTrait ),
                sut.CommonTableExpressions.ToArray().TestSequence( cte ),
                sut.ContainsRecursive.TestFalse(),
                text.TestEquals(
                    """
                    WITH ORDINAL [A] (
                      SELECT * FROM foo
                    ),
                    ORDINAL [B] (
                      SELECT * FROM bar
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void CommonTableExpressionTrait_ShouldCreateCommonTableExpressionTraitNode_WithRecursive()
    {
        var cte = new SqlCommonTableExpressionNode[]
        {
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM foo" ), "A" ),
            SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM bar" ), "B" )
                .ToRecursive( SqlNode.RawQuery( "SELECT * FROM B" ).ToUnion() )
        };

        var sut = SqlNode.CommonTableExpressionTrait( cte );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CommonTableExpressionTrait ),
                sut.CommonTableExpressions.ToArray().TestSequence( cte ),
                sut.ContainsRecursive.TestTrue(),
                text.TestEquals(
                    """
                    WITH ORDINAL [A] (
                      SELECT * FROM foo
                    ),
                    RECURSIVE [B] (
                      
                      SELECT * FROM bar
                    
                      UNION
                      
                      SELECT * FROM B

                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void CommonTableExpressionTrait_ShouldCreateCommonTableExpressionTraitNode_WithEmptyTables()
    {
        var sut = SqlNode.CommonTableExpressionTrait();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CommonTableExpressionTrait ),
                sut.CommonTableExpressions.ToArray().TestEmpty(),
                sut.ContainsRecursive.TestFalse(),
                text.TestEquals( "WITH" ) )
            .Go();
    }

    [Fact]
    public void WindowDefinitionTrait_ShouldCreateWindowDefinitionTraitNode()
    {
        var set = SqlNode.RawRecordSet( "qux" );
        var windows = new[]
        {
            SqlNode.WindowDefinition(
                "foo",
                new SqlExpressionNode[] { set["a"] },
                new[] { set["b"].Asc() },
                SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.CurrentRow ) ),
            SqlNode.WindowDefinition(
                "bar",
                new SqlExpressionNode[] { set["x"] },
                new[] { set["y"].Desc() },
                SqlNode.RangeWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.UnboundedFollowing ) )
        };

        var sut = SqlNode.WindowDefinitionTrait( windows );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowDefinitionTrait ),
                sut.Windows.ToArray().TestSequence( windows ),
                text.TestEquals(
                    """
                    WINDOW [foo] AS (PARTITION BY ([qux].[a] : ?) ORDER BY ([qux].[b] : ?) ASC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),
                      [bar] AS (PARTITION BY ([qux].[x] : ?) ORDER BY ([qux].[y] : ?) DESC RANGE BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING)
                    """ ) )
            .Go();
    }

    [Fact]
    public void WindowDefinitionTrait_ShouldCreateWindowDefinitionTraitNode_WithEmptyWindows()
    {
        var sut = SqlNode.WindowDefinitionTrait();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowDefinitionTrait ),
                sut.Windows.ToArray().TestEmpty(),
                text.TestEquals( "WINDOW" ) )
            .Go();
    }

    [Fact]
    public void WindowTrait_ShouldCreateWindowTraitNode()
    {
        var definition = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawRecordSet( "qux" )["a"].Asc() } );
        var sut = SqlNode.WindowTrait( definition );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowTrait ),
                sut.Definition.TestRefEquals( definition ),
                text.TestEquals( "OVER [foo] AS (ORDER BY ([qux].[a] : ?) ASC)" ) )
            .Go();
    }

    [Fact]
    public void OrderByAsc_ShouldCreateOrderByNode()
    {
        var expression = SqlNode.Parameter<int>( "a" );
        var sut = expression.Asc();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.OrderBy ),
                sut.Expression.TestRefEquals( expression ),
                sut.Ordering.TestRefEquals( OrderBy.Asc ),
                text.TestEquals( "(@a : System.Int32) ASC" ) )
            .Go();
    }

    [Fact]
    public void OrderByDesc_ShouldCreateOrderByNode()
    {
        var expression = SqlNode.Parameter<int>( "a" );
        var sut = expression.Desc();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.OrderBy ),
                sut.Expression.TestRefEquals( expression ),
                sut.Ordering.TestRefEquals( OrderBy.Desc ),
                text.TestEquals( "(@a : System.Int32) DESC" ) )
            .Go();
    }

    [Fact]
    public void OrderByAsc_ShouldCreateOrderByNode_FromSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From["bar"].As( "qux" );
        var sut = selection.Asc();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.OrderBy ),
                sut.Expression.TestType().AssignableTo<SqlSelectExpressionNode>( n => n.Selection.TestRefEquals( selection ) ),
                sut.Ordering.TestRefEquals( OrderBy.Asc ),
                text.TestEquals( "([qux]) ASC" ) )
            .Go();
    }

    [Fact]
    public void OrderByDesc_ShouldCreateOrderByNode_FromSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From["bar"].As( "qux" );
        var sut = selection.Desc();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.OrderBy ),
                sut.Expression.TestType().AssignableTo<SqlSelectExpressionNode>( n => n.Selection.TestRefEquals( selection ) ),
                sut.Ordering.TestRefEquals( OrderBy.Desc ),
                text.TestEquals( "([qux]) DESC" ) )
            .Go();
    }

    [Fact]
    public void WindowDefinition_ShouldCreateWindowDefinitionNode()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var partitioning = new SqlExpressionNode[] { set["a"], set["b"] };
        var ordering = new[] { set["x"].Asc(), set["y"].Desc() };
        var frame = SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.CurrentRow );
        var sut = SqlNode.WindowDefinition( "wnd", partitioning, ordering, frame );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowDefinition ),
                sut.Name.TestEquals( "wnd" ),
                sut.Partitioning.ToArray().TestSequence( partitioning ),
                sut.Ordering.ToArray().TestSequence( ordering ),
                sut.Frame.TestRefEquals( frame ),
                text.TestEquals(
                    "[wnd] AS (PARTITION BY ([foo].[a] : ?), ([foo].[b] : ?) ORDER BY ([foo].[x] : ?) ASC, ([foo].[y] : ?) DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)" ) )
            .Go();
    }

    [Fact]
    public void WindowDefinition_ShouldCreateWindowDefinitionNode_WithoutPartitioning()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var ordering = new[] { set["x"].Asc(), set["y"].Desc() };
        var frame = SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.CurrentRow );
        var sut = SqlNode.WindowDefinition( "wnd", ordering, frame );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowDefinition ),
                sut.Name.TestEquals( "wnd" ),
                sut.Partitioning.ToArray().TestEmpty(),
                sut.Ordering.ToArray().TestSequence( ordering ),
                sut.Frame.TestRefEquals( frame ),
                text.TestEquals(
                    "[wnd] AS (ORDER BY ([foo].[x] : ?) ASC, ([foo].[y] : ?) DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)" ) )
            .Go();
    }

    [Fact]
    public void WindowDefinition_ShouldCreateWindowDefinitionNode_WithoutFrame()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var partitioning = new SqlExpressionNode[] { set["a"], set["b"] };
        var ordering = new[] { set["x"].Asc(), set["y"].Desc() };
        var sut = SqlNode.WindowDefinition( "wnd", partitioning, ordering );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowDefinition ),
                sut.Name.TestEquals( "wnd" ),
                sut.Partitioning.ToArray().TestSequence( partitioning ),
                sut.Ordering.ToArray().TestSequence( ordering ),
                sut.Frame.TestNull(),
                text.TestEquals(
                    "[wnd] AS (PARTITION BY ([foo].[a] : ?), ([foo].[b] : ?) ORDER BY ([foo].[x] : ?) ASC, ([foo].[y] : ?) DESC)" ) )
            .Go();
    }

    [Fact]
    public void RowsWindowFrame_ShouldCreateRowsWindowFrameNode()
    {
        var sut = SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.UnboundedFollowing );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowFrame ),
                sut.FrameType.TestEquals( SqlWindowFrameType.Rows ),
                sut.Start.TestEquals( SqlWindowFrameBoundary.UnboundedPreceding ),
                sut.End.TestEquals( SqlWindowFrameBoundary.UnboundedFollowing ),
                text.TestEquals( "ROWS BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING" ) )
            .Go();
    }

    [Fact]
    public void RangeWindowFrame_ShouldCreateRangeWindowFrameNode()
    {
        var sut = SqlNode.RangeWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.UnboundedFollowing );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowFrame ),
                sut.FrameType.TestEquals( SqlWindowFrameType.Range ),
                sut.Start.TestEquals( SqlWindowFrameBoundary.CurrentRow ),
                sut.End.TestEquals( SqlWindowFrameBoundary.UnboundedFollowing ),
                text.TestEquals( "RANGE BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING" ) )
            .Go();
    }

    [Fact]
    public void WindowFrame_ShouldCreateWindowFrameNode_WithExpressionPrecedingAndFollowing()
    {
        var start = SqlWindowFrameBoundary.Preceding( SqlNode.Literal( 3 ) );
        var end = SqlWindowFrameBoundary.Following( SqlNode.Literal( 5 ) );
        var sut = SqlNode.RangeWindowFrame( start, end );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.WindowFrame ),
                sut.FrameType.TestEquals( SqlWindowFrameType.Range ),
                sut.Start.TestEquals( start ),
                sut.End.TestEquals( end ),
                text.TestEquals( "RANGE BETWEEN (\"3\" : System.Int32) PRECEDING AND (\"5\" : System.Int32) FOLLOWING" ) )
            .Go();
    }

    [Fact]
    public void Distinct_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.DistinctTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Distinct_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.DistinctTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Distinct_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var sut = function.Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( function ),
                sut.NodeType.TestEquals( function.NodeType ),
                sut.FunctionType.TestEquals( function.FunctionType ),
                sut.Arguments.TestSequence( function.Arguments ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.DistinctTrait ) ),
                text.TestEquals(
                    """
                    AGG_COUNT((*))
                      DISTINCT
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    AND WHERE a > 10
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    AND WHERE a > 10
                    """ ) )
            .Go();
    }

    [Fact]
    public void AndWhere_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var sut = function.AndWhere( filter );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( function ),
                sut.NodeType.TestEquals( function.NodeType ),
                sut.FunctionType.TestEquals( function.FunctionType ),
                sut.Arguments.TestSequence( function.Arguments ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    AGG_COUNT((*))
                      AND WHERE a > 10
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    OR WHERE a > 10
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    OR WHERE a > 10
                    """ ) )
            .Go();
    }

    [Fact]
    public void OrWhere_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var filter = SqlNode.RawCondition( "a > 10" );
        var sut = function.OrWhere( filter );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( function ),
                sut.NodeType.TestEquals( function.NodeType ),
                sut.FunctionType.TestEquals( function.FunctionType ),
                sut.Arguments.TestSequence( function.Arguments ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    AGG_COUNT((*))
                      OR WHERE a > 10
                    """ ) )
            .Go();
    }

    [Fact]
    public void OrderBy_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
    {
        var ordering = new[] { SqlNode.Literal( 10 ).Asc() }.AsEnumerable();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlOrderByNode>>>();
        selector.WithAnyArgs( _ => ordering );
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var sut = function.OrderBy( ordering );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( function ),
                sut.NodeType.TestEquals( function.NodeType ),
                sut.FunctionType.TestEquals( function.FunctionType ),
                sut.Arguments.TestSequence( function.Arguments ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.SortTrait ) ),
                text.TestEquals(
                    """
                    AGG_COUNT((*))
                      ORDER BY ("10" : System.Int32) ASC
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    GROUP BY ([foo].[a] : ?)
                    """ ) )
            .Go();
    }

    [Fact]
    public void GroupBy_ForSingleDataSource_ShouldReturnDataSource_WithEmptyExpressions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlExpressionNode>() );
        var sut = dataSource.GroupBy( selector );

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestRefEquals( dataSource ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    GROUP BY ([foo].[a] : ?)
                    """ ) )
            .Go();
    }

    [Fact]
    public void GroupBy_ForMultiDataSource_ShouldReturnDataSource_WithEmptyExpressions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var selector = Substitute.For<Func<SqlMultiDataSourceNode, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlExpressionNode>() );
        var sut = dataSource.GroupBy( selector );

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestRefEquals( dataSource ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    AND HAVING a > 10
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    AND HAVING a > 10
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    OR HAVING a > 10
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( dataSource ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    OR HAVING a > 10
                    """ ) )
            .Go();
    }

    [Fact]
    public void Window_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var windows = new[] { SqlNode.WindowDefinition( "x", new[] { dataSource.From["a"].Asc() } ) };
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlWindowDefinitionNode>>>();
        selector.WithAnyArgs( _ => windows );
        var sut = dataSource.Window( selector );
        var text = sut.ToString();

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.WindowDefinitionTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    WINDOW [x] AS (ORDER BY ([foo].[a] : ?) ASC)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Window_ForSingleDataSource_ShouldReturnDataSource_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Window( Enumerable.Empty<SqlWindowDefinitionNode>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "FROM [foo]" ) )
            .Go();
    }

    [Fact]
    public void Window_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var windows = new[]
        {
            SqlNode.WindowDefinition( "x", new[] { dataSource["foo"]["a"].Asc() } ),
            SqlNode.WindowDefinition( "y", new[] { dataSource["bar"]["b"].Desc() } )
        };

        var selector = Substitute.For<Func<SqlMultiDataSourceNode, IEnumerable<SqlWindowDefinitionNode>>>();
        selector.WithAnyArgs( _ => windows );
        var sut = dataSource.Window( selector );
        var text = sut.ToString();

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.WindowDefinitionTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    WINDOW [x] AS (ORDER BY ([foo].[a] : ?) ASC),
                      [y] AS (ORDER BY ([bar].[b] : ?) DESC)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Window_ForMultiDataSource_ShouldReturnDataSource_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.Window( Enumerable.Empty<SqlWindowDefinitionNode>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.TestEmpty(),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    """ ) )
            .Go();
    }

    [Fact]
    public void Over_ForAggregateFunction_ShouldReturnAggregateFunctionWithTrait()
    {
        var function = SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "*" ) );
        var sut = function.Over( SqlNode.WindowDefinition( "x", new[] { SqlNode.RawRecordSet( "foo" )["a"].Asc() } ) );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( function ),
                sut.NodeType.TestEquals( function.NodeType ),
                sut.FunctionType.TestEquals( function.FunctionType ),
                sut.Arguments.TestSequence( function.Arguments ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.WindowTrait ) ),
                text.TestEquals(
                    """
                    AGG_COUNT((*))
                      OVER [x] AS (ORDER BY ([foo].[a] : ?) ASC)
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.SortTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    ORDER BY ([foo].[a] : ?) ASC
                    """ ) )
            .Go();
    }

    [Fact]
    public void OrderBy_ForSingleDataSource_ShouldReturnDataSource_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.OrderBy( Enumerable.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "FROM [foo]" ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.SortTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    ORDER BY ([foo].[a] : ?) ASC, ([bar].[b] : ?) DESC
                    """ ) )
            .Go();
    }

    [Fact]
    public void OrderBy_ForMultiDataSource_ShouldReturnDataSource_WithEmptyOrdering()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.OrderBy( Enumerable.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.TestEmpty(),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.CommonTableExpressionTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    WITH ORDINAL [A] (
                      SELECT * FROM bar
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void With_ForSingleDataSource_ShouldReturnDataSource_WithEmptyCte()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.With( Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var text = sut.ToString();

        Assertion.All(
                sut.TestRefEquals( dataSource ),
                text.TestEquals( "FROM [foo]" ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.CommonTableExpressionTrait ) ),
                text.TestEquals(
                    """
                    FROM [A]
                    INNER JOIN [B] AS [C] ON TRUE
                    WITH ORDINAL [A] (
                      SELECT * FROM foo
                    ),
                    ORDINAL [B] (
                      SELECT * FROM bar
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void With_ForMultiDataSource_ShouldReturnDataSource_WithEmptyCte()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var sut = dataSource.With( Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.TestEmpty(),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    """ ) )
            .Go();
    }

    [Fact]
    public void Limit_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Limit( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.LimitTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    LIMIT ("10" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Limit_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Limit( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.LimitTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    LIMIT ("10" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Offset_ForSingleDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Offset( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.OffsetTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    OFFSET ("10" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Offset_ForMultiDataSource_ShouldReturnDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.True() ) );
        var value = SqlNode.Literal( 10 );
        var sut = dataSource.Offset( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.Joins.TestSequence( dataSource.Joins ),
                sut.RecordSets.TestRefEquals( dataSource.RecordSets ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.OffsetTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    OFFSET ("10" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Distinct_ForDataSourceQuery_ShouldReturnQueryWithDataSourceWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var sut = query.Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Selection.TestSequence( query.Selection ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.DistinctTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    DISTINCT
                    SELECT
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Selection.TestSequence( query.Selection ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    AND WHERE a > 10
                    SELECT
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Selection.TestSequence( query.Selection ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.FilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    OR WHERE a > 10
                    SELECT
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Selection.TestSequence( query.Selection ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    GROUP BY ([foo].[a] : ?), ([foo].[b] : ?)
                    SELECT
                    """ ) )
            .Go();
    }

    [Fact]
    public void GroupBy_ForDataSourceQuery_ShouldReturnQuery_WhenExpressionsAreEmpty()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlExpressionNode>() );
        var sut = query.GroupBy( selector );

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestRefEquals( query ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Selection.TestSequence( query.Selection ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    AND HAVING a > 10
                    SELECT
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Selection.TestSequence( query.Selection ),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.AggregationFilterTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    OR HAVING a > 10
                    SELECT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Window_ForDataSourceQuery_ShouldReturnDataSourceQueryWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var windows = new[]
        {
            SqlNode.WindowDefinition( "x", new[] { dataSource.From["a"].Asc() } ),
            SqlNode.WindowDefinition( "y", new[] { dataSource.From["b"].Desc() } )
        };

        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlWindowDefinitionNode>>>();
        selector.WithAnyArgs( _ => windows );
        var sut = query.Window( selector );
        var text = sut.ToString();

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.DataSource.TestRefEquals( query.DataSource ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.WindowDefinitionTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    WINDOW [x] AS (ORDER BY ([foo].[a] : ?) ASC),
                      [y] AS (ORDER BY ([foo].[b] : ?) DESC)
                    SELECT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Window_ForDataSourceQuery_ShouldReturnDataSourceQuery_WhenOrderingIsEmpty()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlWindowDefinitionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlWindowDefinitionNode>() );
        var sut = query.Window( selector );

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.TestRefEquals( query ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.DataSource.TestRefEquals( query.DataSource ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.SortTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    ORDER BY ([foo].[a] : ?) ASC, ([foo].[b] : ?) DESC
                    SELECT
                    """ ) )
            .Go();
    }

    [Fact]
    public void OrderBy_ForDataSourceQuery_ShouldReturnDataSourceQuery_WhenOrderingIsEmpty()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var selector = Substitute
            .For<Func<SqlDataSourceQueryExpressionNode<SqlSingleDataSourceNode<SqlRawRecordSetNode>>, IEnumerable<SqlOrderByNode>>>();

        selector.WithAnyArgs( _ => Enumerable.Empty<SqlOrderByNode>() );
        var sut = query.OrderBy( selector );

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestRefEquals( query ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.CompoundQuery ),
                sut.FirstQuery.TestRefEquals( query.FirstQuery ),
                sut.FollowingQueries.TestSequence( query.FollowingQueries ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.SortTrait ) ),
                text.TestEquals(
                    """
                    SELECT * FROM foo
                    UNION
                    SELECT * FROM bar
                    ORDER BY (a) ASC, (b) DESC
                    """ ) )
            .Go();
    }

    [Fact]
    public void OrderBy_ForCompoundQuery_ShouldReturnCompoundQuery_WhenOrderingIsEmpty()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var selector = Substitute.For<Func<SqlCompoundQueryExpressionNode, IEnumerable<SqlOrderByNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlOrderByNode>() );
        var sut = query.OrderBy( selector );

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestRefEquals( query ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.DataSource.TestRefEquals( query.DataSource ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.CommonTableExpressionTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    WITH ORDINAL [A] (
                      SELECT * FROM bar
                    )
                    SELECT
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestRefEquals( query ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.CompoundQuery ),
                sut.FirstQuery.TestRefEquals( query.FirstQuery ),
                sut.FollowingQueries.TestSequence( query.FollowingQueries ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.CommonTableExpressionTrait ) ),
                text.TestEquals(
                    """
                    SELECT * FROM foo
                    UNION
                    SELECT * FROM bar
                    WITH ORDINAL [A] (
                      SELECT * FROM qux
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void With_ForCompoundQuery_ShouldReturnCompoundQuery_WhenCteAreEmpty()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var selector = Substitute.For<Func<SqlCompoundQueryExpressionNode, IEnumerable<SqlCommonTableExpressionNode>>>();
        selector.WithAnyArgs( _ => Enumerable.Empty<SqlCommonTableExpressionNode>() );
        var sut = query.With( selector );

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ query ] ),
                sut.TestRefEquals( query ) )
            .Go();
    }

    [Fact]
    public void Limit_ForDataSourceQuery_ShouldReturnDataSourceQueryWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var value = SqlNode.Literal( 10 );
        var sut = query.Limit( value );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.DataSource.TestRefEquals( query.DataSource ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.LimitTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    LIMIT ("10" : System.Int32)
                    SELECT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Limit_ForCompoundQuery_ShouldReturnCompoundQueryWithTrait()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var value = SqlNode.Literal( 10 );
        var sut = query.Limit( value );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.CompoundQuery ),
                sut.FirstQuery.TestRefEquals( query.FirstQuery ),
                sut.FollowingQueries.TestSequence( query.FollowingQueries ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.LimitTrait ) ),
                text.TestEquals(
                    """
                    SELECT * FROM foo
                    UNION
                    SELECT * FROM bar
                    LIMIT ("10" : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Offset_ForDataSourceQuery_ShouldReturnDataSourceQueryWithTrait()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select();
        var value = SqlNode.Literal( 10 );
        var sut = query.Offset( value );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.DataSource.TestRefEquals( query.DataSource ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.OffsetTrait ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    OFFSET ("10" : System.Int32)
                    SELECT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Offset_ForCompoundQuery_ShouldReturnCompoundQueryWithTrait()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" ).CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnion() );
        var value = SqlNode.Literal( 10 );
        var sut = query.Offset( value );
        var text = sut.ToString();

        Assertion.All(
                sut.TestNotRefEquals( query ),
                sut.NodeType.TestEquals( SqlNodeType.CompoundQuery ),
                sut.FirstQuery.TestRefEquals( query.FirstQuery ),
                sut.FollowingQueries.TestSequence( query.FollowingQueries ),
                sut.Selection.TestSequence( query.Selection ),
                sut.Traits.Count.TestEquals( 1 ),
                sut.Traits.TestAll( (t, _) => t.NodeType.TestEquals( SqlNodeType.OffsetTrait ) ),
                text.TestEquals(
                    """
                    SELECT * FROM foo
                    UNION
                    SELECT * FROM bar
                    OFFSET ("10" : System.Int32)
                    """ ) )
            .Go();
    }
}
