using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public class FunctionExpressionTests : TestsBase
{
    [Fact]
    public void RecordsAffected_ShouldCreateRecordsAffectedFunctionExpressionNode()
    {
        var sut = SqlNode.Functions.RecordsAffected();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.RecordsAffected );
            sut.Type.Should().Be( SqlExpressionType.Create<long>() );
            sut.Arguments.ToArray().Should().BeEmpty();
            text.Should().Be( "RECORDSAFFECTED()" );
        }
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "COALESCE((\"10\" : System.Int32))" );
        }
    }

    [Fact]
    public void Coalesce_ShouldCreateCoalesceFunctionExpressionNode_WithManyArguments_WithUnknownType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter( "b" ),
            SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Coalesce( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Coalesce );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "COALESCE((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Coalesce_ShouldCreateCoalesceFunctionExpressionNode_WithManyArguments_WithNonNullableType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter<int>( "b" ),
            SqlNode.Parameter( "c" )
        };

        var sut = args[0].Coalesce( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Coalesce );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "COALESCE((@a : Nullable<System.Int32>), (@b : System.Int32), (@c : ?))" );
        }
    }

    [Fact]
    public void Coalesce_ShouldCreateCoalesceFunctionExpressionNode_WithManyArguments_WithNullableType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter<int>( "b", isNullable: true ),
            SqlNode.Parameter<int>( "c", isNullable: true )
        };

        var sut = args[0].Coalesce( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Coalesce );
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "COALESCE((@a : Nullable<System.Int32>), (@b : Nullable<System.Int32>), (@c : Nullable<System.Int32>))" );
        }
    }

    [Fact]
    public void Coalesce_ShouldCreateCoalesceFunctionExpressionNode_WithManyArguments_WithIncompatibleArgumentTypes()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter<double>( "b" ),
            SqlNode.Parameter<long>( "c" )
        };

        var sut = args[0].Coalesce( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Coalesce );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "COALESCE((@a : Nullable<System.Int32>), (@b : System.Double), (@c : System.Int64))" );
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
            sut.Type.Should().Be( SqlExpressionType.Create<DateOnly>() );
            sut.Arguments.ToArray().Should().BeEmpty();
            text.Should().Be( "CURRENTDATE()" );
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
            sut.Type.Should().Be( SqlExpressionType.Create<TimeOnly>() );
            sut.Arguments.ToArray().Should().BeEmpty();
            text.Should().Be( "CURRENTTIME()" );
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
            sut.Type.Should().Be( SqlExpressionType.Create<DateTime>() );
            sut.Arguments.ToArray().Should().BeEmpty();
            text.Should().Be( "CURRENTDATETIME()" );
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
            sut.Type.Should().Be( SqlExpressionType.Create<TimeSpan>() );
            sut.Arguments.ToArray().Should().BeEmpty();
            text.Should().Be( "CURRENTTIMESTAMP()" );
        }
    }

    [Fact]
    public void Length_ShouldCreateLengthFunctionExpressionNode_WithUnknownType()
    {
        var arg = SqlNode.Parameter( "a" );
        var sut = SqlNode.Functions.Length( arg );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Length );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "LENGTH((@a : ?))" );
        }
    }

    [Fact]
    public void Length_ShouldCreateLengthFunctionExpressionNode_WithNonNullableType()
    {
        var arg = SqlNode.Literal( "foo" );
        var sut = arg.Length();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Length );
            sut.Type.Should().Be( SqlExpressionType.Create<long>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "LENGTH((\"foo\" : System.String))" );
        }
    }

    [Fact]
    public void Length_ShouldCreateLengthFunctionExpressionNode_WithNullableType()
    {
        var arg = SqlNode.Parameter<string>( "a", isNullable: true );
        var sut = arg.Length();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Length );
            sut.Type.Should().Be( SqlExpressionType.Create<long>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "LENGTH((@a : Nullable<System.String>))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TOLOWER((\"FOO\" : System.String))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TOUPPER((\"foo\" : System.String))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TRIMSTART((\"foo\" : System.String))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, characters );
            text.Should().Be( "TRIMSTART((\"foo\" : System.String), (\"f\" : System.String))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TRIMEND((\"foo\" : System.String))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, characters );
            text.Should().Be( "TRIMEND((\"foo\" : System.String), (\"o\" : System.String))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, characters );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, startIndex );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, startIndex, length );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, oldValue, newValue );
            text.Should().Be( "REPLACE((\"foo\" : System.String), (\"f\" : System.String), (\"b\" : System.String))" );
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
            sut.Type.Should().Be( SqlExpressionType.Create<long>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "INDEXOF((\"foo\" : System.String), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void IndexOf_ShouldCreateIndexOfFunctionExpressionNode_WithUnknownType()
    {
        var arg = SqlNode.Parameter( "a" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.IndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.IndexOf );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "INDEXOF((@a : ?), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void IndexOf_ShouldCreateIndexOfFunctionExpressionNode_WithNonNullableType()
    {
        var arg = SqlNode.Literal( "foo" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.IndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.IndexOf );
            sut.Type.Should().Be( SqlExpressionType.Create<long>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "INDEXOF((\"foo\" : System.String), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void IndexOf_ShouldCreateIndexOfFunctionExpressionNode_WithNullableType()
    {
        var arg = SqlNode.Parameter<string>( "a", isNullable: true );
        var value = SqlNode.Literal( "o" );
        var sut = arg.IndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.IndexOf );
            sut.Type.Should().Be( SqlExpressionType.Create<long>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "INDEXOF((@a : Nullable<System.String>), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void LastIndexOf_ShouldCreateLastIndexOfFunctionExpressionNode_WithUnknownType()
    {
        var arg = SqlNode.Parameter( "a" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.LastIndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.LastIndexOf );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "LASTINDEXOF((@a : ?), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void LastIndexOf_ShouldCreateLastIndexOfFunctionExpressionNode_WithNonNullableType()
    {
        var arg = SqlNode.Literal( "foo" );
        var value = SqlNode.Literal( "o" );
        var sut = arg.LastIndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.LastIndexOf );
            sut.Type.Should().Be( SqlExpressionType.Create<long>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "LASTINDEXOF((\"foo\" : System.String), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void LastIndexOf_ShouldCreateLastIndexOfFunctionExpressionNode_WithNullableType()
    {
        var arg = SqlNode.Parameter<string>( "a", isNullable: true );
        var value = SqlNode.Literal( "o" );
        var sut = arg.LastIndexOf( value );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.LastIndexOf );
            sut.Type.Should().Be( SqlExpressionType.Create<long>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, value );
            text.Should().Be( "LASTINDEXOF((@a : Nullable<System.String>), (\"o\" : System.String))" );
        }
    }

    [Fact]
    public void Sign_ShouldCreateSignFunctionExpressionNode_WithUnknownType()
    {
        var arg = SqlNode.Parameter( "a" );
        var sut = arg.Sign();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Sign );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SIGN((@a : ?))" );
        }
    }

    [Fact]
    public void Sign_ShouldCreateSignFunctionExpressionNode_WithNonNullableType()
    {
        var arg = SqlNode.Parameter<double>( "a" );
        var sut = arg.Sign();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Sign );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SIGN((@a : System.Double))" );
        }
    }

    [Fact]
    public void Sign_ShouldCreateSignFunctionExpressionNode_WithNullableType()
    {
        var arg = SqlNode.Parameter<double>( "a", isNullable: true );
        var sut = arg.Sign();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Sign );
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SIGN((@a : Nullable<System.Double>))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "TRUNCATE((@a : System.Double))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, power );
            text.Should().Be( "POWER((@a : System.Int32), (@b : System.Int32))" );
        }
    }

    [Fact]
    public void SquareRoot_ShouldCreateSquareRootFunctionExpressionNode_WithUnknownType()
    {
        var arg = SqlNode.Parameter( "a" );
        var sut = arg.SquareRoot();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.SquareRoot );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SQUAREROOT((@a : ?))" );
        }
    }

    [Fact]
    public void SquareRoot_ShouldCreateSquareRootFunctionExpressionNode_WithNonNullableType()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.SquareRoot();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.SquareRoot );
            sut.Type.Should().Be( SqlExpressionType.Create<double>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SQUAREROOT((@a : System.Int32))" );
        }
    }

    [Fact]
    public void SquareRoot_ShouldCreateSquareRootFunctionExpressionNode_WithNullableType()
    {
        var arg = SqlNode.Parameter<int>( "a", isNullable: true );
        var sut = arg.SquareRoot();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.SquareRoot );
            sut.Type.Should().Be( SqlExpressionType.Create<double>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "SQUAREROOT((@a : Nullable<System.Int32>))" );
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
            sut.Type.Should().Be( SqlExpressionType.Create<long>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().BeEmpty();
            text.Should().Be( "AGG_COUNT((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Count_ShouldCreateCountAggregateFunctionExpressionNode_Decorated()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Count().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Count );
            sut.Type.Should().Be( SqlExpressionType.Create<long>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().BeEmpty();
            text.Should().Be( "AGG_MIN((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Min_ShouldCreateMinAggregateFunctionExpressionNode_Decorated()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Min().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
            text.Should()
                .Be(
                    @"AGG_MIN((@a : System.Int32))
    DISTINCT" );
        }
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "MIN((@a : ?))" );
        }
    }

    [Fact]
    public void Min_ShouldCreateMinFunctionExpressionNode_WithManyArguments_WithUnknownType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter( "b" ),
            SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Min( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MIN((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Min_ShouldCreateMinFunctionExpressionNode_WithManyArguments_WithNonNullableType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a" ),
            SqlNode.Parameter<int>( "b" ),
            SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Min( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MIN((@a : System.Int32), (@b : System.Int32), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Min_ShouldCreateMinFunctionExpressionNode_WithManyArguments_WithNullableType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter<int>( "b", isNullable: true ),
            SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Min( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MIN((@a : Nullable<System.Int32>), (@b : Nullable<System.Int32>), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Min_ShouldCreateMinFunctionExpressionNode_WithManyArguments_WithIncompatibleArgumentTypes()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter<double>( "b" ),
            SqlNode.Parameter<long>( "c" )
        };

        var sut = args[0].Min( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Min );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MIN((@a : Nullable<System.Int32>), (@b : System.Double), (@c : System.Int64))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().BeEmpty();
            text.Should().Be( "AGG_MAX((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Max_ShouldCreateMaxAggregateFunctionExpressionNode_Decorated()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Max().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
            text.Should()
                .Be(
                    @"AGG_MAX((@a : System.Int32))
    DISTINCT" );
        }
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            text.Should().Be( "MAX((@a : ?))" );
        }
    }

    [Fact]
    public void Max_ShouldCreateMaxFunctionExpressionNode_WithManyArguments_WithUnknownType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter( "b" ),
            SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Max( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MAX((@a : Nullable<System.Int32>), (@b : ?), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Max_ShouldCreateMaxFunctionExpressionNode_WithManyArguments_WithNonNullableType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a" ),
            SqlNode.Parameter<int>( "b" ),
            SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Max( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MAX((@a : System.Int32), (@b : System.Int32), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Max_ShouldCreateMaxFunctionExpressionNode_WithManyArguments_WithNullableType()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter<int>( "b", isNullable: true ),
            SqlNode.Parameter<int>( "c" )
        };

        var sut = args[0].Max( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MAX((@a : Nullable<System.Int32>), (@b : Nullable<System.Int32>), (@c : System.Int32))" );
        }
    }

    [Fact]
    public void Max_ShouldCreateMaxFunctionExpressionNode_WithManyArguments_WithIncompatibleArgumentTypes()
    {
        var args = new SqlExpressionNode[]
        {
            SqlNode.Parameter<int>( "a", isNullable: true ),
            SqlNode.Parameter<double>( "b" ),
            SqlNode.Parameter<long>( "c" )
        };

        var sut = args[0].Max( args[1], args[2] );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.FunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Max );
            sut.Type.Should().BeNull();
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( args );
            text.Should().Be( "MAX((@a : Nullable<System.Int32>), (@b : System.Double), (@c : System.Int64))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().BeEmpty();
            text.Should().Be( "AGG_SUM((@a : System.Int32))" );
        }
    }

    [Fact]
    public void Sum_ShouldCreateSumAggregateFunctionExpressionNode_Decorated()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Sum().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Sum );
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
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
            sut.Type.Should().Be( SqlExpressionType.Create<double>( isNullable: true ) );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().BeEmpty();
            text.Should().Be( "AGG_AVERAGE((@a : Nullable<System.Int32>))" );
        }
    }

    [Fact]
    public void Average_ShouldCreateAverageAggregateFunctionExpressionNode_Decorated()
    {
        var arg = SqlNode.Parameter<int>( "a" );
        var sut = arg.Average().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.Average );
            sut.Type.Should().Be( SqlExpressionType.Create<double>() );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().BeEmpty();
            text.Should().Be( "AGG_STRINGCONCAT((@a : System.String))" );
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
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg, separator );
            sut.Decorators.Should().BeEmpty();
            text.Should().Be( "AGG_STRINGCONCAT((@a : System.String), (\",\" : System.String))" );
        }
    }

    [Fact]
    public void StringConcat_ShouldCreateStringConcatAggregateFunctionExpressionNode_Decorated()
    {
        var arg = SqlNode.Parameter<string>( "a" );
        var sut = arg.StringConcat().Distinct();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AggregateFunctionExpression );
            sut.FunctionType.Should().Be( SqlFunctionType.StringConcat );
            sut.Type.Should().Be( arg.Type );
            sut.Arguments.ToArray().Should().BeSequentiallyEqualTo( arg );
            sut.Decorators.Should().HaveCount( 1 );
            (sut.Decorators.ElementAtOrDefault( 0 )?.NodeType).Should().Be( SqlNodeType.DistinctDecorator );
            text.Should()
                .Be(
                    @"AGG_STRINGCONCAT((@a : System.String))
    DISTINCT" );
        }
    }
}
