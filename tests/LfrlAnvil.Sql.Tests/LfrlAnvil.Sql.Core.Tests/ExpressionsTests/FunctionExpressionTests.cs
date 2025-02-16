using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class FunctionExpressionTests : TestsBase
{
    [Fact]
    public void Named_ShouldCreateNamedFunctionExpressionNode()
    {
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };
        var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo", "bar" ), arguments );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Named ),
                sut.Arguments.TestSequence( arguments ),
                text.TestEquals( "[foo].[bar]((\"10\" : System.Int32), (\"20\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Named_ShouldCreateNamedAggregateFunctionExpressionNode()
    {
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };
        var trait = SqlNode.DistinctTrait();
        var sut = SqlNode.AggregateFunctions.Named( SqlSchemaObjectName.Create( "foo", "bar" ), arguments ).AddTrait( trait );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Named ),
                sut.Arguments.TestSequence( arguments ),
                sut.Traits.TestSequence( [ trait ] ),
                text.TestEquals(
                    """
                    AGG_[foo].[bar](("10" : System.Int32), ("20" : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Coalesce_ShouldThrowArgumentException_WhenArgumentsAreEmpty()
    {
        var action = Lambda.Of( () => SqlNode.Functions.Coalesce() );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Coalesce_ShouldCreateCoalesceFunctionExpressionNode_WithOneArgument()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.Coalesce();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Coalesce ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "COALESCE((\"10\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Coalesce_ShouldCreateCoalesceFunctionExpressionNode_WithManyArguments()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ), SqlNode.Parameter( "b" ), SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Coalesce( args[1], args[2] );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Coalesce ),
                sut.Arguments.TestSequence( args ),
                text.TestEquals( "COALESCE((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void CurrentDate_ShouldCreateCurrentDateFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentDate();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.CurrentDate ),
                sut.Arguments.TestEmpty(),
                text.TestEquals( "CURRENT_DATE()" ) )
            .Go();
    }

    [Fact]
    public void CurrentTime_ShouldCreateCurrentTimeFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentTime();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.CurrentTime ),
                sut.Arguments.TestEmpty(),
                text.TestEquals( "CURRENT_TIME()" ) )
            .Go();
    }

    [Fact]
    public void CurrentDateTime_ShouldCreateCurrentDateTimeFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentDateTime();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.CurrentDateTime ),
                sut.Arguments.TestEmpty(),
                text.TestEquals( "CURRENT_DATETIME()" ) )
            .Go();
    }

    [Fact]
    public void CurrentUtcDateTime_ShouldCreateCurrentUtcDateTimeFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentUtcDateTime();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.CurrentUtcDateTime ),
                sut.Arguments.TestEmpty(),
                text.TestEquals( "CURRENT_UTC_DATETIME()" ) )
            .Go();
    }

    [Fact]
    public void CurrentTimestamp_ShouldCreateCurrentTimestampFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentTimestamp();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.CurrentTimestamp ),
                sut.Arguments.TestEmpty(),
                text.TestEquals( "CURRENT_TIMESTAMP()" ) )
            .Go();
    }

    [Fact]
    public void ExtractDate_ShouldCreateExtractDateFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDate();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ExtractDate ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "EXTRACT_DATE((\"10\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void ExtractTimeOfDay_ShouldCreateExtractTimeOfDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractTimeOfDay();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ExtractTimeOfDay ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "EXTRACT_TIME_OF_DAY((\"10\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void ExtractDayOfYear_ShouldCreateExtractDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDayOfYear();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ExtractDay ),
                sut.Unit.TestEquals( SqlTemporalUnit.Year ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "EXTRACT_DAY_OF_YEAR((\"10\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void ExtractDayOfMonth_ShouldCreateExtractDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDayOfMonth();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ExtractDay ),
                sut.Unit.TestEquals( SqlTemporalUnit.Month ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "EXTRACT_DAY_OF_MONTH((\"10\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void ExtractDayOfWeek_ShouldCreateExtractDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDayOfWeek();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ExtractDay ),
                sut.Unit.TestEquals( SqlTemporalUnit.Week ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "EXTRACT_DAY_OF_WEEK((\"10\" : System.Int32))" ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlTemporalUnit.Year, "YEAR" )]
    [InlineData( SqlTemporalUnit.Month, "MONTH" )]
    [InlineData( SqlTemporalUnit.Week, "WEEK" )]
    [InlineData( SqlTemporalUnit.Day, "DAY" )]
    [InlineData( SqlTemporalUnit.Hour, "HOUR" )]
    [InlineData( SqlTemporalUnit.Minute, "MINUTE" )]
    [InlineData( SqlTemporalUnit.Second, "SECOND" )]
    [InlineData( SqlTemporalUnit.Millisecond, "MILLISECOND" )]
    [InlineData( SqlTemporalUnit.Microsecond, "MICROSECOND" )]
    [InlineData( SqlTemporalUnit.Nanosecond, "NANOSECOND" )]
    public void ExtractTemporalUnit_ShouldCreateExtractTemporalUnitFunctionExpressionNode(SqlTemporalUnit unit, string expectedUnit)
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractTemporalUnit( unit );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ExtractTemporalUnit ),
                sut.Unit.TestEquals( unit ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( $"EXTRACT_TEMPORAL_{expectedUnit}((\"10\" : System.Int32))" ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlTemporalUnit.Year, "YEAR" )]
    [InlineData( SqlTemporalUnit.Month, "MONTH" )]
    [InlineData( SqlTemporalUnit.Week, "WEEK" )]
    [InlineData( SqlTemporalUnit.Day, "DAY" )]
    [InlineData( SqlTemporalUnit.Hour, "HOUR" )]
    [InlineData( SqlTemporalUnit.Minute, "MINUTE" )]
    [InlineData( SqlTemporalUnit.Second, "SECOND" )]
    [InlineData( SqlTemporalUnit.Millisecond, "MILLISECOND" )]
    [InlineData( SqlTemporalUnit.Microsecond, "MICROSECOND" )]
    [InlineData( SqlTemporalUnit.Nanosecond, "NANOSECOND" )]
    public void TemporalAdd_ShouldCreateTemporalAddFunctionExpressionNode(SqlTemporalUnit unit, string expectedUnit)
    {
        var args = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };
        var sut = args[0].TemporalAdd( args[1], unit );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.TemporalAdd ),
                sut.Unit.TestEquals( unit ),
                sut.Arguments.TestSequence( [ args[0], args[1] ] ),
                text.TestEquals( $"TEMPORAL_ADD_{expectedUnit}((\"10\" : System.Int32), (\"20\" : System.Int32))" ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlTemporalUnit.Year, "YEAR" )]
    [InlineData( SqlTemporalUnit.Month, "MONTH" )]
    [InlineData( SqlTemporalUnit.Week, "WEEK" )]
    [InlineData( SqlTemporalUnit.Day, "DAY" )]
    [InlineData( SqlTemporalUnit.Hour, "HOUR" )]
    [InlineData( SqlTemporalUnit.Minute, "MINUTE" )]
    [InlineData( SqlTemporalUnit.Second, "SECOND" )]
    [InlineData( SqlTemporalUnit.Millisecond, "MILLISECOND" )]
    [InlineData( SqlTemporalUnit.Microsecond, "MICROSECOND" )]
    [InlineData( SqlTemporalUnit.Nanosecond, "NANOSECOND" )]
    public void TemporalDiff_ShouldCreateTemporalDiffFunctionExpressionNode(SqlTemporalUnit unit, string expectedUnit)
    {
        var args = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };
        var sut = args[0].TemporalDiff( args[1], unit );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.TemporalDiff ),
                sut.Unit.TestEquals( unit ),
                sut.Arguments.TestSequence( [ args[0], args[1] ] ),
                text.TestEquals( $"TEMPORAL_DIFF_{expectedUnit}((\"10\" : System.Int32), (\"20\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void NewGuid_ShouldCreateNewGuidFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.NewGuid();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.NewGuid ),
                sut.Arguments.TestEmpty(),
                text.TestEquals( "NEW_GUID()" ) )
            .Go();
    }

    [Fact]
    public void Length_ShouldCreateLengthFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.Length();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Length ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "LENGTH((\"foo\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void ByteLength_ShouldCreateByteLengthFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.ByteLength();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ByteLength ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "BYTE_LENGTH((\"foo\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void ToLower_ShouldCreateToLowerFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "FOO" );
        var sut = arg.ToLower();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ToLower ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "TO_LOWER((\"FOO\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void ToUpper_ShouldCreateToUpperFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.ToUpper();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.ToUpper ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "TO_UPPER((\"foo\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void TrimStart_ShouldCreateTrimStartFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.TrimStart();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.TrimStart ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "TRIM_START((\"foo\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void TrimStart_ShouldCreateTrimStartFunctionExpressionNode_WithCharacters()
    {
        var arg = SqlNode.Literal( "foo" );
        var characters = SqlNode.Literal( "f" );
        var sut = arg.TrimStart( characters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.TrimStart ),
                sut.Arguments.TestSequence( [ arg, characters ] ),
                text.TestEquals( "TRIM_START((\"foo\" : System.String), (\"f\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void TrimEnd_ShouldCreateTrimEndFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.TrimEnd();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.TrimEnd ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "TRIM_END((\"foo\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void TrimEnd_ShouldCreateTrimEndFunctionExpressionNode_WithCharacters()
    {
        var arg = SqlNode.Literal( "foo" );
        var characters = SqlNode.Literal( "o" );
        var sut = arg.TrimEnd( characters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.TrimEnd ),
                sut.Arguments.TestSequence( [ arg, characters ] ),
                text.TestEquals( "TRIM_END((\"foo\" : System.String), (\"o\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Trim_ShouldCreateTrimFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.Trim();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Trim ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "TRIM((\"foo\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Trim_ShouldCreateTrimFunctionExpressionNode_WithCharacters()
    {
        var arg = SqlNode.Literal( "foo" );
        var characters = SqlNode.Literal( "o" );
        var sut = arg.Trim( characters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Trim ),
                sut.Arguments.TestSequence( [ arg, characters ] ),
                text.TestEquals( "TRIM((\"foo\" : System.String), (\"o\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Substring_ShouldCreateSubstringFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var startIndex = SqlNode.Literal( 5 );
        var sut = arg.Substring( startIndex );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Substring ),
                sut.Arguments.TestSequence( [ arg, startIndex ] ),
                text.TestEquals( "SUBSTRING((\"foo\" : System.String), (\"5\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Substring_ShouldCreateSubstringFunctionExpressionNode_WithLength()
    {
        var arg = SqlNode.Literal( "foo" );
        var startIndex = SqlNode.Literal( 5 );
        var length = SqlNode.Literal( 10 );
        var sut = arg.Substring( startIndex, length );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Substring ),
                sut.Arguments.TestSequence( [ arg, startIndex, length ] ),
                text.TestEquals( "SUBSTRING((\"foo\" : System.String), (\"5\" : System.Int32), (\"10\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Replace_ShouldCreateReplaceFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var oldValue = SqlNode.Literal( "f" );
        var newValue = SqlNode.Literal( "b" );
        var sut = arg.Replace( oldValue, newValue );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Replace ),
                sut.Arguments.TestSequence( [ arg, oldValue, newValue ] ),
                text.TestEquals( "REPLACE((\"foo\" : System.String), (\"f\" : System.String), (\"b\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Reverse_ShouldCreateReverseFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.Reverse();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Reverse ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "REVERSE((\"foo\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void IndexOf_ShouldCreateIndexOfFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.IndexOf( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.IndexOf ),
                sut.Arguments.TestSequence( [ arg, value ] ),
                text.TestEquals( "INDEX_OF((\"foo\" : System.String), (\"o\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void LastIndexOf_ShouldCreateIndexOfFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.LastIndexOf( value );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.LastIndexOf ),
                sut.Arguments.TestSequence( [ arg, value ] ),
                text.TestEquals( "LAST_INDEX_OF((\"foo\" : System.String), (\"o\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Sign_ShouldCreateSignFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Sign();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Sign ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "SIGN((@a : System.Double))" ) )
            .Go();
    }

    [Fact]
    public void Abs_ShouldCreateAbsFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( -10 );
        var sut = arg.Abs();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Abs ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "ABS((\"-10\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Floor_ShouldCreateFloorFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Floor();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Floor ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "FLOOR((@a : System.Double))" ) )
            .Go();
    }

    [Fact]
    public void Ceiling_ShouldCreateCeilingFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Ceiling();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Ceiling ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "CEILING((@a : System.Double))" ) )
            .Go();
    }

    [Fact]
    public void Truncate_ShouldCreateTruncateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Truncate();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Truncate ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "TRUNCATE((@a : System.Double))" ) )
            .Go();
    }

    [Fact]
    public void Truncate_ShouldCreateTruncateFunctionExpressionNode_WithPrecision()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var precision = SqlNode.Parameter<int>( "p" );
        var sut = arg.Truncate( precision );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Truncate ),
                sut.Arguments.TestSequence( [ arg, precision ] ),
                text.TestEquals( "TRUNCATE((@a : System.Double), (@p : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Round_ShouldCreateRoundFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var precision = SqlNode.Parameter<int>( "p" );
        var sut = arg.Round( precision );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Round ),
                sut.Arguments.TestSequence( [ arg, precision ] ),
                text.TestEquals( "ROUND((@a : System.Double), (@p : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Power_ShouldCreatePowerFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var power = SqlNode.Parameter<int>( "b" );
        var sut = arg.Power( power );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Power ),
                sut.Arguments.TestSequence( [ arg, power ] ),
                text.TestEquals( "POWER((@a : System.Int32), (@b : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void SquareRoot_ShouldCreateSquareRootFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.SquareRoot();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.SquareRoot ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "SQUARE_ROOT((@a : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Count_ShouldCreateCountAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Count();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Count ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "AGG_COUNT((@a : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Count_ShouldCreateCountAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Count().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Count ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    AGG_COUNT((@a : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Min_ShouldCreateMinAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Min();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Min ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "AGG_MIN((@a : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Min_ShouldCreateMinAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Min().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Min ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    AGG_MIN((@a : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Min_ShouldThrowArgumentException_WhenArgumentsAreEmpty()
    {
        var action = Lambda.Of( () => SqlNode.Functions.Min() );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Min_ShouldCreateMinFunctionExpressionNode_WithOneArgument()
    {
        var arg = SqlNode.Parameter( "a" );
        var sut = SqlNode.Functions.Min( arg );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Min ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "MIN((@a : ?))" ) )
            .Go();
    }

    [Fact]
    public void Min_ShouldCreateMinFunctionExpressionNode_WithManyArguments()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ), SqlNode.Parameter( "b" ), SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Min( args[1], args[2] );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Min ),
                sut.Arguments.TestSequence( args ),
                text.TestEquals( "MIN((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Max_ShouldCreateMaxAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Max();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Max ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "AGG_MAX((@a : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Max_ShouldCreateMaxAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Max().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Max ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    AGG_MAX((@a : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Max_ShouldThrowArgumentException_WhenArgumentsAreEmpty()
    {
        var action = Lambda.Of( () => SqlNode.Functions.Max() );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Max_ShouldCreateMaxFunctionExpressionNode_WithOneArgument()
    {
        var arg = SqlNode.Parameter( "a" );
        var sut = SqlNode.Functions.Max( arg );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Max ),
                sut.Arguments.TestSequence( [ arg ] ),
                text.TestEquals( "MAX((@a : ?))" ) )
            .Go();
    }

    [Fact]
    public void Max_ShouldCreateMaxFunctionExpressionNode_WithManyArguments()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ), SqlNode.Parameter( "b" ), SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Max( args[1], args[2] );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.FunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Max ),
                sut.Arguments.TestSequence( args ),
                text.TestEquals( "MAX((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Sum_ShouldCreateSumAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Sum();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Sum ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "AGG_SUM((@a : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Sum_ShouldCreateSumAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Sum().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Sum ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    AGG_SUM((@a : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Average_ShouldCreateAverageAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a", isNullable: true );
        var sut = arg.Average();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Average ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "AGG_AVERAGE((@a : Nullable<System.Int32>))" ) )
            .Go();
    }

    [Fact]
    public void Average_ShouldCreateAverageAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Average().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Average ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    AGG_AVERAGE((@a : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void StringConcat_ShouldCreateStringConcatAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.StringConcat();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.StringConcat ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "AGG_STRING_CONCAT((@a : System.String))" ) )
            .Go();
    }

    [Fact]
    public void StringConcat_ShouldCreateStringConcatAggregateFunctionExpressionNode_WithSeparator()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var separator = SqlNode.Literal( "," );
        var sut = arg.StringConcat( separator );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.StringConcat ),
                sut.Arguments.TestSequence( [ arg, separator ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "AGG_STRING_CONCAT((@a : System.String), (\",\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void StringConcat_ShouldCreateStringConcatAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.StringConcat().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.StringConcat ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    AGG_STRING_CONCAT((@a : System.String))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void RowNumber_ShouldCreateRowNumberWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.RowNumber();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.RowNumber ),
                sut.Arguments.TestEmpty(),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_ROW_NUMBER()" ) )
            .Go();
    }

    [Fact]
    public void RowNumber_ShouldCreateRowNumberWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.RowNumber().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.RowNumber ),
                sut.Arguments.TestEmpty(),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_ROW_NUMBER()
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Rank_ShouldCreateRankWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.Rank();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Rank ),
                sut.Arguments.TestEmpty(),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_RANK()" ) )
            .Go();
    }

    [Fact]
    public void Rank_ShouldCreateRankWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.Rank().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Rank ),
                sut.Arguments.TestEmpty(),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_RANK()
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void DenseRank_ShouldCreateDenseRankWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.DenseRank();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.DenseRank ),
                sut.Arguments.TestEmpty(),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_DENSE_RANK()" ) )
            .Go();
    }

    [Fact]
    public void DenseRank_ShouldCreateDenseRankWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.DenseRank().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.DenseRank ),
                sut.Arguments.TestEmpty(),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_DENSE_RANK()
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void CumulativeDistribution_ShouldCreateCumulativeDistributionWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.CumulativeDistribution();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.CumulativeDistribution ),
                sut.Arguments.TestEmpty(),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_CUMULATIVE_DISTRIBUTION()" ) )
            .Go();
    }

    [Fact]
    public void CumulativeDistribution_ShouldCreateCumulativeDistributionWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.CumulativeDistribution().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.CumulativeDistribution ),
                sut.Arguments.TestEmpty(),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_CUMULATIVE_DISTRIBUTION()
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void NTile_ShouldCreateNTileWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.NTile();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.NTile ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_N_TILE((@a : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void NTile_ShouldCreateNTileWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.NTile().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.NTile ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_N_TILE((@a : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.Lag();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lag ),
                sut.Arguments.Count.TestEquals( 2 ),
                sut.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( arg ),
                sut.Arguments.ElementAtOrDefault( 1 ).TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 1 ) ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LAG((@a : System.String), (\"1\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithOffset()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var sut = arg.Lag( offset );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lag ),
                sut.Arguments.TestSequence( [ arg, offset ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LAG((@a : System.String), (@b : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithOffsetAndDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lag( offset, @default );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lag ),
                sut.Arguments.TestSequence( [ arg, offset, @default ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LAG((@a : System.String), (@b : System.Int32), (@c : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lag( offset: null, @default );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lag ),
                sut.Arguments.Count.TestEquals( 3 ),
                sut.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( arg ),
                sut.Arguments.ElementAtOrDefault( 1 ).TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 1 ) ),
                sut.Arguments.ElementAtOrDefault( 2 ).TestRefEquals( @default ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LAG((@a : System.String), (\"1\" : System.Int32), (@c : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lag( offset, @default ).Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lag ),
                sut.Arguments.TestSequence( [ arg, offset, @default ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_LAG((@a : System.String), (@b : System.Int32), (@c : System.String))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.Lead();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lead ),
                sut.Arguments.Count.TestEquals( 2 ),
                sut.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( arg ),
                sut.Arguments.ElementAtOrDefault( 1 ).TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 1 ) ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LEAD((@a : System.String), (\"1\" : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithOffset()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var sut = arg.Lead( offset );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lead ),
                sut.Arguments.TestSequence( [ arg, offset ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LEAD((@a : System.String), (@b : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithOffsetAndDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lead( offset, @default );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lead ),
                sut.Arguments.TestSequence( [ arg, offset, @default ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LEAD((@a : System.String), (@b : System.Int32), (@c : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lead( offset: null, @default );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lead ),
                sut.Arguments.Count.TestEquals( 3 ),
                sut.Arguments.ElementAtOrDefault( 0 ).TestRefEquals( arg ),
                sut.Arguments.ElementAtOrDefault( 1 ).TestType().AssignableTo<SqlLiteralNode<int>>( n => n.Value.TestEquals( 1 ) ),
                sut.Arguments.ElementAtOrDefault( 2 ).TestRefEquals( @default ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LEAD((@a : System.String), (\"1\" : System.Int32), (@c : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lead( offset, @default ).Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Lead ),
                sut.Arguments.TestSequence( [ arg, offset, @default ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_LEAD((@a : System.String), (@b : System.Int32), (@c : System.String))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void FirstValue_ShouldCreateFirstValueWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.FirstValue();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.FirstValue ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_FIRST_VALUE((@a : System.String))" ) )
            .Go();
    }

    [Fact]
    public void FirstValue_ShouldCreateFirstValueWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.FirstValue().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.FirstValue ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_FIRST_VALUE((@a : System.String))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void LastValue_ShouldCreateLastValueWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.LastValue();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.LastValue ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_LAST_VALUE((@a : System.String))" ) )
            .Go();
    }

    [Fact]
    public void LastValue_ShouldCreateLastValueWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.LastValue().Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.LastValue ),
                sut.Arguments.TestSequence( [ arg ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_LAST_VALUE((@a : System.String))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void NthValue_ShouldCreateNthValueWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var n = SqlNode.Parameter<int>( "b" );
        var sut = arg.NthValue( n );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.NthValue ),
                sut.Arguments.TestSequence( [ arg, n ] ),
                sut.Traits.TestEmpty(),
                text.TestEquals( "WND_NTH_VALUE((@a : System.String), (@b : System.Int32))" ) )
            .Go();
    }

    [Fact]
    public void NthValue_ShouldCreateNthValueWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var n = SqlNode.Parameter<int>( "b" );
        var sut = arg.NthValue( n ).Distinct();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.NthValue ),
                sut.Arguments.TestSequence( [ arg, n ] ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ),
                text.TestEquals(
                    """
                    WND_NTH_VALUE((@a : System.String), (@b : System.Int32))
                      DISTINCT
                    """ ) )
            .Go();
    }

    [Fact]
    public void BaseAggregateFunctionNodeAddTrait_ShouldCallSetTraits()
    {
        var sut = new SqlAggregateFunctionNodeMock().AddTrait( SqlNode.DistinctTrait() );

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AggregateFunctionExpression ),
                sut.FunctionType.TestEquals( SqlFunctionType.Custom ),
                sut.Arguments.TestEmpty(),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ) )
            .Go();
    }
}
