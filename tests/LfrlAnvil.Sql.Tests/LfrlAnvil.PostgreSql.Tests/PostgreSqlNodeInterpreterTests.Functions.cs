using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.PostgreSql.Tests;

public partial class PostgreSqlNodeInterpreterTests
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

            sut.Context.Sql.ToString().Should().Be( "\"foo\".\"bar\"(@a, (qux.a))" );
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
            sut.Context.Sql.ToString().Should().Be( "CURRENT_DATE" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentTimeFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentTime() );
            sut.Context.Sql.ToString().Should().Be( "LOCALTIME" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentDateTimeFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentDateTime() );
            sut.Context.Sql.ToString().Should().Be( "LOCALTIMESTAMP" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentUtcDateTimeFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentUtcDateTime() );
            sut.Context.Sql.ToString().Should().Be( "CURRENT_TIMESTAMP" );
        }

        [Fact]
        public void Visit_ShouldInterpretCurrentTimestampFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.CurrentTimestamp() );
            sut.Context.Sql.ToString().Should().Be( "CAST(EXTRACT(EPOCH FROM CURRENT_TIMESTAMP) * 10000000 AS INT8)" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDateFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDate() );
            sut.Context.Sql.ToString().Should().Be( "10::DATE" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractTimeOfDayFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractTimeOfDay() );
            sut.Context.Sql.ToString().Should().Be( "10::TIME" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDayOfYearFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDayOfYear() );
            sut.Context.Sql.ToString().Should().Be( "EXTRACT(DOY FROM 10)::INT2" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDayOfMonthFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDayOfMonth() );
            sut.Context.Sql.ToString().Should().Be( "EXTRACT(DAY FROM 10)::INT2" );
        }

        [Fact]
        public void Visit_ShouldInterpretExtractDayOfWeekFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractDayOfWeek() );
            sut.Context.Sql.ToString().Should().Be( "EXTRACT(DOW FROM 10)::INT2" );
        }

        [Theory]
        [InlineData( SqlTemporalUnit.Nanosecond, "(EXTRACT(MICROSECONDS FROM 10) % 1000000 * 1000)::INT4" )]
        [InlineData( SqlTemporalUnit.Microsecond, "(EXTRACT(MICROSECONDS FROM 10) % 1000000)::INT4" )]
        [InlineData( SqlTemporalUnit.Millisecond, "(EXTRACT(MILLISECONDS FROM 10) % 1000)::INT2" )]
        [InlineData( SqlTemporalUnit.Second, "EXTRACT(SECOND FROM 10)::INT2" )]
        [InlineData( SqlTemporalUnit.Minute, "EXTRACT(MINUTE FROM 10)::INT2" )]
        [InlineData( SqlTemporalUnit.Hour, "EXTRACT(HOUR FROM 10)::INT2" )]
        [InlineData( SqlTemporalUnit.Day, "EXTRACT(DAY FROM 10)::INT2" )]
        [InlineData( SqlTemporalUnit.Week, "EXTRACT(WEEK FROM 10)::INT2" )]
        [InlineData( SqlTemporalUnit.Month, "EXTRACT(MONTH FROM 10)::INT2" )]
        [InlineData( SqlTemporalUnit.Year, "EXTRACT(YEAR FROM 10)::INT4" )]
        public void Visit_ShouldInterpretExtractTemporalUnitFunction(SqlTemporalUnit unit, string expected)
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).ExtractTemporalUnit( unit ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( SqlTemporalUnit.Nanosecond, "10 + MAKE_INTERVAL(SECS => 5 / 1000000000.0)" )]
        [InlineData( SqlTemporalUnit.Microsecond, "10 + MAKE_INTERVAL(SECS => 5 / 1000000.0)" )]
        [InlineData( SqlTemporalUnit.Millisecond, "10 + MAKE_INTERVAL(SECS => 5 / 1000.0)" )]
        [InlineData( SqlTemporalUnit.Second, "10 + MAKE_INTERVAL(SECS => 5)" )]
        [InlineData( SqlTemporalUnit.Minute, "10 + MAKE_INTERVAL(MINS => 5)" )]
        [InlineData( SqlTemporalUnit.Hour, "10 + MAKE_INTERVAL(HOURS => 5)" )]
        [InlineData( SqlTemporalUnit.Day, "10 + MAKE_INTERVAL(DAYS => 5)" )]
        [InlineData( SqlTemporalUnit.Week, "10 + MAKE_INTERVAL(WEEKS => 5)" )]
        [InlineData( SqlTemporalUnit.Month, "10 + MAKE_INTERVAL(MONTHS => 5)" )]
        [InlineData( SqlTemporalUnit.Year, "10 + MAKE_INTERVAL(YEARS => 5)" )]
        public void Visit_ShouldInterpretTemporalAddFunction(SqlTemporalUnit unit, string expected)
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Literal( 10 ).TemporalAdd( SqlNode.Literal( 5 ), unit ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( SqlTemporalUnit.Nanosecond, "(10 + MAKE_INTERVAL(SECS => 5 / 1000000000.0))" )]
        [InlineData( SqlTemporalUnit.Microsecond, "(10 + MAKE_INTERVAL(SECS => 5 / 1000000.0))" )]
        [InlineData( SqlTemporalUnit.Millisecond, "(10 + MAKE_INTERVAL(SECS => 5 / 1000.0))" )]
        [InlineData( SqlTemporalUnit.Second, "(10 + MAKE_INTERVAL(SECS => 5))" )]
        [InlineData( SqlTemporalUnit.Minute, "(10 + MAKE_INTERVAL(MINS => 5))" )]
        [InlineData( SqlTemporalUnit.Hour, "(10 + MAKE_INTERVAL(HOURS => 5))" )]
        [InlineData( SqlTemporalUnit.Day, "(10 + MAKE_INTERVAL(DAYS => 5))" )]
        [InlineData( SqlTemporalUnit.Week, "(10 + MAKE_INTERVAL(WEEKS => 5))" )]
        [InlineData( SqlTemporalUnit.Month, "(10 + MAKE_INTERVAL(MONTHS => 5))" )]
        [InlineData( SqlTemporalUnit.Year, "(10 + MAKE_INTERVAL(YEARS => 5))" )]
        public void VisitChild_ShouldInterpretTemporalAddFunction(SqlTemporalUnit unit, string expected)
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Literal( 10 ).TemporalAdd( SqlNode.Literal( 5 ), unit ) );
            sut.Context.Sql.ToString().Should().Be( expected );
        }

        [Theory]
        [InlineData( SqlTemporalUnit.Nanosecond, "TRUNC(EXTRACT(EPOCH FROM 5 - 10) * 1000000000)::INT8" )]
        [InlineData( SqlTemporalUnit.Microsecond, "TRUNC(EXTRACT(EPOCH FROM 5 - 10) * 1000000)::INT8" )]
        [InlineData( SqlTemporalUnit.Millisecond, "TRUNC(EXTRACT(EPOCH FROM 5 - 10) * 1000)::INT8" )]
        [InlineData( SqlTemporalUnit.Second, "TRUNC(EXTRACT(EPOCH FROM 5 - 10))::INT8" )]
        [InlineData( SqlTemporalUnit.Minute, "TRUNC(EXTRACT(EPOCH FROM 5 - 10) / 60)::INT8" )]
        [InlineData( SqlTemporalUnit.Hour, "TRUNC(EXTRACT(EPOCH FROM 5 - 10) / 3600)::INT8" )]
        [InlineData( SqlTemporalUnit.Day, "EXTRACT(DAY FROM 5::TIMESTAMP - 10::TIMESTAMP)::INT4" )]
        [InlineData( SqlTemporalUnit.Week, "TRUNC(EXTRACT(DAY FROM 5::TIMESTAMP - 10::TIMESTAMP) / 7)::INT4" )]
        [InlineData( SqlTemporalUnit.Month, "(EXTRACT(YEAR FROM AGE(5, 10)) * 12 + EXTRACT(MONTH FROM AGE(5, 10)))::INT4" )]
        [InlineData( SqlTemporalUnit.Year, "EXTRACT(YEAR FROM AGE(5, 10))::INT4" )]
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
            sut.Context.Sql.ToString().Should().Be( "UUID_GENERATE_V4()" );
        }

        [Fact]
        public void Visit_ShouldInterpretLengthFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Length( SqlNode.Literal( "foo" ) ) );
            sut.Context.Sql.ToString().Should().Be( "CHAR_LENGTH('foo')" );
        }

        [Fact]
        public void Visit_ShouldInterpretByteLengthFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.ByteLength( SqlNode.Literal( "foo" ) ) );
            sut.Context.Sql.ToString().Should().Be( "LENGTH('foo')" );
        }

        [Fact]
        public void Visit_ShouldInterpretToLowerFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.ToLower( SqlNode.Literal( "FOO" ) ) );
            sut.Context.Sql.ToString().Should().Be( "LOWER('FOO')" );
        }

        [Fact]
        public void Visit_ShouldInterpretToUpperFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.ToUpper( SqlNode.Literal( "foo" ) ) );
            sut.Context.Sql.ToString().Should().Be( "UPPER('foo')" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimStartFunction_WithOneArgument()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.TrimStart( SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "LTRIM((foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimStartFunction_WithTwoArguments()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.TrimStart( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "LTRIM((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimEndFunction_WithOneArgument()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.TrimEnd( SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "RTRIM((foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimEndFunction_WithTwoArguments()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.TrimEnd( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "RTRIM((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimFunction_WithOneArgument()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Trim( SqlNode.RawExpression( "foo.a" ) ) );
            sut.Context.Sql.ToString().Should().Be( "BTRIM((foo.a))" );
        }

        [Fact]
        public void Visit_ShouldInterpretTrimFunction_WithTwoArguments()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.Trim( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString().Should().Be( "BTRIM((foo.a), 'bar')" );
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
            sut.Context.Sql.ToString().Should().Be( "STRPOS((foo.a), 'bar')" );
        }

        [Fact]
        public void Visit_ShouldInterpretLastIndexOfFunction()
        {
            var sut = CreateInterpreter();
            sut.Visit( SqlNode.Functions.LastIndexOf( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    "CASE WHEN STRPOS(REVERSE((foo.a)), REVERSE('bar')) = 0 THEN 0 ELSE CHAR_LENGTH((foo.a)) - STRPOS(REVERSE((foo.a)), REVERSE('bar')) - GREATEST(CHAR_LENGTH('bar'), CASE WHEN 'bar' IS NULL THEN NULL ELSE 1 END) + 2 END" );
        }

        [Fact]
        public void VisitChild_ShouldInterpretLastIndexOfFunction()
        {
            var sut = CreateInterpreter();
            sut.VisitChild( SqlNode.Functions.LastIndexOf( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    "(CASE WHEN STRPOS(REVERSE((foo.a)), REVERSE('bar')) = 0 THEN 0 ELSE CHAR_LENGTH((foo.a)) - STRPOS(REVERSE((foo.a)), REVERSE('bar')) - GREATEST(CHAR_LENGTH('bar'), CASE WHEN 'bar' IS NULL THEN NULL ELSE 1 END) + 2 END)" );
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
            sut.Context.Sql.ToString().Should().Be( "TRUNC(@a, @p)" );
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
            sut.Context.Sql.ToString().Should().Be( "POWER(@a, (foo.a))" );
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
            sut.Context.Sql.ToString().Should().Be( "LEAST(NULL, @a, (foo.a))" );
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
            sut.Context.Sql.ToString().Should().Be( "GREATEST(NULL, @a, (foo.a))" );
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
            sut.VisitChild( SqlNode.Functions.NewGuid() );
            sut.Context.Sql.ToString().Should().Be( "UUID_GENERATE_V4()" );
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

            sut.Context.Sql.ToString().Should().Be( "\"foo\".\"bar\"(@a, (qux.a))" );
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

            sut.Context.Sql.ToString()
                .Should()
                .Be( "\"foo\".\"bar\"(DISTINCT @a, (qux.a) ORDER BY @b ASC, @c DESC) FILTER (WHERE foo.a > 10)" );
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
            sut.Context.Sql.ToString().Should().Be( "STRING_AGG('foo', ',')" );
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

            sut.Context.Sql.ToString().Should().Be( "STRING_AGG(DISTINCT (foo.a), ' - ') FILTER (WHERE foo.b > 10)" );
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
