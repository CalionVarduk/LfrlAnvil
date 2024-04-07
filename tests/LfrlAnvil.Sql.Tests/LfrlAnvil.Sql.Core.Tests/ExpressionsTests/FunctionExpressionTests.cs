using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.TestExtensions.FluentAssertions;
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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Named );
            sut.Arguments.Should().BeSequentiallyEqualTo( arguments );
            text.Should().Be( "[foo].[bar]((\"10\" : System.Int32), (\"20\" : System.Int32))" );
        }
    }

    [Fact]
    public void Named_ShouldCreateNamedAggregateFunctionExpressionNode()
    {
        var arguments = new[] { SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) };
        var trait = SqlNode.DistinctTrait();
        var sut = SqlNode.AggregateFunctions.Named( SqlSchemaObjectName.Create( "foo", "bar" ), arguments ).AddTrait( trait );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Named );
            sut.Arguments.Should().BeSequentiallyEqualTo( arguments );
            sut.Traits.Should().BeSequentiallyEqualTo( trait );
            text.Should()
                .Be(
                    @"AGG_[foo].[bar]((""10"" : System.Int32), (""20"" : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void Coalesce_ShouldThrowArgumentException_WhenArgumentsAreEmpty()
    {
        var action = Lambda.Of( () => SqlNode.Functions.Coalesce() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Coalesce_ShouldCreateCoalesceFunctionExpressionNode_WithOneArgument()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.Coalesce();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Coalesce );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "COALESCE((\"10\" : System.Int32))" );
        }
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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Coalesce );
            sut.Arguments.Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "COALESCE((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void CurrentDate_ShouldCreateCurrentDateFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentDate();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.CurrentDate );
            sut.Arguments.Should().BeEmpty();
            text.Should().Be( "CURRENT_DATE()" );
        }
    }

    [Fact]
    public void CurrentTime_ShouldCreateCurrentTimeFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentTime();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.CurrentTime );
            sut.Arguments.Should().BeEmpty();
            text.Should().Be( "CURRENT_TIME()" );
        }
    }

    [Fact]
    public void CurrentDateTime_ShouldCreateCurrentDateTimeFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentDateTime();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.CurrentDateTime );
            sut.Arguments.Should().BeEmpty();
            text.Should().Be( "CURRENT_DATETIME()" );
        }
    }

    [Fact]
    public void CurrentUtcDateTime_ShouldCreateCurrentUtcDateTimeFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentUtcDateTime();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.CurrentUtcDateTime );
            sut.Arguments.Should().BeEmpty();
            text.Should().Be( "CURRENT_UTC_DATETIME()" );
        }
    }

    [Fact]
    public void CurrentTimestamp_ShouldCreateCurrentTimestampFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.CurrentTimestamp();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.CurrentTimestamp );
            sut.Arguments.Should().BeEmpty();
            text.Should().Be( "CURRENT_TIMESTAMP()" );
        }
    }

    [Fact]
    public void ExtractDate_ShouldCreateExtractDateFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDate();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ExtractDate );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "EXTRACT_DATE((\"10\" : System.Int32))" );
        }
    }

    [Fact]
    public void ExtractTimeOfDay_ShouldCreateExtractTimeOfDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractTimeOfDay();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ExtractTimeOfDay );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "EXTRACT_TIME_OF_DAY((\"10\" : System.Int32))" );
        }
    }

    [Fact]
    public void ExtractDayOfYear_ShouldCreateExtractDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDayOfYear();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ExtractDay );
            sut.Unit.Should().Be( SqlTemporalUnit.Year );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "EXTRACT_DAY_OF_YEAR((\"10\" : System.Int32))" );
        }
    }

    [Fact]
    public void ExtractDayOfMonth_ShouldCreateExtractDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDayOfMonth();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ExtractDay );
            sut.Unit.Should().Be( SqlTemporalUnit.Month );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "EXTRACT_DAY_OF_MONTH((\"10\" : System.Int32))" );
        }
    }

    [Fact]
    public void ExtractDayOfWeek_ShouldCreateExtractDayFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( 10 );
        var sut = arg.ExtractDayOfWeek();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ExtractDay );
            sut.Unit.Should().Be( SqlTemporalUnit.Week );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "EXTRACT_DAY_OF_WEEK((\"10\" : System.Int32))" );
        }
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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ExtractTemporalUnit );
            sut.Unit.Should().Be( unit );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( $"EXTRACT_TEMPORAL_{expectedUnit}((\"10\" : System.Int32))" );
        }
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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.TemporalAdd );
            sut.Unit.Should().Be( unit );
            sut.Arguments.Should().BeSequentiallyEqualTo( args[0], args[1] );
            text.Should().Be( $"TEMPORAL_ADD_{expectedUnit}((\"10\" : System.Int32), (\"20\" : System.Int32))" );
        }
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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.TemporalDiff );
            sut.Unit.Should().Be( unit );
            sut.Arguments.Should().BeSequentiallyEqualTo( args[0], args[1] );
            text.Should().Be( $"TEMPORAL_DIFF_{expectedUnit}((\"10\" : System.Int32), (\"20\" : System.Int32))" );
        }
    }

    [Fact]
    public void NewGuid_ShouldCreateNewGuidFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.NewGuid();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.NewGuid );
            sut.Arguments.Should().BeEmpty();
            text.Should().Be( "NEW_GUID()" );
        }
    }

    [Fact]
    public void Length_ShouldCreateLengthFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.Length();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Length );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "LENGTH((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void ByteLength_ShouldCreateByteLengthFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.ByteLength();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ByteLength );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "BYTE_LENGTH((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void ToLower_ShouldCreateToLowerFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "FOO" );
        var sut = arg.ToLower();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ToLower );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TO_LOWER((\"FOO\" : System.String))" );
        }
    }

    [Fact]
    public void ToUpper_ShouldCreateToUpperFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.ToUpper();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.ToUpper );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TO_UPPER((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void TrimStart_ShouldCreateTrimStartFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.TrimStart();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.TrimStart );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TRIM_START((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void TrimStart_ShouldCreateTrimStartFunctionExpressionNode_WithCharacters()
    {
        var arg = SqlNode.Literal( "foo" );
        var characters = SqlNode.Literal( "f" );
        var sut = arg.TrimStart( characters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.TrimStart );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, characters );
            text.Should().Be( "TRIM_START((\"foo\" : System.String), (\"f\" : System.String))" );
        }
    }

    [Fact]
    public void TrimEnd_ShouldCreateTrimEndFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.TrimEnd();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.TrimEnd );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TRIM_END((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void TrimEnd_ShouldCreateTrimEndFunctionExpressionNode_WithCharacters()
    {
        var arg = SqlNode.Literal( "foo" );
        var characters = SqlNode.Literal( "o" );
        var sut = arg.TrimEnd( characters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.TrimEnd );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, characters );
            text.Should().Be( "TRIM_END((\"foo\" : System.String), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void Trim_ShouldCreateTrimFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.Trim();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Trim );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TRIM((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void Trim_ShouldCreateTrimFunctionExpressionNode_WithCharacters()
    {
        var arg = SqlNode.Literal( "foo" );
        var characters = SqlNode.Literal( "o" );
        var sut = arg.Trim( characters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Trim );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, characters );
            text.Should().Be( "TRIM((\"foo\" : System.String), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void Substring_ShouldCreateSubstringFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var startIndex = SqlNode.Literal( 5 );
        var sut = arg.Substring( startIndex );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Substring );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, startIndex );
            text.Should().Be( "SUBSTRING((\"foo\" : System.String), (\"5\" : System.Int32))" );
        }
    }

    [Fact]
    public void Substring_ShouldCreateSubstringFunctionExpressionNode_WithLength()
    {
        var arg = SqlNode.Literal( "foo" );
        var startIndex = SqlNode.Literal( 5 );
        var length = SqlNode.Literal( 10 );
        var sut = arg.Substring( startIndex, length );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Substring );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, startIndex, length );
            text.Should().Be( "SUBSTRING((\"foo\" : System.String), (\"5\" : System.Int32), (\"10\" : System.Int32))" );
        }
    }

    [Fact]
    public void Replace_ShouldCreateReplaceFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var oldValue = SqlNode.Literal( "f" );
        var newValue = SqlNode.Literal( "b" );
        var sut = arg.Replace( oldValue, newValue );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Replace );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, oldValue, newValue );
            text.Should().Be( "REPLACE((\"foo\" : System.String), (\"f\" : System.String), (\"b\" : System.String))" );
        }
    }

    [Fact]
    public void Reverse_ShouldCreateReverseFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.Reverse();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Reverse );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "REVERSE((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void IndexOf_ShouldCreateIndexOfFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.IndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.IndexOf );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "INDEX_OF((\"foo\" : System.String), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void LastIndexOf_ShouldCreateIndexOfFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( "foo" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.LastIndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.LastIndexOf );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "LAST_INDEX_OF((\"foo\" : System.String), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void Sign_ShouldCreateSignFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Sign();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Sign );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SIGN((@a : System.Double))" );
        }
    }

    [Fact]
    public void Abs_ShouldCreateAbsFunctionExpressionNode()
    {
        var arg = SqlNode.Literal( -10 );
        var sut = arg.Abs();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Abs );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "ABS((\"-10\" : System.Int32))" );
        }
    }

    [Fact]
    public void Floor_ShouldCreateFloorFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Floor();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Floor );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "FLOOR((@a : System.Double))" );
        }
    }

    [Fact]
    public void Ceiling_ShouldCreateCeilingFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Ceiling();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Ceiling );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "CEILING((@a : System.Double))" );
        }
    }

    [Fact]
    public void Truncate_ShouldCreateTruncateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Truncate();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Truncate );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TRUNCATE((@a : System.Double))" );
        }
    }

    [Fact]
    public void Truncate_ShouldCreateTruncateFunctionExpressionNode_WithPrecision()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var precision = SqlNode.Parameter<int>( "p" );
        var sut = arg.Truncate( precision );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Truncate );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, precision );
            text.Should().Be( "TRUNCATE((@a : System.Double), (@p : System.Int32))" );
        }
    }

    [Fact]
    public void Round_ShouldCreateRoundFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var precision = SqlNode.Parameter<int>( "p" );
        var sut = arg.Round( precision );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Round );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, precision );
            text.Should().Be( "ROUND((@a : System.Double), (@p : System.Int32))" );
        }
    }

    [Fact]
    public void Power_ShouldCreatePowerFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var power = SqlNode.Parameter<int>( "b" );
        var sut = arg.Power( power );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Power );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, power );
            text.Should().Be( "POWER((@a : System.Int32), (@b : System.Int32))" );
        }
    }

    [Fact]
    public void SquareRoot_ShouldCreateSquareRootFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.SquareRoot();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.SquareRoot );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SQUARE_ROOT((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Count_ShouldCreateCountAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Count();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Count );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "AGG_COUNT((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Count_ShouldCreateCountAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Count().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Count );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"AGG_COUNT((@a : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void Min_ShouldCreateMinAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Min();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "AGG_MIN((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Min_ShouldCreateMinAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Min().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"AGG_MIN((@a : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void Min_ShouldThrowArgumentException_WhenArgumentsAreEmpty()
    {
        var action = Lambda.Of( () => SqlNode.Functions.Min() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Min_ShouldCreateMinFunctionExpressionNode_WithOneArgument()
    {
        var arg = SqlNode.Parameter( "a" );
        var sut = SqlNode.Functions.Min( arg );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "MIN((@a : ?))" );
        }
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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Arguments.Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MIN((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Max_ShouldCreateMaxAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Max();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "AGG_MAX((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Max_ShouldCreateMaxAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Max().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"AGG_MAX((@a : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void Max_ShouldThrowArgumentException_WhenArgumentsAreEmpty()
    {
        var action = Lambda.Of( () => SqlNode.Functions.Max() );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Max_ShouldCreateMaxFunctionExpressionNode_WithOneArgument()
    {
        var arg = SqlNode.Parameter( "a" );
        var sut = SqlNode.Functions.Max( arg );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "MAX((@a : ?))" );
        }
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

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Arguments.Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MAX((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Sum_ShouldCreateSumAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Sum();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Sum );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "AGG_SUM((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Sum_ShouldCreateSumAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Sum().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Sum );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"AGG_SUM((@a : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void Average_ShouldCreateAverageAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a", isNullable: true );
        var sut = arg.Average();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Average );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "AGG_AVERAGE((@a : Nullable<System.Int32>))" );
        }
    }

    [Fact]
    public void Average_ShouldCreateAverageAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Average().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Average );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"AGG_AVERAGE((@a : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void StringConcat_ShouldCreateStringConcatAggregateFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.StringConcat();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.StringConcat );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "AGG_STRING_CONCAT((@a : System.String))" );
        }
    }

    [Fact]
    public void StringConcat_ShouldCreateStringConcatAggregateFunctionExpressionNode_WithSeparator()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var separator = SqlNode.Literal( "," );
        var sut = arg.StringConcat( separator );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.StringConcat );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, separator );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "AGG_STRING_CONCAT((@a : System.String), (\",\" : System.String))" );
        }
    }

    [Fact]
    public void StringConcat_ShouldCreateStringConcatAggregateFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.StringConcat().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.StringConcat );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"AGG_STRING_CONCAT((@a : System.String))
  DISTINCT" );
        }
    }

    [Fact]
    public void RowNumber_ShouldCreateRowNumberWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.RowNumber();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.RowNumber );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_ROW_NUMBER()" );
        }
    }

    [Fact]
    public void RowNumber_ShouldCreateRowNumberWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.RowNumber().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.RowNumber );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_ROW_NUMBER()
  DISTINCT" );
        }
    }

    [Fact]
    public void Rank_ShouldCreateRankWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.Rank();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Rank );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_RANK()" );
        }
    }

    [Fact]
    public void Rank_ShouldCreateRankWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.Rank().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Rank );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_RANK()
  DISTINCT" );
        }
    }

    [Fact]
    public void DenseRank_ShouldCreateDenseRankWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.DenseRank();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.DenseRank );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_DENSE_RANK()" );
        }
    }

    [Fact]
    public void DenseRank_ShouldCreateDenseRankWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.DenseRank().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.DenseRank );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_DENSE_RANK()
  DISTINCT" );
        }
    }

    [Fact]
    public void CumulativeDistribution_ShouldCreateCumulativeDistributionWindowFunctionExpressionNode()
    {
        var sut = SqlNode.WindowFunctions.CumulativeDistribution();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.CumulativeDistribution );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_CUMULATIVE_DISTRIBUTION()" );
        }
    }

    [Fact]
    public void CumulativeDistribution_ShouldCreateCumulativeDistributionWindowFunctionExpressionNode_WithTrait()
    {
        var sut = SqlNode.WindowFunctions.CumulativeDistribution().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.CumulativeDistribution );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_CUMULATIVE_DISTRIBUTION()
  DISTINCT" );
        }
    }

    [Fact]
    public void NTile_ShouldCreateNTileWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.NTile();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.NTile );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_N_TILE((@a : System.Int32))" );
        }
    }

    [Fact]
    public void NTile_ShouldCreateNTileWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.NTile().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.NTile );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_N_TILE((@a : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.Lag();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lag );
            sut.Arguments.Count.Should().Be( 2 );
            sut.Arguments.ElementAtOrDefault( 0 ).Should().BeSameAs( arg );
            sut.Arguments.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.Literal( 1 ) );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LAG((@a : System.String), (\"1\" : System.Int32))" );
        }
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithOffset()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var sut = arg.Lag( offset );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lag );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, offset );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LAG((@a : System.String), (@b : System.Int32))" );
        }
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithOffsetAndDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lag( offset, @default );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lag );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, offset, @default );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LAG((@a : System.String), (@b : System.Int32), (@c : System.String))" );
        }
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lag( offset: null, @default );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lag );
            sut.Arguments.Count.Should().Be( 3 );
            sut.Arguments.ElementAtOrDefault( 0 ).Should().BeSameAs( arg );
            sut.Arguments.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.Literal( 1 ) );
            sut.Arguments.ElementAtOrDefault( 2 ).Should().BeSameAs( @default );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LAG((@a : System.String), (\"1\" : System.Int32), (@c : System.String))" );
        }
    }

    [Fact]
    public void Lag_ShouldCreateLagWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lag( offset, @default ).Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lag );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, offset, @default );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_LAG((@a : System.String), (@b : System.Int32), (@c : System.String))
  DISTINCT" );
        }
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.Lead();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lead );
            sut.Arguments.Count.Should().Be( 2 );
            sut.Arguments.ElementAtOrDefault( 0 ).Should().BeSameAs( arg );
            sut.Arguments.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.Literal( 1 ) );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LEAD((@a : System.String), (\"1\" : System.Int32))" );
        }
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithOffset()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var sut = arg.Lead( offset );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lead );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, offset );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LEAD((@a : System.String), (@b : System.Int32))" );
        }
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithOffsetAndDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lead( offset, @default );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lead );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, offset, @default );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LEAD((@a : System.String), (@b : System.Int32), (@c : System.String))" );
        }
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithDefault()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lead( offset: null, @default );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lead );
            sut.Arguments.Count.Should().Be( 3 );
            sut.Arguments.ElementAtOrDefault( 0 ).Should().BeSameAs( arg );
            sut.Arguments.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.Literal( 1 ) );
            sut.Arguments.ElementAtOrDefault( 2 ).Should().BeSameAs( @default );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LEAD((@a : System.String), (\"1\" : System.Int32), (@c : System.String))" );
        }
    }

    [Fact]
    public void Lead_ShouldCreateLeadWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var offset = SqlNode.Parameter<int>( "b" );
        var @default = SqlNode.Parameter<string>( "c" );
        var sut = arg.Lead( offset, @default ).Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Lead );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, offset, @default );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_LEAD((@a : System.String), (@b : System.Int32), (@c : System.String))
  DISTINCT" );
        }
    }

    [Fact]
    public void FirstValue_ShouldCreateFirstValueWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.FirstValue();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.FirstValue );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_FIRST_VALUE((@a : System.String))" );
        }
    }

    [Fact]
    public void FirstValue_ShouldCreateFirstValueWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.FirstValue().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.FirstValue );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_FIRST_VALUE((@a : System.String))
  DISTINCT" );
        }
    }

    [Fact]
    public void LastValue_ShouldCreateLastValueWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.LastValue();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.LastValue );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_LAST_VALUE((@a : System.String))" );
        }
    }

    [Fact]
    public void LastValue_ShouldCreateLastValueWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.LastValue().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.LastValue );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_LAST_VALUE((@a : System.String))
  DISTINCT" );
        }
    }

    [Fact]
    public void NthValue_ShouldCreateNthValueWindowFunctionExpressionNode()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var n = SqlNode.Parameter<int>( "b" );
        var sut = arg.NthValue( n );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.NthValue );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, n );
            sut.Traits.Should().BeEmpty();
            text.Should().Be( "WND_NTH_VALUE((@a : System.String), (@b : System.Int32))" );
        }
    }

    [Fact]
    public void NthValue_ShouldCreateNthValueWindowFunctionExpressionNode_WithTrait()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var n = SqlNode.Parameter<int>( "b" );
        var sut = arg.NthValue( n ).Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.NthValue );
            sut.Arguments.Should().BeSequentiallyEqualTo( arg, n );
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
            text.Should()
                .Be(
                    @"WND_NTH_VALUE((@a : System.String), (@b : System.Int32))
  DISTINCT" );
        }
    }

    [Fact]
    public void BaseAggregateFunctionNodeAddTrait_ShouldCallSetTraits()
    {
        var sut = new SqlAggregateFunctionNodeMock().AddTrait( SqlNode.DistinctTrait() );

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Custom );
            sut.Arguments.Should().BeEmpty();
            sut.Traits.Should().HaveCount( 1 );
            (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctTrait );
        }
    }
}
