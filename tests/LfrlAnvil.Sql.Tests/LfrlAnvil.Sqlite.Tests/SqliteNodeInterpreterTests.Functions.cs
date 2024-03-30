using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sqlite.Tests;

public partial class SqliteNodeInterpreterTests
{
    public class Functions : TestsBase
    {
        [Fact]
        public void Visit_ShouldInterpretNamedFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.Functions.Named(
                    SqlSchemaObjectName.Create( "foo", "bar" ),
                    SqlNode.Parameter<int>( "a" ),
                    SqlNode.RawExpression( "qux.a" ) ) );

            sut.Context.Sql.ToString().Should().Be( "\"foo_bar\"(@a, (qux.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretCoalesceFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Null().Coalesce( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "COALESCE(NULL, @a, (foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretCoalesceFunctionWithOneParameterThatRequiresParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Coalesce() );
            sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCoalesceFunctionWithOneParameterThatDoesNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Coalesce() );
            sut.Context.Sql.ToString().Should().Be( "25" );
        }

        [Fact]
        public void VisitChild_ShouldInterpretCoalesceFunctionWithOneParameterThatRequiresParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.RawExpression( "foo.a" ).Coalesce() );
            sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
        }

        [Fact]
        public void VisitChild_ShouldInterpretCoalesceFunctionWithOneParameterThatDoesNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 25 ).Coalesce() );
            sut.Context.Sql.ToString().Should().Be( "25" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentDateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentDate() );
            sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_DATE()" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentTimeFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentTime() );
            sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_TIME()" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentDateTimeFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentDateTime() );
            sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_DATETIME()" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentUtcDateTimeFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentUtcDateTime() );
            sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_UTC_DATETIME()" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentTimestampFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentTimestamp() );
            sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_TIMESTAMP()" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDate() );
            sut.Context.Sql.ToString().Should().Be( "DATE(10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractTimeOfDayFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractTimeOfDay() );
            sut.Context.Sql.ToString().Should().Be( "TIME_OF_DAY(10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDayOfYearFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDayOfYear() );
            sut.Context.Sql.ToString().Should().Be( "EXTRACT_TEMPORAL(10, 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDayOfMonthFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDayOfMonth() );
            sut.Context.Sql.ToString().Should().Be( "EXTRACT_TEMPORAL(9, 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDayOfWeekFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDayOfWeek() );
            sut.Context.Sql.ToString().Should().Be( "EXTRACT_TEMPORAL(11, 10)" );
        }

        [Theory]
        [InlineData( SqlTemporalUnit.Nanosecond, "EXTRACT_TEMPORAL(8, 10)" )]
        [InlineData( SqlTemporalUnit.Microsecond, "EXTRACT_TEMPORAL(7, 10)" )]
        [InlineData( SqlTemporalUnit.Millisecond, "EXTRACT_TEMPORAL(6, 10)" )]
        [InlineData( SqlTemporalUnit.Second, "EXTRACT_TEMPORAL(5, 10)" )]
        [InlineData( SqlTemporalUnit.Minute, "EXTRACT_TEMPORAL(4, 10)" )]
        [InlineData( SqlTemporalUnit.Hour, "EXTRACT_TEMPORAL(3, 10)" )]
        [InlineData( SqlTemporalUnit.Day, "EXTRACT_TEMPORAL(9, 10)" )]
        [InlineData( SqlTemporalUnit.Week, "EXTRACT_TEMPORAL(2, 10)" )]
        [InlineData( SqlTemporalUnit.Month, "EXTRACT_TEMPORAL(1, 10)" )]
        [InlineData( SqlTemporalUnit.Year, "EXTRACT_TEMPORAL(0, 10)" )]
        public void Visit_ShouldInterpretExtractTemporalUnitFunction(SqlTemporalUnit unit, string expected)
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractTemporalUnit( unit ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( SqlTemporalUnit.Nanosecond, "TEMPORAL_ADD(8, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Microsecond, "TEMPORAL_ADD(7, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Millisecond, "TEMPORAL_ADD(6, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Second, "TEMPORAL_ADD(5, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Minute, "TEMPORAL_ADD(4, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Hour, "TEMPORAL_ADD(3, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Day, "TEMPORAL_ADD(9, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Week, "TEMPORAL_ADD(2, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Month, "TEMPORAL_ADD(1, 5, 10)" )]
        [InlineData( SqlTemporalUnit.Year, "TEMPORAL_ADD(0, 5, 10)" )]
        public void Visit_ShouldInterpretTemporalAddFunction(SqlTemporalUnit unit, string expected)
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).TemporalAdd( SqlNode.Literal( 5 ), unit ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( SqlTemporalUnit.Nanosecond, "TEMPORAL_DIFF(8, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Microsecond, "TEMPORAL_DIFF(7, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Millisecond, "TEMPORAL_DIFF(6, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Second, "TEMPORAL_DIFF(5, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Minute, "TEMPORAL_DIFF(4, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Hour, "TEMPORAL_DIFF(3, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Day, "TEMPORAL_DIFF(9, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Week, "TEMPORAL_DIFF(2, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Month, "TEMPORAL_DIFF(1, 10, 5)" )]
        [InlineData( SqlTemporalUnit.Year, "TEMPORAL_DIFF(0, 10, 5)" )]
        public void Visit_ShouldInterpretTemporalDiffFunction(SqlTemporalUnit unit, string expected)
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).TemporalDiff( SqlNode.Literal( 5 ), unit ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Fact]
        public void Visit_ShouldInterpretNewGuidFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.NewGuid() );
            sut.Context.Sql.ToString().Should().Be( "NEW_GUID()" );
        }

        [Fact]
        public void Visit_ShouldInterpretLengthFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Length( SqlNode.Literal( "foo" ) ) );
            sut.Context.Sql.ToString().Should().Be( "LENGTH('foo')" );
        }

        [Fact]
        public void Visit_ShouldInterpretByteLengthFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.ByteLength( SqlNode.Literal( "foo" ) ) );
            sut.Context.Sql.ToString().Should().Be( "OCTET_LENGTH('foo')" );
        }

        [Fact]
        public void Visit_ShouldInterpretToLowerFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.ToLower( SqlNode.Literal( "FOO" ) ) );
            sut.Context.Sql.ToString().Should().Be( "TO_LOWER('FOO')" );
        }

        [Fact]
        public void Visit_ShouldInterpretToUpperFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.ToUpper( SqlNode.Literal( "foo" ) ) );
            sut.Context.Sql.ToString().Should().Be( "TO_UPPER('foo')" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimStartFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.TrimStart( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "LTRIM((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimEndFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.TrimEnd( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "RTRIM((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Trim( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "TRIM((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretSubstringFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Substring( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( 10 ), SqlNode.Literal( 5 ) ) );
            sut.Context.Sql.ToString().Should().Be( "SUBSTR((foo.a), 10, 5)" );
        }

        [Fact]
        public void Visit_ShouldInterpretReplaceFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Replace( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "REPLACE((foo.a), 'foo', 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretReverseFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Reverse( SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "REVERSE((foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretIndexOfFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.IndexOf( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "INSTR((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretLastIndexOfFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.LastIndexOf( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "INSTR_LAST((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretSignFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Sign( SqlNode.Parameter<int>( "a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "SIGN(@a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretAbsFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Abs( SqlNode.Parameter<int>( "a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "ABS(@a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCeilingFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Ceiling( SqlNode.Parameter<int>( "a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "CEIL(@a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretFloorFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Floor( SqlNode.Parameter<int>( "a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "FLOOR(@a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretTruncateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Truncate( SqlNode.Parameter<int>( "a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "TRUNC(@a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretTruncateFunction_WithPrecision()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Truncate( SqlNode.Parameter<int>( "a" ), SqlNode.Parameter<int>( "p" ) ) );
            sut.Context.Sql.ToString().Should().Be( "TRUNC2(@a, @p)" );
        }

        [Fact]
        public void Visit_ShouldInterpretRoundFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Round( SqlNode.Parameter<int>( "a" ), SqlNode.Parameter<int>( "p" ) ) );
            sut.Context.Sql.ToString().Should().Be( "ROUND(@a, @p)" );
        }

        [Fact]
        public void Visit_ShouldInterpretPowerFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Power( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "POW(@a, (foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretSquareRootFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.SquareRoot( SqlNode.Parameter<int>( "a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "SQRT(@a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretMinFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Null().Min( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "MIN(NULL, @a, (foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretMinFunctionWithOneParameterThatRequiresParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Min( Array.Empty<SqlExpressionNode>() ) );
            sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretMinFunctionWithOneParameterThatDoesNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Min( Array.Empty<SqlExpressionNode>() ) );
            sut.Context.Sql.ToString().Should().Be( "25" );
        }

        [Fact]
        public void Visit_ShouldInterpretMaxFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Null().Max( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "MAX(NULL, @a, (foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretMaxFunctionWithOneParameterThatRequiresParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Max( Array.Empty<SqlExpressionNode>() ) );
            sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
        }

        [Fact]
        public void Visit_ShouldInterpretMaxFunctionWithOneParameterThatDoesNotRequireParentheses()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Max( Array.Empty<SqlExpressionNode>() ) );
            sut.Context.Sql.ToString().Should().Be( "25" );
        }

        [Fact]
        public void VisitChild_ShouldInterpretSimpleFunctionWithoutParentheses()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Functions.CurrentDate() );
            sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_DATE()" );
        }

        [Fact]
        public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenFunctionIsCustom()
        {
            var sut = CreateInterpreter();
            var function = new SqlFunctionNodeMock();

            var action = Lambda.Of( () => sut.Visit( function ) );

            action.Should()
                .ThrowExactly<UnrecognizedSqlNodeException>()
                .AndMatch( e => ReferenceEquals( e.Node, function ) && ReferenceEquals( e.Visitor, sut ) );
        }

        [Fact]
        public void Visit_ShouldInterpretNamedAggregateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.AggregateFunctions.Named(
                    SqlSchemaObjectName.Create( "foo", "bar" ),
                    SqlNode.Parameter<int>( "a" ),
                    SqlNode.RawExpression( "qux.a" ) ) );

            sut.Context.Sql.ToString().Should().Be( "\"foo_bar\"(@a, (qux.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretNamedAggregateFunctionWithTraits()
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.AggregateFunctions.Named(
                        SqlSchemaObjectName.Create( "foo", "bar" ),
                        SqlNode.Parameter<int>( "a" ),
                        SqlNode.RawExpression( "qux.a" ) )
                    .Distinct()
                    .AndWhere( SqlNode.RawCondition( "foo.a > 10" ) )
                    .OrderBy( SqlNode.Parameter<int>( "b" ).Asc(), SqlNode.Parameter<int>( "c" ).Desc() ) );

            sut.Context.Sql.ToString().Should().Be( "\"foo_bar\"(DISTINCT @a, (qux.a)) FILTER (WHERE foo.a > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretNamedAggregateFunctionWithTraits_WithEnabledOrdering()
        {
            var sut = CreateInterpreter( SqliteNodeInterpreterOptions.Default.EnableAggregateFunctionOrdering() );
            sut.Visit(
                SqlNode.AggregateFunctions.Named(
                        SqlSchemaObjectName.Create( "foo", "bar" ),
                        SqlNode.Parameter<int>( "a" ),
                        SqlNode.RawExpression( "qux.a" ) )
                    .Distinct()
                    .AndWhere( SqlNode.RawCondition( "foo.a > 10" ) )
                    .OrderBy( SqlNode.Parameter<int>( "b" ).Asc(), SqlNode.Parameter<int>( "c" ).Desc() ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be( "\"foo_bar\"(DISTINCT @a, (qux.a) ORDER BY @b ASC, @c DESC) FILTER (WHERE foo.a > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretMinAggregateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Min() );
            sut.Context.Sql.ToString().Should().Be( "MIN(25)" );
        }

        [Fact]
        public void Visit_ShouldInterpretMinAggregateFunctionWithTraits()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Min().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
            sut.Context.Sql.ToString().Should().Be( "MIN(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretMaxAggregateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Max() );
            sut.Context.Sql.ToString().Should().Be( "MAX(25)" );
        }

        [Fact]
        public void Visit_ShouldInterpretMaxAggregateFunctionWithTraits()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Max().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
            sut.Context.Sql.ToString().Should().Be( "MAX(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretAverageAggregateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Average() );
            sut.Context.Sql.ToString().Should().Be( "AVG(25)" );
        }

        [Fact]
        public void Visit_ShouldInterpretAverageAggregateFunctionWithTraits()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Average().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
            sut.Context.Sql.ToString().Should().Be( "AVG(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretSumAggregateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Sum() );
            sut.Context.Sql.ToString().Should().Be( "SUM(25)" );
        }

        [Fact]
        public void Visit_ShouldInterpretSumAggregateFunctionWithTraits()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Sum().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
            sut.Context.Sql.ToString().Should().Be( "SUM(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCountAggregateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 25 ).Count() );
            sut.Context.Sql.ToString().Should().Be( "COUNT(25)" );
        }

        [Fact]
        public void Visit_ShouldInterpretCountAggregateFunctionWithTraits()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.RawExpression( "foo.a" ).Count().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
            sut.Context.Sql.ToString().Should().Be( "COUNT(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretStringConcatAggregateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( "foo" ).StringConcat() );
            sut.Context.Sql.ToString().Should().Be( "GROUP_CONCAT('foo')" );
        }

        [Fact]
        public void Visit_ShouldInterpretStringConcatAggregateFunctionWithTraits()
        {
            var sut = CreateInterpreter();
            sut.Visit(
                SqlNode.RawExpression( "foo.a" )
                    .StringConcat( SqlNode.Literal( " - " ) )
                    .Distinct()
                    .AndWhere( SqlNode.RawCondition( "foo.b > 10" ) ) );

            sut.Context.Sql.ToString().Should().Be( "GROUP_CONCAT(DISTINCT (foo.a), ' - ') FILTER (WHERE foo.b > 10)" );
        }

        [Fact]
        public void Visit_ShouldInterpretRowNumberWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.WindowFunctions.RowNumber().Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "ROW_NUMBER() OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretRankWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.WindowFunctions.Rank().Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "RANK() OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretDenseRankWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.WindowFunctions.DenseRank().Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "DENSE_RANK() OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretCumulativeDistributionWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.WindowFunctions.CumulativeDistribution().Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "CUME_DIST() OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretNTileWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.RawExpression( "bar.a" ).NTile().Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "NTILE((bar.a)) OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretLagWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.RawExpression( "bar.a" ).Lag( SqlNode.Literal( 3 ), SqlNode.Literal( "x" ) ).Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "LAG((bar.a), 3, 'x') OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretLeadWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.RawExpression( "bar.a" ).Lead( SqlNode.Literal( 3 ), SqlNode.Literal( "x" ) ).Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "LEAD((bar.a), 3, 'x') OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretFirstValueWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.RawExpression( "bar.a" ).FirstValue().Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "FIRST_VALUE((bar.a)) OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretLastValueWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.RawExpression( "bar.a" ).LastValue().Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "LAST_VALUE((bar.a)) OVER \"foo\"" );
        }

        [Fact]
        public void Visit_ShouldInterpretNthValueWindowFunction()
        {
            var sut = CreateInterpreter();
            var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
            sut.Visit( SqlNode.RawExpression( "bar.a" ).NthValue( SqlNode.Literal( 5 ) ).Over( window ) );
            sut.Context.Sql.ToString().Should().Be( "NTH_VALUE((bar.a), 5) OVER \"foo\"" );
        }

        [Fact]
        public void VisitChild_ShouldInterpretAggregateFunctionWithoutParentheses_WhenTraitsAreEmpty()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "COUNT((foo.a))" );
        }

        [Fact]
        public void VisitChild_ShouldInterpretAggregateFunctionWithoutParentheses_WhenTraitsOnlyContainsDistinct()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) ).Distinct() );
            sut.Context.Sql.ToString().Should().Be( "COUNT(DISTINCT (foo.a))" );
        }

        [Fact]
        public void VisitChild_ShouldInterpretAggregateFunctionWithParentheses_WhenTraitsContainsNonDistinct()
        {
            var sut = CreateInterpreter();
            sut.VisitChild(
                SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) )
                    .Distinct()
                    .AndWhere( SqlNode.RawCondition( "foo.a > 10" ) )
                    .Over( SqlNode.WindowDefinition( "wnd", new[] { SqlNode.RawExpression( "foo.a" ).Asc() } ) ) );

            sut.Context.Sql.ToString().Should().Be( "(COUNT(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10) OVER \"wnd\")" );
        }

        [Fact]
        public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenAggregateFunctionIsCustom()
        {
            var sut = CreateInterpreter();
            var function = new SqlAggregateFunctionNodeMock();

            var action = Lambda.Of( () => sut.Visit( function ) );

            action.Should()
                .ThrowExactly<UnrecognizedSqlNodeException>()
                .AndMatch( e => ReferenceEquals( e.Node, function ) && ReferenceEquals( e.Visitor, sut ) );
        }
    }
}
