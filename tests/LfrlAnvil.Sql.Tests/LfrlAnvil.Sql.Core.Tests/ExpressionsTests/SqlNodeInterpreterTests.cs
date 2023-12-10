using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlNodeInterpreterTests : TestsBase
{
    [Fact]
    public void DebugInterpreter_ShouldUseSquareBracketsAsNameDelimiters()
    {
        var sut = new SqlNodeDebugInterpreter();

        using ( new AssertionScope() )
        {
            sut.BeginNameDelimiter.Should().Be( '[' );
            sut.EndNameDelimiter.Should().Be( ']' );
            sut.RecordSetNameBehavior.Should().BeNull();
        }
    }

    [Fact]
    public void Interpret_ShouldCallVisitAndReturnContext()
    {
        var sut = new SqlNodeDebugInterpreter();
        var result = sut.Interpret( SqlNode.Null() );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut.Context );
            result.Sql.ToString().Should().Be( "NULL" );
        }
    }

    [Fact]
    public void ExtractDataSourceTraits_ShouldReturnCorrectResult()
    {
        var cte1 = SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM X" ), "Z1" );
        var cte2 = SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM Y" ), "Z2" )
            .ToRecursive( SqlNode.RawQuery( "SELECT * FROM Z2" ).ToUnion() );

        var aggregation1 = SqlNode.RawExpression( "B" );
        var aggregation2 = SqlNode.RawExpression( "D" );
        var windows1 = SqlNode.WindowDefinition( "W1", new[] { SqlNode.RawExpression( "C1" ).Asc() } );
        var ordering1 = SqlNode.OrderByAsc( SqlNode.RawExpression( "A" ) );
        var ordering2 = SqlNode.OrderByDesc( SqlNode.RawExpression( "C" ) );
        var windows2 = SqlNode.WindowDefinition( "W2", new[] { SqlNode.RawExpression( "C2" ).Asc() } );
        var limit = SqlNode.Literal( 11 );
        var offset = SqlNode.Literal( 21 );
        var over = SqlNode.WindowTrait( windows1 );
        var custom1 = new TraitNodeMock();
        var custom2 = new TraitNodeMock();

        var traits = Chain.Create<SqlTraitNode>(
            new SqlTraitNode[]
            {
                custom1,
                SqlNode.DistinctTrait(),
                SqlNode.FilterTrait( SqlNode.RawCondition( "A > 10" ), isConjunction: true ),
                SqlNode.AggregationTrait( aggregation1 ),
                SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "B > 15" ), isConjunction: true ),
                SqlNode.FilterTrait( SqlNode.RawCondition( "C > 11" ), isConjunction: true ),
                SqlNode.AggregationTrait( aggregation2 ),
                SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "D > 16" ), isConjunction: true ),
                SqlNode.FilterTrait( SqlNode.RawCondition( "E > 12" ), isConjunction: false ),
                SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "D < 27" ), isConjunction: false ),
                SqlNode.WindowDefinitionTrait( windows1 ),
                SqlNode.SortTrait( ordering1 ),
                SqlNode.LimitTrait( SqlNode.Literal( 10 ) ),
                SqlNode.SortTrait( ordering2 ),
                SqlNode.WindowDefinitionTrait( windows2 ),
                SqlNode.CommonTableExpressionTrait( cte1 ),
                custom2,
                SqlNode.OffsetTrait( SqlNode.Literal( 20 ) ),
                SqlNode.LimitTrait( limit ),
                over,
                SqlNode.CommonTableExpressionTrait( cte2 ),
                SqlNode.OffsetTrait( offset )
            } );

        var result = SqlNodeInterpreter.ExtractDataSourceTraits( traits );

        using ( new AssertionScope() )
        {
            result.CommonTableExpressions.Should().HaveCount( 2 );
            result.CommonTableExpressions.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( cte1 );
            result.CommonTableExpressions.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( cte2 );
            result.ContainsRecursiveCommonTableExpression.Should().BeTrue();
            result.Distinct.Should().NotBeNull();
            (result.Filter?.ToString()).Should().Be( "((A > 10) AND (C > 11)) OR (E > 12)" );
            result.Aggregations.Should().HaveCount( 2 );
            result.Aggregations.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( aggregation1 );
            result.Aggregations.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( aggregation2 );
            (result.AggregationFilter?.ToString()).Should().Be( "((B > 15) AND (D > 16)) OR (D < 27)" );
            result.Windows.Should().HaveCount( 2 );
            result.Windows.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( windows1 );
            result.Windows.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( windows2 );
            result.Ordering.Should().HaveCount( 2 );
            result.Ordering.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( ordering1 );
            result.Ordering.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( ordering2 );
            result.Limit.Should().BeSameAs( limit );
            result.Offset.Should().BeSameAs( offset );
            result.Custom.Should().HaveCount( 3 );
            result.Custom.ElementAtOrDefault( 0 ).Should().BeSameAs( custom1 );
            result.Custom.ElementAtOrDefault( 1 ).Should().BeSameAs( custom2 );
            result.Custom.ElementAtOrDefault( 2 ).Should().BeSameAs( over );
        }
    }

    [Fact]
    public void ExtractQueryTraits_ShouldReturnCorrectResult()
    {
        var cte1 = SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM X" ), "Z1" );
        var cte2 = SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM Y" ), "Z2" );
        var windows = SqlNode.WindowDefinitionTrait( SqlNode.WindowDefinition( "W1", new[] { SqlNode.RawExpression( "C1" ).Asc() } ) );
        var ordering1 = SqlNode.OrderByAsc( SqlNode.RawExpression( "A" ) );
        var ordering2 = SqlNode.OrderByDesc( SqlNode.RawExpression( "C" ) );
        var limit = SqlNode.Literal( 11 );
        var offset = SqlNode.Literal( 21 );
        var distinct = SqlNode.DistinctTrait();
        var filter = SqlNode.FilterTrait( SqlNode.RawCondition( "A > 10" ), isConjunction: true );
        var aggregation = SqlNode.AggregationTrait( SqlNode.RawExpression( "B" ) );
        var aggregationFilter = SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "B > 15" ), isConjunction: true );
        var over = SqlNode.WindowTrait( windows.Windows.Span[0] );
        var custom = new TraitNodeMock();

        var traits = Chain.Create<SqlTraitNode>(
            new SqlTraitNode[]
            {
                distinct,
                filter,
                aggregation,
                aggregationFilter,
                SqlNode.SortTrait( ordering1 ),
                SqlNode.LimitTrait( SqlNode.Literal( 10 ) ),
                windows,
                over,
                SqlNode.SortTrait( ordering2 ),
                SqlNode.CommonTableExpressionTrait( cte1 ),
                custom,
                SqlNode.OffsetTrait( SqlNode.Literal( 20 ) ),
                SqlNode.LimitTrait( limit ),
                SqlNode.CommonTableExpressionTrait( cte2 ),
                SqlNode.OffsetTrait( offset )
            } );

        var result = SqlNodeInterpreter.ExtractQueryTraits( traits );

        using ( new AssertionScope() )
        {
            result.CommonTableExpressions.Should().HaveCount( 2 );
            result.CommonTableExpressions.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( cte1 );
            result.CommonTableExpressions.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( cte2 );
            result.ContainsRecursiveCommonTableExpression.Should().BeFalse();
            result.Ordering.Should().HaveCount( 2 );
            result.Ordering.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( ordering1 );
            result.Ordering.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( ordering2 );
            result.Limit.Should().BeSameAs( limit );
            result.Offset.Should().BeSameAs( offset );
            result.Custom.Should().HaveCount( 7 );
            result.Custom.ElementAtOrDefault( 0 ).Should().BeSameAs( distinct );
            result.Custom.ElementAtOrDefault( 1 ).Should().BeSameAs( filter );
            result.Custom.ElementAtOrDefault( 2 ).Should().BeSameAs( aggregation );
            result.Custom.ElementAtOrDefault( 3 ).Should().BeSameAs( aggregationFilter );
            result.Custom.ElementAtOrDefault( 4 ).Should().BeSameAs( windows );
            result.Custom.ElementAtOrDefault( 5 ).Should().BeSameAs( over );
            result.Custom.ElementAtOrDefault( 6 ).Should().BeSameAs( custom );
        }
    }

    [Fact]
    public void ExtractAggregateFunctionTraits_ShouldReturnCorrectResult()
    {
        var cte = SqlNode.CommonTableExpressionTrait( SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM X" ), "Z" ) );
        var limit = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
        var offset = SqlNode.OffsetTrait( SqlNode.Literal( 20 ) );
        var aggregation = SqlNode.AggregationTrait( SqlNode.RawExpression( "B" ) );
        var aggregationFilter = SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "B > 15" ), isConjunction: true );
        var windows = SqlNode.WindowDefinitionTrait( SqlNode.WindowDefinition( "W", new[] { SqlNode.RawExpression( "C" ).Asc() } ) );
        var over = SqlNode.WindowTrait( windows.Windows.Span[0] );
        var sort = SqlNode.SortTrait( SqlNode.OrderByAsc( SqlNode.RawExpression( "A" ) ) );
        var custom = new TraitNodeMock();

        var traits = Chain.Create<SqlTraitNode>(
            new SqlTraitNode[]
            {
                SqlNode.DistinctTrait(),
                SqlNode.FilterTrait( SqlNode.RawCondition( "A > 10" ), isConjunction: true ),
                aggregation,
                aggregationFilter,
                SqlNode.FilterTrait( SqlNode.RawCondition( "C > 11" ), isConjunction: true ),
                over,
                windows,
                sort,
                limit,
                cte,
                SqlNode.FilterTrait( SqlNode.RawCondition( "E > 12" ), isConjunction: false ),
                custom,
                offset
            } );

        var result = SqlNodeInterpreter.ExtractAggregateFunctionTraits( traits );

        using ( new AssertionScope() )
        {
            result.Distinct.Should().NotBeNull();
            (result.Filter?.ToString()).Should().Be( "((A > 10) AND (C > 11)) OR (E > 12)" );
            (result.Window?.ToString()).Should().Be( "[W] AS (ORDER BY (C) ASC)" );
            result.Ordering.Should().BeSequentiallyEqualTo( sort.Ordering );
            result.Custom.Should().HaveCount( 7 );
            result.Custom.ElementAtOrDefault( 0 ).Should().BeSameAs( aggregation );
            result.Custom.ElementAtOrDefault( 1 ).Should().BeSameAs( aggregationFilter );
            result.Custom.ElementAtOrDefault( 2 ).Should().BeSameAs( windows );
            result.Custom.ElementAtOrDefault( 3 ).Should().BeSameAs( limit );
            result.Custom.ElementAtOrDefault( 4 ).Should().BeSameAs( cte );
            result.Custom.ElementAtOrDefault( 5 ).Should().BeSameAs( custom );
            result.Custom.ElementAtOrDefault( 6 ).Should().BeSameAs( offset );
        }
    }

    [Fact]
    public void TempIgnoreRecordSet_ShouldSetRecordSetNameBehaviorToIgnoreAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var sut = new SqlNodeDebugInterpreter();

        SqlNodeInterpreter.RecordSetNameBehaviorRule? rule;
        using ( sut.TempIgnoreRecordSet( set ) )
            rule = sut.RecordSetNameBehavior;

        using ( new AssertionScope() )
        {
            sut.RecordSetNameBehavior.Should().BeNull();
            rule.Should().NotBeNull();
            (rule?.Node).Should().BeSameAs( set );
            (rule?.ReplacementNode).Should().BeNull();
        }
    }

    [Fact]
    public void TempReplaceRecordSet_ShouldSetRecordSetNameBehaviorToReplaceAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var replacement = SqlNode.RawRecordSet( "bar" );
        var sut = new SqlNodeDebugInterpreter();

        SqlNodeInterpreter.RecordSetNameBehaviorRule? rule;
        using ( sut.TempReplaceRecordSet( set, replacement ) )
            rule = sut.RecordSetNameBehavior;

        using ( new AssertionScope() )
        {
            sut.RecordSetNameBehavior.Should().BeNull();
            rule.Should().NotBeNull();
            (rule?.Node).Should().BeSameAs( set );
            (rule?.ReplacementNode).Should().BeSameAs( replacement );
        }
    }

    [Fact]
    public void TempIgnoreAllRecordSets_ShouldSetRecordSetNameBehaviorToIgnoreAllAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var sut = new SqlNodeDebugInterpreter();

        SqlNodeInterpreter.RecordSetNameBehaviorRule? rule;
        using ( sut.TempIgnoreAllRecordSets() )
            rule = sut.RecordSetNameBehavior;

        using ( new AssertionScope() )
        {
            sut.RecordSetNameBehavior.Should().BeNull();
            rule.Should().NotBeNull();
            (rule?.Node).Should().BeNull();
            (rule?.ReplacementNode).Should().BeNull();
        }
    }

    [Fact]
    public void TempIncludeAllRecordSets_ShouldSetRecordSetNameBehaviorToIncludeAllAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var sut = new SqlNodeDebugInterpreter();
        _ = sut.TempIgnoreAllRecordSets();
        var previous = sut.RecordSetNameBehavior;

        SqlNodeInterpreter.RecordSetNameBehaviorRule? rule;
        using ( sut.TempIncludeAllRecordSets() )
            rule = sut.RecordSetNameBehavior;

        using ( new AssertionScope() )
        {
            sut.RecordSetNameBehavior.Should().BeEquivalentTo( previous );
            rule.Should().BeNull();
            (rule?.ReplacementNode).Should().BeNull();
        }
    }
}
