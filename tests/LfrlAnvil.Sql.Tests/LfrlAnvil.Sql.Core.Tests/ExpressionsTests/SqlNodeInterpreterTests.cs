﻿using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
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
        var cte2 = SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM Y" ), "Z2" );
        var aggregation1 = SqlNode.RawExpression( "B" );
        var aggregation2 = SqlNode.RawExpression( "D" );
        var ordering1 = SqlNode.OrderByAsc( SqlNode.RawExpression( "A" ) );
        var ordering2 = SqlNode.OrderByDesc( SqlNode.RawExpression( "C" ) );
        var limit = SqlNode.Literal( 11 );
        var offset = SqlNode.Literal( 21 );
        var custom1 = new TraitMock();
        var custom2 = new TraitMock();

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
                SqlNode.SortTrait( ordering1 ),
                SqlNode.LimitTrait( SqlNode.Literal( 10 ) ),
                SqlNode.SortTrait( ordering2 ),
                SqlNode.CommonTableExpressionTrait( cte1 ),
                custom2,
                SqlNode.OffsetTrait( SqlNode.Literal( 20 ) ),
                SqlNode.LimitTrait( limit ),
                SqlNode.CommonTableExpressionTrait( cte2 ),
                SqlNode.OffsetTrait( offset )
            } );

        var result = SqlNodeInterpreter.ExtractDataSourceTraits( traits );

        using ( new AssertionScope() )
        {
            result.CommonTableExpressions.Should().HaveCount( 2 );
            result.CommonTableExpressions.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( cte1 );
            result.CommonTableExpressions.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( cte2 );
            result.Distinct.Should().NotBeNull();
            (result.Filter?.ToString()).Should().Be( "((A > 10) AND (C > 11)) OR (E > 12)" );
            result.Aggregations.Should().HaveCount( 2 );
            result.Aggregations.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( aggregation1 );
            result.Aggregations.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( aggregation2 );
            (result.AggregationFilter?.ToString()).Should().Be( "((B > 15) AND (D > 16)) OR (D < 27)" );
            result.Ordering.Should().HaveCount( 2 );
            result.Ordering.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( ordering1 );
            result.Ordering.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( ordering2 );
            result.Limit.Should().BeSameAs( limit );
            result.Offset.Should().BeSameAs( offset );
            result.Custom.Should().HaveCount( 2 );
            result.Custom.ElementAtOrDefault( 0 ).Should().BeSameAs( custom1 );
            result.Custom.ElementAtOrDefault( 1 ).Should().BeSameAs( custom2 );
        }
    }

    [Fact]
    public void ExtractQueryTraits_ShouldReturnCorrectResult()
    {
        var cte1 = SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM X" ), "Z1" );
        var cte2 = SqlNode.OrdinalCommonTableExpression( SqlNode.RawQuery( "SELECT * FROM Y" ), "Z2" );
        var ordering1 = SqlNode.OrderByAsc( SqlNode.RawExpression( "A" ) );
        var ordering2 = SqlNode.OrderByDesc( SqlNode.RawExpression( "C" ) );
        var limit = SqlNode.Literal( 11 );
        var offset = SqlNode.Literal( 21 );
        var distinct = SqlNode.DistinctTrait();
        var filter = SqlNode.FilterTrait( SqlNode.RawCondition( "A > 10" ), isConjunction: true );
        var aggregation = SqlNode.AggregationTrait( SqlNode.RawExpression( "B" ) );
        var aggregationFilter = SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "B > 15" ), isConjunction: true );
        var custom = new TraitMock();

        var traits = Chain.Create<SqlTraitNode>(
            new SqlTraitNode[]
            {
                distinct,
                filter,
                aggregation,
                aggregationFilter,
                SqlNode.SortTrait( ordering1 ),
                SqlNode.LimitTrait( SqlNode.Literal( 10 ) ),
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
            result.Ordering.Should().HaveCount( 2 );
            result.Ordering.ElementAtOrDefault( 0 ).ToArray().Should().BeSequentiallyEqualTo( ordering1 );
            result.Ordering.ElementAtOrDefault( 1 ).ToArray().Should().BeSequentiallyEqualTo( ordering2 );
            result.Limit.Should().BeSameAs( limit );
            result.Offset.Should().BeSameAs( offset );
            result.Custom.Should().HaveCount( 5 );
            result.Custom.ElementAtOrDefault( 0 ).Should().BeSameAs( distinct );
            result.Custom.ElementAtOrDefault( 1 ).Should().BeSameAs( filter );
            result.Custom.ElementAtOrDefault( 2 ).Should().BeSameAs( aggregation );
            result.Custom.ElementAtOrDefault( 3 ).Should().BeSameAs( aggregationFilter );
            result.Custom.ElementAtOrDefault( 4 ).Should().BeSameAs( custom );
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
        var sort = SqlNode.SortTrait( SqlNode.OrderByAsc( SqlNode.RawExpression( "A" ) ) );
        var custom = new TraitMock();

        var traits = Chain.Create<SqlTraitNode>(
            new SqlTraitNode[]
            {
                SqlNode.DistinctTrait(),
                SqlNode.FilterTrait( SqlNode.RawCondition( "A > 10" ), isConjunction: true ),
                aggregation,
                aggregationFilter,
                SqlNode.FilterTrait( SqlNode.RawCondition( "C > 11" ), isConjunction: true ),
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
            result.Custom.Should().HaveCount( 7 );
            result.Custom.ElementAtOrDefault( 0 ).Should().BeSameAs( aggregation );
            result.Custom.ElementAtOrDefault( 1 ).Should().BeSameAs( aggregationFilter );
            result.Custom.ElementAtOrDefault( 2 ).Should().BeSameAs( sort );
            result.Custom.ElementAtOrDefault( 3 ).Should().BeSameAs( limit );
            result.Custom.ElementAtOrDefault( 4 ).Should().BeSameAs( cte );
            result.Custom.ElementAtOrDefault( 5 ).Should().BeSameAs( custom );
            result.Custom.ElementAtOrDefault( 6 ).Should().BeSameAs( offset );
        }
    }

    private sealed class TraitMock : SqlTraitNode
    {
        public TraitMock()
            : base( SqlNodeType.Unknown ) { }
    }
}