using System.Data;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlConstantExpressionValidatorTests : TestsBase
{
    private readonly SqlDatabaseBuilderMock _db = SqlDatabaseBuilderMock.Create();
    private readonly SqlConstantExpressionValidator _sut = new SqlConstantExpressionValidator();

    [Fact]
    public void VisitRawExpression_ShouldVisitParameters()
    {
        var node = SqlNode.RawExpression( "foo.a + @a + @b", SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitRawExpression( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitRawDataField_RegisterError()
    {
        var node = SqlNode.RawRecordSet( "foo" ).GetField( "a" );
        _sut.VisitRawDataField( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitNull_ShouldDoNothing()
    {
        _sut.VisitNull( SqlNode.Null() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitLiteral_ShouldDoNothing()
    {
        _sut.VisitLiteral( (SqlLiteralNode)SqlNode.Literal( 10 ) );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitParameter_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "b" );
        _sut.VisitParameter( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitColumn_ShouldRegisterError()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "A" ).Asc() );
        var node = SqlDatabaseMock.Create( _db ).Schemas.Default.Objects.GetTable( "T" ).Node.GetField( "A" );

        _sut.VisitColumn( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitColumnBuilder_ShouldRegisterError()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        table.Columns.Create( "A" );
        var node = table.Node.GetField( "A" );

        _sut.VisitColumnBuilder( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitQueryDataField_ShouldRegisterError()
    {
        var node = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .AsSet( "bar" )
            .GetField( "a" );

        _sut.VisitQueryDataField( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitViewDataField_ShouldRegisterError()
    {
        _db.Schemas.Default.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var node = SqlDatabaseMock.Create( _db ).Schemas.Default.Objects.GetView( "V" ).Node.GetField( "a" );

        _sut.VisitViewDataField( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitNegate_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).Negate();
        _sut.VisitNegate( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitAdd_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) + SqlNode.Parameter( "b" );
        _sut.VisitAdd( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitConcat_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).Concat( SqlNode.Parameter( "b" ) );
        _sut.VisitConcat( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitSubtract_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) - SqlNode.Parameter( "b" );
        _sut.VisitSubtract( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitMultiply_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) * SqlNode.Parameter( "b" );
        _sut.VisitMultiply( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitDivide_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) / SqlNode.Parameter( "b" );
        _sut.VisitDivide( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitModulo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) % SqlNode.Parameter( "b" );
        _sut.VisitModulo( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitBitwiseNot_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseNot();
        _sut.VisitBitwiseNot( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitBitwiseAnd_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) & SqlNode.Parameter( "b" );
        _sut.VisitBitwiseAnd( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitBitwiseOr_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) | SqlNode.Parameter( "b" );
        _sut.VisitBitwiseOr( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitBitwiseXor_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) ^ SqlNode.Parameter( "b" );
        _sut.VisitBitwiseXor( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitBitwiseLeftShift_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseLeftShift( SqlNode.Parameter( "b" ) );
        _sut.VisitBitwiseLeftShift( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitBitwiseRightShift_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseRightShift( SqlNode.Parameter( "b" ) );
        _sut.VisitBitwiseRightShift( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitSwitchCase_ShouldVisitConditionAndExpression()
    {
        var node = SqlNode.SwitchCase( SqlNode.RawCondition( "foo.a < @a", SqlNode.Parameter( "a" ) ), SqlNode.Parameter( "b" ) );
        _sut.VisitSwitchCase( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitSwitch_ShouldVisitCasesAndDefault()
    {
        var node = SqlNode.Switch(
            new[]
            {
                SqlNode.SwitchCase( SqlNode.RawCondition( "foo.a < @a", SqlNode.Parameter( "a" ) ), SqlNode.Parameter( "b" ) ),
                SqlNode.SwitchCase( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ), SqlNode.Literal( 10 ) )
            },
            SqlNode.Parameter( "d" ) );

        _sut.VisitSwitch( node );

        _sut.GetErrors().Should().HaveCount( 4 );
    }

    [Fact]
    public void VisitNamedFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ), SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitNamedFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitRecordsAffectedFunction_ShouldDoNothing()
    {
        _sut.VisitRecordsAffectedFunction( SqlNode.Functions.RecordsAffected() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitCoalesceFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Coalesce( SqlNode.Parameter( "b" ) );
        _sut.VisitCoalesceFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitCurrentDateFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentDateFunction( SqlNode.Functions.CurrentDate() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitCurrentTimeFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentTimeFunction( SqlNode.Functions.CurrentTime() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitCurrentDateTimeFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentDateTimeFunction( SqlNode.Functions.CurrentDateTime() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitCurrentTimestampFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentTimestampFunction( SqlNode.Functions.CurrentTimestamp() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitExtractDateFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractDate();
        _sut.VisitExtractDateFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitExtractTimeOfDayFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractTimeOfDay();
        _sut.VisitExtractTimeOfDayFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitExtractDayFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractDayOfYear();
        _sut.VisitExtractDayFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitExtractTemporalUnitFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractTemporalUnit( SqlTemporalUnit.Year );
        _sut.VisitExtractTemporalUnitFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitTemporalAddFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TemporalAdd( SqlNode.Parameter( "b" ), SqlTemporalUnit.Year );
        _sut.VisitTemporalAddFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitTemporalDiffFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TemporalDiff( SqlNode.Parameter( "b" ), SqlTemporalUnit.Year );
        _sut.VisitTemporalDiffFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitNewGuidFunction_ShouldDoNothing()
    {
        _sut.VisitNewGuidFunction( SqlNode.Functions.NewGuid() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitLengthFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Length();
        _sut.VisitLengthFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitByteLengthFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ByteLength();
        _sut.VisitByteLengthFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitToLowerFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ToLower();
        _sut.VisitToLowerFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitToUpperFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ToUpper();
        _sut.VisitToUpperFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitTrimStartFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TrimStart( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimStartFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitTrimEndFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TrimEnd( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimEndFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitTrimFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Trim( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitSubstringFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Substring( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitSubstringFunction( node );
        _sut.GetErrors().Should().HaveCount( 3 );
    }

    [Fact]
    public void VisitReplaceFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Replace( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitReplaceFunction( node );
        _sut.GetErrors().Should().HaveCount( 3 );
    }

    [Fact]
    public void VisitReverseFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Reverse();
        _sut.VisitReverseFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitIndexOfFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).IndexOf( SqlNode.Parameter( "b" ) );
        _sut.VisitIndexOfFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitLastIndexOfFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).LastIndexOf( SqlNode.Parameter( "b" ) );
        _sut.VisitLastIndexOfFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitSignFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Sign();
        _sut.VisitSignFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitAbsFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Abs();
        _sut.VisitAbsFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCeilingFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Ceiling();
        _sut.VisitCeilingFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitFloorFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Floor();
        _sut.VisitFloorFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitTruncateFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Truncate( SqlNode.Parameter( "b" ) );
        _sut.VisitTruncateFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitRoundFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Round( SqlNode.Parameter( "b" ) );
        _sut.VisitRoundFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitPowerFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Power( SqlNode.Parameter( "b" ) );
        _sut.VisitPowerFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitSquareRootFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).SquareRoot();
        _sut.VisitSquareRootFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitMinFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Min( SqlNode.Parameter( "b" ) );
        _sut.VisitMinFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitMaxFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Max( SqlNode.Parameter( "b" ) );
        _sut.VisitMaxFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitCustomFunction_ShouldVisitArguments()
    {
        var node = new FunctionMock( SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitCustomFunction( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitNamedAggregateFunction_ShouldRegisterError()
    {
        var node = SqlNode.AggregateFunctions.Named(
            SqlSchemaObjectName.Create( "foo" ),
            SqlNode.Parameter( "a" ),
            SqlNode.Parameter( "b" ) );

        _sut.VisitNamedAggregateFunction( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitMinAggregateFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).Min();
        _sut.VisitMinAggregateFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitMaxAggregateFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).Max();
        _sut.VisitMaxAggregateFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitAverageAggregateFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).Average();
        _sut.VisitAverageAggregateFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitSumAggregateFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).Sum();
        _sut.VisitSumAggregateFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCountAggregateFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).Count();
        _sut.VisitCountAggregateFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitStringConcatAggregateFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).StringConcat( SqlNode.Parameter( "b" ) );
        _sut.VisitStringConcatAggregateFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRowNumberWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.WindowFunctions.RowNumber();
        _sut.VisitRowNumberWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRankWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.WindowFunctions.Rank();
        _sut.VisitRankWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDenseRankWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.WindowFunctions.DenseRank();
        _sut.VisitDenseRankWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCumulativeDistributionWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.WindowFunctions.CumulativeDistribution();
        _sut.VisitCumulativeDistributionWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitNTileWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).NTile();
        _sut.VisitNTileWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitLagWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).Lag();
        _sut.VisitLagWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitLeadWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).Lead();
        _sut.VisitLeadWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitFirstValueWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).FirstValue();
        _sut.VisitFirstValueWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitLastValueWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).LastValue();
        _sut.VisitLastValueWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitNthValueWindowFunction_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "a" ).NthValue( SqlNode.Literal( 5 ) );
        _sut.VisitNthValueWindowFunction( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCustomAggregateFunction_ShouldRegisterError()
    {
        var node = new AggregateFunctionMock(
            new SqlExpressionNode[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) },
            Chain<SqlTraitNode>.Empty );

        _sut.VisitCustomAggregateFunction( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRawCondition_ShouldVisitParameters()
    {
        var node = SqlNode.RawCondition( "@a > @b", SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitRawCondition( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitTrue_ShouldDoNothing()
    {
        _sut.VisitTrue( SqlNode.True() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitFalse_ShouldDoNothing()
    {
        _sut.VisitFalse( SqlNode.False() );
        _sut.GetErrors().Should().BeEmpty();
    }

    [Fact]
    public void VisitEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitEqualTo( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitNotEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsNotEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitNotEqualTo( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitGreaterThan_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsGreaterThan( SqlNode.Parameter( "b" ) );
        _sut.VisitGreaterThan( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitLessThan_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsLessThan( SqlNode.Parameter( "b" ) );
        _sut.VisitLessThan( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitGreaterThanOrEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsGreaterThanOrEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitGreaterThanOrEqualTo( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitLessThanOrEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsLessThanOrEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitLessThanOrEqualTo( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitAnd_ShouldVisitOperands()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) )
            .And( SqlNode.RawCondition( "foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitAnd( node );

        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitOr_ShouldVisitOperands()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) )
            .Or( SqlNode.RawCondition( "foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitOr( node );

        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitConditionValue_ShouldVisitCondition()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ).ToValue();
        _sut.VisitConditionValue( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitBetween_ShouldVisitValueAndMinAndMax()
    {
        var node = SqlNode.Parameter( "a" ).IsBetween( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitBetween( node );
        _sut.GetErrors().Should().HaveCount( 3 );
    }

    [Fact]
    public void VisitExists_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > 5" ).Exists();
        _sut.VisitExists( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitLike_ShouldVisitValueAndPatternAndEscape()
    {
        var node = SqlNode.Parameter( "a" ).Like( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitLike( node );
        _sut.GetErrors().Should().HaveCount( 3 );
    }

    [Fact]
    public void VisitIn_ShouldVisitValueAndExpressions()
    {
        var node = (SqlInConditionNode)SqlNode.Parameter( "a" ).In( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitIn( node );
        _sut.GetErrors().Should().HaveCount( 3 );
    }

    [Fact]
    public void VisitInQuery_ShouldVisitValueAndQuery()
    {
        var node = SqlNode.Parameter( "a" ).InQuery( SqlNode.RawQuery( "SELECT foo.b FROM foo WHERE foo.b > 5" ) );
        _sut.VisitInQuery( node );
        _sut.GetErrors().Should().HaveCount( 2 );
    }

    [Fact]
    public void VisitRawRecordSet_ShouldRegisterError()
    {
        var node = SqlNode.RawRecordSet( "foo" );
        _sut.VisitRawRecordSet( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitNamedFunctionRecordSet_ShouldRegisterError()
    {
        var node = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
        _sut.VisitNamedFunctionRecordSet( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitTable_ShouldRegisterError()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var node = SqlDatabaseMock.Create( _db ).Schemas.Default.Objects.GetTable( "T" ).Node;

        _sut.VisitTable( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitTableBuilder_ShouldRegisterError()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        var node = table.Node;

        _sut.VisitTableBuilder( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitView_ShouldRegisterError()
    {
        _db.Schemas.Default.Objects.CreateView( "V", SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.GetAll() } ) );
        var node = SqlDatabaseMock.Create( _db ).Schemas.Default.Objects.GetView( "V" ).Node;

        _sut.VisitView( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitViewBuilder_ShouldRegisterError()
    {
        var view = _db.Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.Node;

        _sut.VisitViewBuilder( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitQueryRecordSet_ShouldRegisterError()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > 5" ).AsSet( "bar" );
        _sut.VisitQueryRecordSet( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCommonTableExpressionRecordSet_ShouldRegisterError()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet;
        _sut.VisitCommonTableExpressionRecordSet( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitNewTable_ShouldRegisterError()
    {
        var node = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), new[] { SqlNode.Column<int>( "a" ) } ).AsSet( "bar" );
        _sut.VisitNewTable( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitNewView_ShouldRegisterError()
    {
        var node = SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM qux" ) ).AsSet( "bar" );
        _sut.VisitNewView( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitJoinOn_ShouldRegisterError()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a" ).AsSet( "bar" ).LeftOn( SqlNode.RawCondition( "qux.x = 5" ) );
        _sut.VisitJoinOn( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDataSource_ShouldRegisterError()
    {
        var node = SqlNode.DummyDataSource();
        _sut.VisitDataSource( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitSelectField_RegisterError()
    {
        var node = SqlNode.RawExpression( "foo.a + 5" ).As( "bar" );
        _sut.VisitSelectField( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitSelectCompoundField_ShouldRegisterError()
    {
        var node = (SqlSelectCompoundFieldNode)SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .CompoundWith( SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ).ToUnionAll() )
            .Selection[0];

        _sut.VisitSelectCompoundField( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitSelectRecordSet_ShouldRegisterError()
    {
        var node = SqlNode.RawRecordSet( "foo" ).GetAll();
        _sut.VisitSelectRecordSet( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitSelectAll_ShouldRegisterError()
    {
        var node = SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll();
        _sut.VisitSelectAll( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitSelectExpression_ShouldRegisterError()
    {
        var node = SqlNode.RawExpression( "foo.a + 5" ).As( "bar" ).ToExpression();
        _sut.VisitSelectExpression( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRawQuery_ShouldRegisterError()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo" );
        _sut.VisitRawQuery( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDataSourceQuery_ShouldRegisterError()
    {
        var node = SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.GetAll() } );
        _sut.VisitDataSourceQuery( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCompoundQuery_ShouldRegisterError()
    {
        var node = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.GetAll() } )
            .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnionAll() );

        _sut.VisitCompoundQuery( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCompoundQueryComponent_ShouldRegisterError()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo" ).ToUnionAll();
        _sut.VisitCompoundQueryComponent( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDistinctTrait_ShouldRegisterError()
    {
        _sut.VisitDistinctTrait( SqlNode.DistinctTrait() );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitFilterTrait_ShouldRegisterError()
    {
        var node = SqlNode.FilterTrait( SqlNode.RawCondition( "foo.a > 5" ), isConjunction: true );
        _sut.VisitFilterTrait( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitAggregationTrait_ShouldRegisterError()
    {
        var node = SqlNode.AggregationTrait( SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) );
        _sut.VisitAggregationTrait( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitAggregationFilterTrait_ShouldRegisterError()
    {
        var node = SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "foo.a > 5" ), isConjunction: true );
        _sut.VisitAggregationFilterTrait( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitSortTrait_ShouldRegisterError()
    {
        var node = SqlNode.SortTrait( SqlNode.Literal( 10 ).Asc(), SqlNode.Literal( 20 ).Desc() );
        _sut.VisitSortTrait( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitLimitTrait_ShouldRegisterError()
    {
        var node = SqlNode.LimitTrait( SqlNode.Literal( 10 ) );
        _sut.VisitLimitTrait( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitOffsetTrait_ShouldRegisterError()
    {
        var node = SqlNode.OffsetTrait( SqlNode.Literal( 10 ) );
        _sut.VisitOffsetTrait( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCommonTableExpressionTrait_ShouldRegisterError()
    {
        var node = SqlNode.CommonTableExpressionTrait(
            SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "x" ),
            SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "y" ) );

        _sut.VisitCommonTableExpressionTrait( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitWindowDefinitionTrait_ShouldRegisterError()
    {
        var node = SqlNode.WindowDefinitionTrait(
            SqlNode.WindowDefinition( "foo", Array.Empty<SqlExpressionNode>(), Array.Empty<SqlOrderByNode>() ),
            SqlNode.WindowDefinition( "bar", Array.Empty<SqlExpressionNode>(), Array.Empty<SqlOrderByNode>() ) );

        _sut.VisitWindowDefinitionTrait( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitWindowTrait_ShouldRegisterError()
    {
        var node = SqlNode.WindowTrait(
            SqlNode.WindowDefinition( "foo", Array.Empty<SqlExpressionNode>(), Array.Empty<SqlOrderByNode>() ) );

        _sut.VisitWindowTrait( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitOrderBy_ShouldRegisterError()
    {
        var node = SqlNode.Literal( 10 ).Asc();
        _sut.VisitOrderBy( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCommonTableExpression_ShouldRegisterError()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" );
        _sut.VisitCommonTableExpression( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitWindowDefinition_ShouldRegisterError()
    {
        var node = SqlNode.WindowDefinition( "foo", Array.Empty<SqlExpressionNode>(), Array.Empty<SqlOrderByNode>() );
        _sut.VisitWindowDefinition( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitWindowFrame_ShouldRegisterError()
    {
        var node = SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.CurrentRow );
        _sut.VisitWindowFrame( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitTypeCast_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).CastTo<int>();
        _sut.VisitTypeCast( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitValues_ShouldRegisterError()
    {
        var node = SqlNode.Values( SqlNode.Literal( 10 ) );
        _sut.VisitValues( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRawStatement_ShouldRegisterError()
    {
        var node = SqlNode.RawStatement( "INSERT INTO foo (a, b) VALUES (1, 2)" );
        _sut.VisitRawStatement( node );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitInsertInto_ShouldRegisterError()
    {
        var node = SqlNode.InsertInto(
            SqlNode.RawQuery( "SELECT a FROM foo" ),
            SqlNode.RawRecordSet( "bar" ),
            SqlNode.RawRecordSet( "bar" ).GetField( "a" ) );

        _sut.VisitInsertInto( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitUpdate_ShouldRegisterError()
    {
        var node = SqlNode.Update(
            SqlNode.RawRecordSet( "foo" ).ToDataSource(),
            SqlNode.ValueAssignment( SqlNode.RawRecordSet( "foo" ).GetField( "a" ), SqlNode.Literal( 10 ) ) );

        _sut.VisitUpdate( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitUpsert_ShouldRegisterError()
    {
        var node = SqlNode.Upsert(
            SqlNode.RawQuery( "SELECT a FROM foo" ),
            SqlNode.RawRecordSet( "bar" ),
            new[] { SqlNode.RawRecordSet( "bar" ).GetField( "a" ) },
            (r, i) => new[] { r["b"].Assign( i["b"] ) } );

        _sut.VisitUpsert( node );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitValueAssignment_ShouldRegisterError()
    {
        _sut.VisitValueAssignment( SqlNode.ValueAssignment( SqlNode.RawRecordSet( "foo" ).GetField( "a" ), SqlNode.Literal( 10 ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDeleteFrom_ShouldRegisterError()
    {
        _sut.VisitDeleteFrom( SqlNode.DeleteFrom( SqlNode.RawRecordSet( "foo" ).ToDataSource() ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitTruncate_ShouldRegisterError()
    {
        _sut.VisitTruncate( SqlNode.Truncate( SqlNode.RawRecordSet( "foo" ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitColumnDefinition_ShouldRegisterError()
    {
        _sut.VisitColumnDefinition( SqlNode.Column<int>( "a" ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitPrimaryKeyDefinition_ShouldRegisterError()
    {
        _sut.VisitPrimaryKeyDefinition( SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), ReadOnlyArray<SqlOrderByNode>.Empty ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitForeignKeyDefinition_ShouldRegisterError()
    {
        _sut.VisitForeignKeyDefinition(
            SqlNode.ForeignKey(
                SqlSchemaObjectName.Create( "FK" ),
                Array.Empty<SqlDataFieldNode>(),
                SqlNode.RawRecordSet( "foo" ),
                Array.Empty<SqlDataFieldNode>() ) );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCheckDefinition_ShouldRegisterError()
    {
        _sut.VisitCheckDefinition( SqlNode.Check( SqlSchemaObjectName.Create( "CHK" ), SqlNode.True() ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCreateTable_ShouldRegisterError()
    {
        _sut.VisitCreateTable( SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), new[] { SqlNode.Column<int>( "a" ) } ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCreateView_ShouldRegisterError()
    {
        _sut.VisitCreateView( SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM bar" ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCreateIndex_ShouldRegisterError()
    {
        _sut.VisitCreateIndex(
            SqlNode.CreateIndex(
                SqlSchemaObjectName.Create( "foo" ),
                isUnique: Fixture.Create<bool>(),
                SqlNode.RawRecordSet( "bar" ),
                Array.Empty<SqlOrderByNode>() ) );

        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRenameTable_ShouldRegisterError()
    {
        _sut.VisitRenameTable( SqlNode.RenameTable( SqlRecordSetInfo.Create( "foo" ), SqlSchemaObjectName.Create( "bar" ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRenameColumn_ShouldRegisterError()
    {
        _sut.VisitRenameColumn( SqlNode.RenameColumn( SqlRecordSetInfo.Create( "foo" ), "bar", "qux" ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitAddColumn_ShouldRegisterError()
    {
        _sut.VisitAddColumn( SqlNode.AddColumn( SqlRecordSetInfo.Create( "foo" ), SqlNode.Column<int>( "a" ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDropColumn_ShouldRegisterError()
    {
        _sut.VisitDropColumn( SqlNode.DropColumn( SqlRecordSetInfo.Create( "foo" ), "bar" ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDropTable_ShouldRegisterError()
    {
        _sut.VisitDropTable( SqlNode.DropTable( SqlRecordSetInfo.Create( "foo" ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDropView_ShouldRegisterError()
    {
        _sut.VisitDropView( SqlNode.DropView( SqlRecordSetInfo.Create( "foo" ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitDropIndex_ShouldRegisterError()
    {
        _sut.VisitDropIndex( SqlNode.DropIndex( SqlRecordSetInfo.Create( "bar" ), SqlSchemaObjectName.Create( "foo" ) ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitStatementBatch_ShouldRegisterError()
    {
        _sut.VisitStatementBatch( SqlNode.Batch() );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitBeginTransaction_ShouldRegisterError()
    {
        _sut.VisitBeginTransaction( SqlNode.BeginTransaction( IsolationLevel.Serializable ) );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCommitTransaction_ShouldRegisterError()
    {
        _sut.VisitCommitTransaction( SqlNode.CommitTransaction() );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitRollbackTransaction_ShouldRegisterError()
    {
        _sut.VisitRollbackTransaction( SqlNode.RollbackTransaction() );
        _sut.GetErrors().Should().HaveCount( 1 );
    }

    [Fact]
    public void VisitCustom_ShouldDoNothing()
    {
        _sut.VisitCustom( new NodeMock() );
        _sut.GetErrors().Should().BeEmpty();
    }

    private sealed class FunctionMock : SqlFunctionExpressionNode
    {
        public FunctionMock(params SqlExpressionNode[] arguments)
            : base( arguments ) { }
    }

    private sealed class AggregateFunctionMock : SqlAggregateFunctionExpressionNode
    {
        public AggregateFunctionMock(SqlExpressionNode[] arguments, Chain<SqlTraitNode> traits)
            : base( arguments, traits ) { }

        public override SqlAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
        {
            return new AggregateFunctionMock( Arguments.AsSpan().ToArray(), traits );
        }
    }

    private sealed class NodeMock : SqlNodeBase { }
}
