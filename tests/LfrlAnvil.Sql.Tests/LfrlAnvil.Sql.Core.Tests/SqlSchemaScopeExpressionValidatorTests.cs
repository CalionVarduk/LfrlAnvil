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

public class SqlSchemaScopeExpressionValidatorTests : TestsBase
{
    private readonly SqlSchemaBuilderMock _schema;
    private readonly SqlSchemaScopeExpressionValidator _sut;

    public SqlSchemaScopeExpressionValidatorTests()
    {
        _schema = SqlDatabaseBuilderMock.Create().Schemas.Default;
        _sut = new SqlSchemaScopeExpressionValidator( _schema );
    }

    [Fact]
    public void VisitRawExpression_ShouldVisitParameters()
    {
        var node = SqlNode.RawExpression( "foo.a + @a + @b", SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitRawExpression( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRawDataField_ShouldVisitRecordSet_WithoutErrors()
    {
        var node = SqlNode.RawRecordSet( "foo" ).GetField( "a" );
        _sut.VisitRawDataField( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRawDataField_ShouldVisitRecordSet_WithErrors()
    {
        var table = _schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "A" ).Asc() );
        var node = SqlNode.RawDataField( SqlDatabaseMock.Create( _schema.Database ).Schemas.Default.Objects.GetTable( "T" ).Node, "b" );

        _sut.VisitRawDataField( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNull_ShouldDoNothing()
    {
        _sut.VisitNull( SqlNode.Null() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLiteral_ShouldDoNothing()
    {
        _sut.VisitLiteral( ( SqlLiteralNode )SqlNode.Literal( 10 ) );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitParameter_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "b" );
        _sut.VisitParameter( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitColumn_ShouldRegisterError()
    {
        var table = _schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "A" ).Asc() );
        var node = SqlDatabaseMock.Create( _schema.Database ).Schemas.Default.Objects.GetTable( "T" ).Node.GetField( "A" );

        _sut.VisitColumn( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitColumnBuilder_ShouldBeSuccessful_WhenColumnBelongsToTheSameDatabaseAndIsNotRemoved()
    {
        var table = _schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "A" );
        var node = table.Node.GetField( "A" );

        _sut.VisitColumnBuilder( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestSetEqual( [ column, table ] ) )
            .Go();
    }

    [Fact]
    public void VisitColumnBuilder_ShouldBeSuccessful_WhenColumnBelongsToTheSameDatabaseButDifferentSchemaAndIsNotRemoved()
    {
        var schema = _schema.Database.Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "A" );
        var node = table.Node.GetField( "A" );

        _sut.VisitColumnBuilder( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestSetEqual( [ column, table, schema ] ) )
            .Go();
    }

    [Fact]
    public void VisitColumnBuilder_ShouldRegisterError_WhenColumnDoesNotBelongToTheSameDatabase()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        table.Columns.Create( "A" );
        var node = table.Node.GetField( "A" );

        _sut.VisitColumnBuilder( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitColumnBuilder_ShouldRegisterError_WhenColumnBelongsToTheSameDatabaseAndIsRemoved()
    {
        var table = _schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "A" );
        var node = table.Node.GetField( "A" );
        column.Remove();

        _sut.VisitColumnBuilder( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestSetEqual( [ table ] ) )
            .Go();
    }

    [Fact]
    public void VisitQueryDataField_ShouldDoNothing_WhenRecordSetIsQuery()
    {
        var node = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .AsSet( "bar" )
            .GetField( "a" );

        _sut.VisitQueryDataField( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitQueryDataField_ShouldVisitRecordSet_WithErrors()
    {
        var view = _schema.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var node = view.Node.GetField( "a" );
        view.Remove();

        _sut.VisitQueryDataField( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitViewDataField_ShouldRegisterError()
    {
        _schema.Objects.CreateView( "V", SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var node = SqlDatabaseMock.Create( _schema.Database ).Schemas.Default.Objects.GetView( "V" ).Node.GetField( "a" );

        _sut.VisitViewDataField( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNegate_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).Negate();
        _sut.VisitNegate( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitAdd_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) + SqlNode.Parameter( "b" );
        _sut.VisitAdd( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitConcat_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).Concat( SqlNode.Parameter( "b" ) );
        _sut.VisitConcat( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSubtract_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) - SqlNode.Parameter( "b" );
        _sut.VisitSubtract( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitMultiply_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) * SqlNode.Parameter( "b" );
        _sut.VisitMultiply( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDivide_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) / SqlNode.Parameter( "b" );
        _sut.VisitDivide( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitModulo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) % SqlNode.Parameter( "b" );
        _sut.VisitModulo( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBitwiseNot_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseNot();
        _sut.VisitBitwiseNot( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBitwiseAnd_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) & SqlNode.Parameter( "b" );
        _sut.VisitBitwiseAnd( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBitwiseOr_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) | SqlNode.Parameter( "b" );
        _sut.VisitBitwiseOr( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBitwiseXor_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) ^ SqlNode.Parameter( "b" );
        _sut.VisitBitwiseXor( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBitwiseLeftShift_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseLeftShift( SqlNode.Parameter( "b" ) );
        _sut.VisitBitwiseLeftShift( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBitwiseRightShift_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseRightShift( SqlNode.Parameter( "b" ) );
        _sut.VisitBitwiseRightShift( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSwitchCase_ShouldVisitConditionAndExpression()
    {
        var node = SqlNode.SwitchCase( SqlNode.RawCondition( "foo.a < @a", SqlNode.Parameter( "a" ) ), SqlNode.Parameter( "b" ) );
        _sut.VisitSwitchCase( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
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

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 4 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNamedFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ), SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitNamedFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCoalesceFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Coalesce( SqlNode.Parameter( "b" ) );
        _sut.VisitCoalesceFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCurrentDateFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentDateFunction( SqlNode.Functions.CurrentDate() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCurrentTimeFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentTimeFunction( SqlNode.Functions.CurrentTime() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCurrentDateTimeFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentDateTimeFunction( SqlNode.Functions.CurrentDateTime() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCurrentUtcDateTimeFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentUtcDateTimeFunction( SqlNode.Functions.CurrentUtcDateTime() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCurrentTimestampFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentTimestampFunction( SqlNode.Functions.CurrentTimestamp() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitExtractDateFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractDate();
        _sut.VisitExtractDateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitExtractTimeOfDayFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractTimeOfDay();
        _sut.VisitExtractTimeOfDayFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitExtractDayFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractDayOfYear();
        _sut.VisitExtractDayFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitExtractTemporalUnitFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ExtractTemporalUnit( SqlTemporalUnit.Year );
        _sut.VisitExtractTemporalUnitFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTemporalAddFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TemporalAdd( SqlNode.Parameter( "b" ), SqlTemporalUnit.Year );
        _sut.VisitTemporalAddFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTemporalDiffFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TemporalDiff( SqlNode.Parameter( "b" ), SqlTemporalUnit.Year );
        _sut.VisitTemporalDiffFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNewGuidFunction_ShouldDoNothing()
    {
        _sut.VisitNewGuidFunction( SqlNode.Functions.NewGuid() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLengthFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Length();
        _sut.VisitLengthFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitByteLengthFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ByteLength();
        _sut.VisitByteLengthFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitToLowerFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ToLower();
        _sut.VisitToLowerFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitToUpperFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ToUpper();
        _sut.VisitToUpperFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTrimStartFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TrimStart( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimStartFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTrimEndFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TrimEnd( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimEndFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTrimFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Trim( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSubstringFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Substring( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitSubstringFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitReplaceFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Replace( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitReplaceFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitReverseFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Reverse();
        _sut.VisitReverseFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitIndexOfFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).IndexOf( SqlNode.Parameter( "b" ) );
        _sut.VisitIndexOfFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLastIndexOfFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).LastIndexOf( SqlNode.Parameter( "b" ) );
        _sut.VisitLastIndexOfFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSignFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Sign();
        _sut.VisitSignFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitAbsFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Abs();
        _sut.VisitAbsFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCeilingFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Ceiling();
        _sut.VisitCeilingFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitFloorFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Floor();
        _sut.VisitFloorFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTruncateFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Truncate( SqlNode.Parameter( "b" ) );
        _sut.VisitTruncateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRoundFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Round( SqlNode.Parameter( "b" ) );
        _sut.VisitRoundFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitPowerFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Power( SqlNode.Parameter( "b" ) );
        _sut.VisitPowerFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSquareRootFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).SquareRoot();
        _sut.VisitSquareRootFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitMinFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Min( SqlNode.Parameter( "b" ) );
        _sut.VisitMinFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitMaxFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Max( SqlNode.Parameter( "b" ) );
        _sut.VisitMaxFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCustomFunction_ShouldVisitArguments()
    {
        var node = new FunctionMock( SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitCustomFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNamedAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.AggregateFunctions
            .Named( SqlSchemaObjectName.Create( "foo" ), SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitNamedAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitMinAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Min().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitMinAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitMaxAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Max().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitMaxAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitAverageAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Average().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitAverageAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSumAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Sum().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitSumAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCountAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Count().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitCountAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitStringConcatAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" )
            .StringConcat( SqlNode.Parameter( "b" ) )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitStringConcatAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRowNumberWindowFunction_ShouldVisitTraits()
    {
        var node = SqlNode.WindowFunctions.RowNumber().AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );
        _sut.VisitRowNumberWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRankWindowFunction_ShouldVisitTraits()
    {
        var node = SqlNode.WindowFunctions.Rank().AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );
        _sut.VisitRankWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDenseRankWindowFunction_ShouldVisitTraits()
    {
        var node = SqlNode.WindowFunctions.DenseRank().AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );
        _sut.VisitDenseRankWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCumulativeDistributionWindowFunction_ShouldVisitTraits()
    {
        var node = SqlNode.WindowFunctions.CumulativeDistribution()
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitCumulativeDistributionWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNTileWindowFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).NTile().AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );
        _sut.VisitNTileWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLagWindowFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" )
            .Lag( SqlNode.Parameter( "b" ), SqlNode.Parameter( "d" ) )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitLagWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 4 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLeadWindowFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" )
            .Lead( SqlNode.Parameter( "b" ), SqlNode.Parameter( "d" ) )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitLeadWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 4 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitFirstValueWindowFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).FirstValue().AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );
        _sut.VisitFirstValueWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLastValueWindowFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).LastValue().AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );
        _sut.VisitLastValueWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNthValueWindowFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" )
            .NthValue( SqlNode.Parameter( "b" ) )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitNthValueWindowFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCustomAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = new AggregateFunctionMock(
                new SqlExpressionNode[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) },
                Chain<SqlTraitNode>.Empty )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitCustomAggregateFunction( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRawCondition_ShouldVisitParameters()
    {
        var node = SqlNode.RawCondition( "@a > @b", SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitRawCondition( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTrue_ShouldDoNothing()
    {
        _sut.VisitTrue( SqlNode.True() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitFalse_ShouldDoNothing()
    {
        _sut.VisitFalse( SqlNode.False() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitEqualTo( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNotEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsNotEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitNotEqualTo( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitGreaterThan_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsGreaterThan( SqlNode.Parameter( "b" ) );
        _sut.VisitGreaterThan( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLessThan_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsLessThan( SqlNode.Parameter( "b" ) );
        _sut.VisitLessThan( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitGreaterThanOrEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsGreaterThanOrEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitGreaterThanOrEqualTo( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLessThanOrEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsLessThanOrEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitLessThanOrEqualTo( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitAnd_ShouldVisitOperands()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) )
            .And( SqlNode.RawCondition( "foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitAnd( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitOr_ShouldVisitOperands()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) )
            .Or( SqlNode.RawCondition( "foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitOr( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitConditionValue_ShouldVisitCondition()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ).ToValue();
        _sut.VisitConditionValue( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBetween_ShouldVisitValueAndMinAndMax()
    {
        var node = SqlNode.Parameter( "a" ).IsBetween( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitBetween( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitExists_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).Exists();
        _sut.VisitExists( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLike_ShouldVisitValueAndPatternAndEscape()
    {
        var node = SqlNode.Parameter( "a" ).Like( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitLike( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitIn_ShouldVisitValueAndExpressions()
    {
        var node = ( SqlInConditionNode )SqlNode.Parameter( "a" ).In( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitIn( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitInQuery_ShouldVisitValueAndQuery()
    {
        var node = SqlNode.Parameter( "a" )
            .InQuery( SqlNode.RawQuery( "SELECT foo.b FROM foo WHERE foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitInQuery( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRawRecordSet_ShouldDoNothing()
    {
        var node = SqlNode.RawRecordSet( "foo" );
        _sut.VisitRawRecordSet( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNamedFunctionRecordSet_ShouldVisitFunction()
    {
        var node = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ), SqlNode.Parameter( "a" ) ).AsSet( "bar" );
        _sut.VisitNamedFunctionRecordSet( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTable_ShouldRegisterError()
    {
        var table = _schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var node = SqlDatabaseMock.Create( _schema.Database ).Schemas.Default.Objects.GetTable( "T" ).Node;

        _sut.VisitTable( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTableBuilder_ShouldBeSuccessful_WhenTableBelongsToTheSameDatabaseAndIsNotRemoved()
    {
        var table = _schema.Objects.CreateTable( "T" );
        var node = table.Node;

        _sut.VisitTableBuilder( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestSetEqual( [ table ] ) )
            .Go();
    }

    [Fact]
    public void VisitTableBuilder_ShouldBeSuccessful_WhenTableBelongsToTheSameDatabaseButDifferentSchemaAndIsNotRemoved()
    {
        var schema = _schema.Database.Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var node = table.Node;

        _sut.VisitTableBuilder( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestSetEqual( [ table, schema ] ) )
            .Go();
    }

    [Fact]
    public void VisitTableBuilder_ShouldRegisterError_WhenTableDoesNotBelongToTheSameDatabase()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var node = table.Node;

        _sut.VisitTableBuilder( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTableBuilder_ShouldRegisterError_WhenTableBelongsToTheSameDatabaseAndIsRemoved()
    {
        var table = _schema.Objects.CreateTable( "T" );
        var node = table.Node;
        table.Remove();

        _sut.VisitTableBuilder( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitView_ShouldRegisterError()
    {
        _schema.Objects.CreateView( "V", SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.GetAll() } ) );
        var node = SqlDatabaseMock.Create( _schema.Database ).Schemas.Default.Objects.GetView( "V" ).Node;

        _sut.VisitView( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitViewBuilder_ShouldBeSuccessful_WhenViewBelongsToTheSameDatabaseAndIsNotRemoved()
    {
        var view = _schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.Node;

        _sut.VisitViewBuilder( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestSetEqual( [ view ] ) )
            .Go();
    }

    [Fact]
    public void VisitViewBuilder_ShouldBeSuccessful_WhenViewBelongsToTheSameDatabaseButDifferentSchemaAndIsNotRemoved()
    {
        var schema = _schema.Database.Schemas.Create( "foo" );
        var view = schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.Node;

        _sut.VisitViewBuilder( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestSetEqual( [ view, schema ] ) )
            .Go();
    }

    [Fact]
    public void VisitViewBuilder_ShouldRegisterError_WhenViewDoesNotBelongToTheSameDatabase()
    {
        var view = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.Node;

        _sut.VisitViewBuilder( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitViewBuilder_ShouldRegisterError_WhenViewBelongsToTheSameDatabaseAndIsRemoved()
    {
        var view = _schema.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.Node;
        view.Remove();

        _sut.VisitViewBuilder( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitQueryRecordSet_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).AsSet( "bar" );
        _sut.VisitQueryRecordSet( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCommonTableExpressionRecordSet_ShouldDoNothing()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet;
        _sut.VisitCommonTableExpressionRecordSet( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNewTable_ShouldRegisterError()
    {
        var node = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), new[] { SqlNode.Column<int>( "a" ) } ).AsSet( "bar" );
        _sut.VisitNewTable( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitNewView_ShouldRegisterError()
    {
        var node = SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM qux" ) ).AsSet( "bar" );
        _sut.VisitNewView( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitJoinOn_ShouldVisitInnerRecordSetAndOnExpression()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) )
            .AsSet( "bar" )
            .LeftOn( SqlNode.RawCondition( "qux.x = @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitJoinOn( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDataSource_ShouldVisitFromAndJoinsAndTraits_WhenDataSourceIsNotDummy()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo1 WHERE foo1.a > @a", SqlNode.Parameter( "a" ) )
            .AsSet( "bar1" )
            .Join(
                SqlNode.RawQuery( "SELECT * FROM foo2 WHERE foo2.a > @b", SqlNode.Parameter( "b" ) )
                    .AsSet( "bar2" )
                    .InnerOn( SqlNode.RawCondition( "bar1.x IN (@c, bar2.x)", SqlNode.Parameter( "c" ) ) ) )
            .AndWhere( SqlNode.RawCondition( "bar2.y < @d", SqlNode.Parameter( "d" ) ) );

        _sut.VisitDataSource( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 4 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDataSource_ShouldVisitTraits_WhenDataSourceIsDummy()
    {
        var node = SqlNode.DummyDataSource().AndWhere( SqlNode.RawCondition( "@a > 10", SqlNode.Parameter( "a" ) ) );
        _sut.VisitDataSource( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSelectField_ShouldVisitExpression()
    {
        var node = SqlNode.RawExpression( "foo.a + @a", SqlNode.Parameter( "a" ) ).As( "bar" );
        _sut.VisitSelectField( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSelectCompoundField_ShouldDoNothing()
    {
        var node = ( SqlSelectCompoundFieldNode )SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .CompoundWith( SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ).ToUnionAll() )
            .Selection[0];

        _sut.VisitSelectCompoundField( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSelectRecordSet_ShouldDoNothing()
    {
        var node = SqlNode.RawRecordSet( "foo" ).GetAll();
        _sut.VisitSelectRecordSet( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSelectAll_ShouldDoNothing()
    {
        var node = SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll();
        _sut.VisitSelectAll( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSelectExpression_ShouldVisitSelection()
    {
        var node = SqlNode.RawExpression( "foo.a + @a", SqlNode.Parameter( "a" ) ).As( "bar" ).ToExpression();
        _sut.VisitSelectExpression( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRawQuery_ShouldVisitParameters()
    {
        var node = SqlNode.RawQuery(
            "SELECT * FROM foo WHERE foo.a > @a AND foo.b > @b",
            SqlNode.Parameter( "a" ),
            SqlNode.Parameter( "b" ) );

        _sut.VisitRawQuery( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDataSourceQuery_ShouldVisitSelectionsAndDataSourceAndTraits()
    {
        var node = SqlNode.RawRecordSet( "foo" )
            .Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.RawCondition( "bar.x IN (@a, foo.x)", SqlNode.Parameter( "a" ) ) ) )
            .Select( s => new[] { s["foo"]["a"].AsSelf(), (s["foo"]["b"] + s["bar"]["b"] + SqlNode.Parameter( "b" )).As( "b" ) } )
            .AndWhere( SqlNode.RawCondition( "bar.y > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitDataSourceQuery( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCompoundQuery_ShouldVisitSelectionsAndFirstQueryAndFollowingQueriesAndTraits()
    {
        var node = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .AndWhere( SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ) )
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .CompoundWith( SqlNode.RawQuery( "SELECT bar.b AS a FROM bar WHERE bar.b > @b", SqlNode.Parameter( "b" ) ).ToUnionAll() )
            .OrderBy( SqlNode.RawExpression( "foo.a + @c", SqlNode.Parameter( "c" ) ).Asc() );

        _sut.VisitCompoundQuery( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 3 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCompoundQueryComponent_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).ToUnionAll();
        _sut.VisitCompoundQueryComponent( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDistinctTrait_ShouldDoNothing()
    {
        _sut.VisitDistinctTrait( SqlNode.DistinctTrait() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitFilterTrait_ShouldVisitFilter()
    {
        var node = SqlNode.FilterTrait( SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ), isConjunction: true );
        _sut.VisitFilterTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitAggregationTrait_ShouldVisitExpressions()
    {
        var node = SqlNode.AggregationTrait( SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitAggregationTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitAggregationFilterTrait_ShouldVisitFilter()
    {
        var node = SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ), isConjunction: true );
        _sut.VisitAggregationFilterTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitSortTrait_ShouldVisitOrderings()
    {
        var node = SqlNode.SortTrait( SqlNode.Parameter( "a" ).Asc(), SqlNode.Parameter( "b" ).Desc() );
        _sut.VisitSortTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitLimitTrait_ShouldVisitValue()
    {
        var node = SqlNode.LimitTrait( SqlNode.Parameter( "a" ) );
        _sut.VisitLimitTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitOffsetTrait_ShouldVisitValue()
    {
        var node = SqlNode.OffsetTrait( SqlNode.Parameter( "a" ) );
        _sut.VisitOffsetTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCommonTableExpressionTrait_ShouldVisitCommonTableExpressions()
    {
        var node = SqlNode.CommonTableExpressionTrait(
            SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).ToCte( "x" ),
            SqlNode.RawQuery( "SELECT * FROM bar WHERE bar.b > @b", SqlNode.Parameter( "b" ) ).ToCte( "y" ) );

        _sut.VisitCommonTableExpressionTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitWindowDefinitionTrait_ShouldVisitWindows()
    {
        var node = SqlNode.WindowDefinitionTrait(
            SqlNode.WindowDefinition( "foo", new[] { SqlNode.Parameter( "a" ).Asc() } ),
            SqlNode.WindowDefinition( "bar", new[] { SqlNode.Parameter( "b" ).Asc() } ) );

        _sut.VisitWindowDefinitionTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitWindowTrait_ShouldVisitDefinition()
    {
        var node = SqlNode.WindowTrait( SqlNode.WindowDefinition( "foo", new[] { SqlNode.Parameter( "a" ).Asc() } ) );
        _sut.VisitWindowTrait( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitOrderBy_ShouldVisitExpression()
    {
        var node = SqlNode.Parameter( "a" ).Asc();
        _sut.VisitOrderBy( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCommonTableExpression_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).ToCte( "bar" );
        _sut.VisitCommonTableExpression( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitWindowDefinition_ShouldVisitPartitioningAndOrderingAndFrame()
    {
        var node = SqlNode.WindowDefinition(
            "foo",
            new SqlExpressionNode[] { SqlNode.Parameter( "a" ) },
            new[] { SqlNode.Parameter( "b" ).Asc() },
            SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.CurrentRow ) );

        _sut.VisitWindowDefinition( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 2 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitWindowFrame_ShouldDoNothing()
    {
        var node = SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.CurrentRow );
        _sut.VisitWindowFrame( node );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTypeCast_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).CastTo<int>();
        _sut.VisitTypeCast( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitValues_ShouldRegisterError()
    {
        var node = SqlNode.Values( SqlNode.Literal( 10 ) );
        _sut.VisitValues( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRawStatement_ShouldRegisterError()
    {
        var node = SqlNode.RawStatement( "INSERT INTO foo (a, b) VALUES (1, 2)" );
        _sut.VisitRawStatement( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitInsertInto_ShouldRegisterError()
    {
        var node = SqlNode.InsertInto(
            SqlNode.RawQuery( "SELECT a FROM foo" ),
            SqlNode.RawRecordSet( "bar" ),
            SqlNode.RawRecordSet( "bar" ).GetField( "a" ) );

        _sut.VisitInsertInto( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitUpdate_ShouldRegisterError()
    {
        var node = SqlNode.Update(
            SqlNode.RawRecordSet( "foo" ).ToDataSource(),
            SqlNode.ValueAssignment( SqlNode.RawRecordSet( "foo" ).GetField( "a" ), SqlNode.Literal( 10 ) ) );

        _sut.VisitUpdate( node );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
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

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitValueAssignment_ShouldRegisterError()
    {
        _sut.VisitValueAssignment( SqlNode.ValueAssignment( SqlNode.RawRecordSet( "foo" ).GetField( "a" ), SqlNode.Literal( 10 ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDeleteFrom_ShouldRegisterError()
    {
        _sut.VisitDeleteFrom( SqlNode.DeleteFrom( SqlNode.RawRecordSet( "foo" ).ToDataSource() ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitTruncate_ShouldRegisterError()
    {
        _sut.VisitTruncate( SqlNode.Truncate( SqlNode.RawRecordSet( "foo" ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitColumnDefinition_ShouldRegisterError()
    {
        _sut.VisitColumnDefinition( SqlNode.Column<int>( "a" ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitPrimaryKeyDefinition_ShouldRegisterError()
    {
        _sut.VisitPrimaryKeyDefinition( SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK" ), ReadOnlyArray<SqlOrderByNode>.Empty ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
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

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCheckDefinition_ShouldRegisterError()
    {
        _sut.VisitCheckDefinition( SqlNode.Check( SqlSchemaObjectName.Create( "CHK" ), SqlNode.True() ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCreateTable_ShouldRegisterError()
    {
        _sut.VisitCreateTable( SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), new[] { SqlNode.Column<int>( "a" ) } ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCreateView_ShouldRegisterError()
    {
        _sut.VisitCreateView( SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM bar" ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
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

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRenameTable_ShouldRegisterError()
    {
        _sut.VisitRenameTable( SqlNode.RenameTable( SqlRecordSetInfo.Create( "foo" ), SqlSchemaObjectName.Create( "bar" ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRenameColumn_ShouldRegisterError()
    {
        _sut.VisitRenameColumn( SqlNode.RenameColumn( SqlRecordSetInfo.Create( "foo" ), "bar", "qux" ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitAddColumn_ShouldRegisterError()
    {
        _sut.VisitAddColumn( SqlNode.AddColumn( SqlRecordSetInfo.Create( "foo" ), SqlNode.Column<int>( "a" ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDropColumn_ShouldRegisterError()
    {
        _sut.VisitDropColumn( SqlNode.DropColumn( SqlRecordSetInfo.Create( "foo" ), "bar" ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDropTable_ShouldRegisterError()
    {
        _sut.VisitDropTable( SqlNode.DropTable( SqlRecordSetInfo.Create( "foo" ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDropView_ShouldRegisterError()
    {
        _sut.VisitDropView( SqlNode.DropView( SqlRecordSetInfo.Create( "foo" ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitDropIndex_ShouldRegisterError()
    {
        _sut.VisitDropIndex( SqlNode.DropIndex( SqlRecordSetInfo.Create( "bar" ), SqlSchemaObjectName.Create( "foo" ) ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitStatementBatch_ShouldRegisterError()
    {
        _sut.VisitStatementBatch( SqlNode.Batch() );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitBeginTransaction_ShouldRegisterError()
    {
        _sut.VisitBeginTransaction( SqlNode.BeginTransaction( IsolationLevel.Serializable ) );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCommitTransaction_ShouldRegisterError()
    {
        _sut.VisitCommitTransaction( SqlNode.CommitTransaction() );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitRollbackTransaction_ShouldRegisterError()
    {
        _sut.VisitRollbackTransaction( SqlNode.RollbackTransaction() );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitExpressionPlaceholder_ShouldRegisterError()
    {
        _sut.VisitExpressionPlaceholder( SqlNode.Placeholders.Expression() );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitConditionPlaceholder_ShouldRegisterError()
    {
        _sut.VisitConditionPlaceholder( SqlNode.Placeholders.Condition() );

        Assertion.All(
                _sut.GetErrors().Count.TestEquals( 1 ),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
    }

    [Fact]
    public void VisitCustom_ShouldDoNothing()
    {
        _sut.VisitCustom( new NodeMock() );

        Assertion.All(
                _sut.GetErrors().TestEmpty(),
                _sut.GetReferencedObjects().TestEmpty() )
            .Go();
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
