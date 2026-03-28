using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class SqlNodeInterpreterTests : TestsBase
{
    [Fact]
    public void DebugInterpreter_ShouldUseSquareBracketsAsNameDelimiters()
    {
        var sut = new SqlNodeDebugInterpreter();

        Assertion.All(
                sut.BeginNameDelimiter.TestEquals( '[' ),
                sut.EndNameDelimiter.TestEquals( ']' ),
                sut.RecordSetNodeBehavior.TestNull() )
            .Go();
    }

    [Fact]
    public void Interpret_ShouldCallVisitAndReturnContext()
    {
        var sut = new SqlNodeDebugInterpreter();
        var result = sut.Interpret( SqlNode.Null() );

        Assertion.All(
                result.TestRefEquals( sut.Context ),
                result.Sql.ToString().TestEquals( "NULL" ) )
            .Go();
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
        var sortPlaceholder = SqlNode.Placeholders.SortTrait();
        var custom1 = new SqlTraitNodeMock();
        var custom2 = new SqlTraitNodeMock();

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
                SqlNode.Placeholders.SortTrait(),
                SqlNode.CommonTableExpressionTrait( cte1 ),
                custom2,
                SqlNode.OffsetTrait( SqlNode.Literal( 20 ) ),
                sortPlaceholder,
                SqlNode.LimitTrait( limit ),
                over,
                SqlNode.CommonTableExpressionTrait( cte2 ),
                SqlNode.OffsetTrait( offset )
            } );

        var result = SqlNodeInterpreter.ExtractDataSourceTraits( traits );

        Assertion.All(
                result.CommonTableExpressions.Count.TestEquals( 2 ),
                result.CommonTableExpressions.ElementAtOrDefault( 0 ).TestSequence( [ cte1 ] ),
                result.CommonTableExpressions.ElementAtOrDefault( 1 ).TestSequence( [ cte2 ] ),
                result.ContainsRecursiveCommonTableExpression.TestTrue(),
                result.Distinct.TestNotNull(),
                (result.Filter?.ToString()).TestEquals( "((A > 10) AND (C > 11)) OR (E > 12)" ),
                result.Aggregations.Count.TestEquals( 2 ),
                result.Aggregations.ElementAtOrDefault( 0 ).TestSequence( [ aggregation1 ] ),
                result.Aggregations.ElementAtOrDefault( 1 ).TestSequence( [ aggregation2 ] ),
                (result.AggregationFilter?.ToString()).TestEquals( "((B > 15) AND (D > 16)) OR (D < 27)" ),
                result.Windows.Count.TestEquals( 2 ),
                result.Windows.ElementAtOrDefault( 0 ).TestSequence( [ windows1 ] ),
                result.Windows.ElementAtOrDefault( 1 ).TestSequence( [ windows2 ] ),
                result.Ordering.Nodes.Count.TestEquals( 2 ),
                result.Ordering.Nodes.ElementAtOrDefault( 0 ).TestSequence( [ ordering1 ] ),
                result.Ordering.Nodes.ElementAtOrDefault( 1 ).TestSequence( [ ordering2 ] ),
                result.Ordering.Placeholder.TestRefEquals( sortPlaceholder ),
                result.Limit.TestRefEquals( limit ),
                result.Offset.TestRefEquals( offset ),
                result.Custom.Count.TestEquals( 3 ),
                result.Custom.ElementAtOrDefault( 0 ).TestRefEquals( custom1 ),
                result.Custom.ElementAtOrDefault( 1 ).TestRefEquals( custom2 ),
                result.Custom.ElementAtOrDefault( 2 ).TestRefEquals( over ) )
            .Go();
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
        var over = SqlNode.WindowTrait( windows.Windows[0] );
        var sortPlaceholder = SqlNode.Placeholders.SortTrait();
        var custom = new SqlTraitNodeMock();

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
                SqlNode.Placeholders.SortTrait(),
                SqlNode.OffsetTrait( SqlNode.Literal( 20 ) ),
                SqlNode.LimitTrait( limit ),
                sortPlaceholder,
                SqlNode.CommonTableExpressionTrait( cte2 ),
                SqlNode.OffsetTrait( offset )
            } );

        var result = SqlNodeInterpreter.ExtractQueryTraits( traits );

        Assertion.All(
                result.CommonTableExpressions.Count.TestEquals( 2 ),
                result.CommonTableExpressions.ElementAtOrDefault( 0 ).ToArray().TestSequence( [ cte1 ] ),
                result.CommonTableExpressions.ElementAtOrDefault( 1 ).ToArray().TestSequence( [ cte2 ] ),
                result.ContainsRecursiveCommonTableExpression.TestFalse(),
                result.Ordering.Nodes.Count.TestEquals( 2 ),
                result.Ordering.Nodes.ElementAtOrDefault( 0 ).ToArray().TestSequence( [ ordering1 ] ),
                result.Ordering.Nodes.ElementAtOrDefault( 1 ).ToArray().TestSequence( [ ordering2 ] ),
                result.Ordering.Placeholder.TestRefEquals( sortPlaceholder ),
                result.Limit.TestRefEquals( limit ),
                result.Offset.TestRefEquals( offset ),
                result.Custom.Count.TestEquals( 7 ),
                result.Custom.ElementAtOrDefault( 0 ).TestRefEquals( distinct ),
                result.Custom.ElementAtOrDefault( 1 ).TestRefEquals( filter ),
                result.Custom.ElementAtOrDefault( 2 ).TestRefEquals( aggregation ),
                result.Custom.ElementAtOrDefault( 3 ).TestRefEquals( aggregationFilter ),
                result.Custom.ElementAtOrDefault( 4 ).TestRefEquals( windows ),
                result.Custom.ElementAtOrDefault( 5 ).TestRefEquals( over ),
                result.Custom.ElementAtOrDefault( 6 ).TestRefEquals( custom ) )
            .Go();
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
        var over = SqlNode.WindowTrait( windows.Windows[0] );
        var ordering1 = SqlNode.OrderByAsc( SqlNode.RawExpression( "A" ) );
        var ordering2 = SqlNode.OrderByDesc( SqlNode.RawExpression( "C" ) );
        var sortPlaceholder = SqlNode.Placeholders.SortTrait();
        var custom = new SqlTraitNodeMock();

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
                SqlNode.Placeholders.SortTrait(),
                SqlNode.SortTrait( ordering1 ),
                limit,
                cte,
                SqlNode.SortTrait( ordering2 ),
                sortPlaceholder,
                SqlNode.FilterTrait( SqlNode.RawCondition( "E > 12" ), isConjunction: false ),
                custom,
                offset
            } );

        var result = SqlNodeInterpreter.ExtractAggregateFunctionTraits( traits );

        Assertion.All(
                result.Distinct.TestNotNull(),
                (result.Filter?.ToString()).TestEquals( "((A > 10) AND (C > 11)) OR (E > 12)" ),
                (result.Window?.ToString()).TestEquals( "[W] AS (ORDER BY (C) ASC)" ),
                result.Ordering.Nodes.Count.TestEquals( 2 ),
                result.Ordering.Nodes.ElementAtOrDefault( 0 ).ToArray().TestSequence( [ ordering1 ] ),
                result.Ordering.Nodes.ElementAtOrDefault( 1 ).ToArray().TestSequence( [ ordering2 ] ),
                result.Ordering.Placeholder.TestRefEquals( sortPlaceholder ),
                result.Custom.Count.TestEquals( 7 ),
                result.Custom.ElementAtOrDefault( 0 ).TestRefEquals( aggregation ),
                result.Custom.ElementAtOrDefault( 1 ).TestRefEquals( aggregationFilter ),
                result.Custom.ElementAtOrDefault( 2 ).TestRefEquals( windows ),
                result.Custom.ElementAtOrDefault( 3 ).TestRefEquals( limit ),
                result.Custom.ElementAtOrDefault( 4 ).TestRefEquals( cte ),
                result.Custom.ElementAtOrDefault( 5 ).TestRefEquals( custom ),
                result.Custom.ElementAtOrDefault( 6 ).TestRefEquals( offset ) )
            .Go();
    }

    [Fact]
    public void FilterTraits_ShouldReturnedFilteredCollectionOfTraits()
    {
        var limit = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
        var offset = SqlNode.OffsetTrait( SqlNode.Literal( 20 ) );
        var traits = Chain.Create<SqlTraitNode>( limit ).Extend( offset );

        var result = SqlNodeInterpreter.FilterTraits( traits, t => t.NodeType == SqlNodeType.LimitTrait );

        result.TestSequence( [ limit ] ).Go();
    }

    [Fact]
    public void TempIgnoreRecordSet_ShouldSetRecordSetNameBehaviorToIgnoreAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var sut = new SqlNodeDebugInterpreter();

        SqlNodeInterpreter.RecordSetNodeBehaviorRule? rule;
        using ( sut.TempIgnoreRecordSet( set ) )
            rule = sut.RecordSetNodeBehavior;

        Assertion.All(
                sut.RecordSetNodeBehavior.TestNull(),
                rule.TestNotNull(),
                (rule?.Node).TestRefEquals( set ),
                (rule?.ReplacementNode).TestNull() )
            .Go();
    }

    [Fact]
    public void TempReplaceRecordSet_ShouldSetRecordSetNameBehaviorToReplaceAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var set = SqlNode.RawRecordSet( "foo" );
        var replacement = SqlNode.RawRecordSet( "bar" );
        var sut = new SqlNodeDebugInterpreter();

        SqlNodeInterpreter.RecordSetNodeBehaviorRule? rule;
        using ( sut.TempReplaceRecordSet( set, replacement ) )
            rule = sut.RecordSetNodeBehavior;

        Assertion.All(
                sut.RecordSetNodeBehavior.TestNull(),
                rule.TestNotNull(),
                (rule?.Node).TestRefEquals( set ),
                (rule?.ReplacementNode).TestRefEquals( replacement ) )
            .Go();
    }

    [Fact]
    public void TempIgnoreAllRecordSets_ShouldSetRecordSetNameBehaviorToIgnoreAllAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var sut = new SqlNodeDebugInterpreter();

        SqlNodeInterpreter.RecordSetNodeBehaviorRule? rule;
        using ( sut.TempIgnoreAllRecordSets() )
            rule = sut.RecordSetNodeBehavior;

        Assertion.All(
                sut.RecordSetNodeBehavior.TestNull(),
                rule.TestNotNull(),
                (rule?.Node).TestNull(),
                (rule?.ReplacementNode).TestNull() )
            .Go();
    }

    [Fact]
    public void TempIncludeAllRecordSets_ShouldSetRecordSetNameBehaviorToIncludeAllAndReturnObjectThatResetsTheRuleToPreviousWhenDisposed()
    {
        var sut = new SqlNodeDebugInterpreter();
        _ = sut.TempIgnoreAllRecordSets();
        var previous = sut.RecordSetNodeBehavior;

        SqlNodeInterpreter.RecordSetNodeBehaviorRule? rule;
        using ( sut.TempIncludeAllRecordSets() )
            rule = sut.RecordSetNodeBehavior;

        Assertion.All(
                sut.RecordSetNodeBehavior.TestEquals( previous ),
                rule.TestNull(),
                (rule?.ReplacementNode).TestNull() )
            .Go();
    }
}
