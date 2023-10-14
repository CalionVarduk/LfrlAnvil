﻿using System.Data;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteViewSourceValidatorTests : TestsBase
{
    private readonly SqliteDatabaseBuilder _db;
    private readonly SqliteViewSourceValidator _sut;

    public SqliteViewSourceValidatorTests()
    {
        _db = SqliteDatabaseBuilderMock.Create();
        _sut = new SqliteViewSourceValidator( _db );
    }

    [Fact]
    public void VisitRawExpression_ShouldVisitParameters()
    {
        var node = SqlNode.RawExpression( "foo.a + @a + @b", SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitRawExpression( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitRawDataField_ShouldVisitRecordSet_WithoutErrors()
    {
        var node = SqlNode.RawRecordSet( "foo" ).GetField( "a" );
        _sut.VisitRawDataField( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitRawDataField_ShouldVisitRecordSet_WithErrors()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "A" ).Asc() );
        var node = SqlNode.RawDataField( new SqliteDatabaseMock( _db ).Schemas.Default.Objects.GetTable( "T" ).ToRecordSet(), "b" );

        _sut.VisitRawDataField( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitNull_ShouldDoNothing()
    {
        _sut.VisitNull( SqlNode.Null() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitLiteral_ShouldDoNothing()
    {
        _sut.VisitLiteral( (SqlLiteralNode)SqlNode.Literal( 10 ) );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitParameter_ShouldRegisterError()
    {
        var node = SqlNode.Parameter( "b" );
        _sut.VisitParameter( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitColumn_ShouldRegisterError()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "A" ).Asc() );
        var node = new SqliteDatabaseMock( _db ).Schemas.Default.Objects.GetTable( "T" ).ToRecordSet().GetField( "A" );

        _sut.VisitColumn( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitColumnBuilder_ShouldBeSuccessful_WhenColumnBelongsToTheSameDatabaseAndIsNotRemoved()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "A" );
        var node = table.ToRecordSet().GetField( "A" );

        _sut.VisitColumnBuilder( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Values.Should().BeEquivalentTo( column, table );
        }
    }

    [Fact]
    public void VisitColumnBuilder_ShouldRegisterError_WhenColumnDoesNotBelongToTheSameDatabase()
    {
        var table = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        table.Columns.Create( "A" );
        var node = table.ToRecordSet().GetField( "A" );

        _sut.VisitColumnBuilder( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitColumnBuilder_ShouldRegisterError_WhenColumnBelongsToTheSameDatabaseAndIsRemoved()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "A" );
        var node = table.ToRecordSet().GetField( "A" );
        column.Remove();

        _sut.VisitColumnBuilder( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEquivalentTo( table );
        }
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

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitQueryDataField_ShouldVisitRecordSet_WithErrors()
    {
        var view = _db.Schemas.Default.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var node = view.ToRecordSet().GetField( "a" );
        view.Remove();

        _sut.VisitQueryDataField( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitViewDataField_ShouldRegisterError()
    {
        _db.Schemas.Default.Objects.CreateView(
            "V",
            SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var node = new SqliteDatabaseMock( _db ).Schemas.Default.Objects.GetView( "V" ).ToRecordSet().GetField( "a" );

        _sut.VisitViewDataField( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitNegate_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).Negate();
        _sut.VisitNegate( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitAdd_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) + SqlNode.Parameter( "b" );
        _sut.VisitAdd( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitConcat_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).Concat( SqlNode.Parameter( "b" ) );
        _sut.VisitConcat( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSubtract_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) - SqlNode.Parameter( "b" );
        _sut.VisitSubtract( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitMultiply_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) * SqlNode.Parameter( "b" );
        _sut.VisitMultiply( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitDivide_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) / SqlNode.Parameter( "b" );
        _sut.VisitDivide( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitModulo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) % SqlNode.Parameter( "b" );
        _sut.VisitModulo( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBitwiseNot_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseNot();
        _sut.VisitBitwiseNot( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBitwiseAnd_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) & SqlNode.Parameter( "b" );
        _sut.VisitBitwiseAnd( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBitwiseOr_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) | SqlNode.Parameter( "b" );
        _sut.VisitBitwiseOr( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBitwiseXor_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ) ^ SqlNode.Parameter( "b" );
        _sut.VisitBitwiseXor( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBitwiseLeftShift_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseLeftShift( SqlNode.Parameter( "b" ) );
        _sut.VisitBitwiseLeftShift( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBitwiseRightShift_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).BitwiseRightShift( SqlNode.Parameter( "b" ) );
        _sut.VisitBitwiseRightShift( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSwitchCase_ShouldVisitConditionAndExpression()
    {
        var node = SqlNode.SwitchCase( SqlNode.RawCondition( "foo.a < @a", SqlNode.Parameter( "a" ) ), SqlNode.Parameter( "b" ) );
        _sut.VisitSwitchCase( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 4 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitRecordsAffectedFunction_ShouldDoNothing()
    {
        _sut.VisitRecordsAffectedFunction( SqlNode.Functions.RecordsAffected() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCoalesceFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Coalesce( SqlNode.Parameter( "b" ) );
        _sut.VisitCoalesceFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCurrentDateFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentDateFunction( SqlNode.Functions.CurrentDate() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCurrentTimeFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentTimeFunction( SqlNode.Functions.CurrentTime() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCurrentDateTimeFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentDateTimeFunction( SqlNode.Functions.CurrentDateTime() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCurrentTimestampFunction_ShouldDoNothing()
    {
        _sut.VisitCurrentTimestampFunction( SqlNode.Functions.CurrentTimestamp() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitNewGuidFunction_ShouldDoNothing()
    {
        _sut.VisitNewGuidFunction( SqlNode.Functions.NewGuid() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitLengthFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Length();
        _sut.VisitLengthFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitToLowerFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ToLower();
        _sut.VisitToLowerFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitToUpperFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).ToUpper();
        _sut.VisitToUpperFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTrimStartFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TrimStart( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimStartFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTrimEndFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).TrimEnd( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimEndFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTrimFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Trim( SqlNode.Parameter( "b" ) );
        _sut.VisitTrimFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSubstringFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Substring( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitSubstringFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitReplaceFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Replace( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitReplaceFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitIndexOfFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).IndexOf( SqlNode.Parameter( "b" ) );
        _sut.VisitIndexOfFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitLastIndexOfFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).LastIndexOf( SqlNode.Parameter( "b" ) );
        _sut.VisitLastIndexOfFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSignFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Sign();
        _sut.VisitSignFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitAbsFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Abs();
        _sut.VisitAbsFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCeilingFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Ceiling();
        _sut.VisitCeilingFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitFloorFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Floor();
        _sut.VisitFloorFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTruncateFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Truncate();
        _sut.VisitTruncateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitPowerFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Power( SqlNode.Parameter( "b" ) );
        _sut.VisitPowerFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSquareRootFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).SquareRoot();
        _sut.VisitSquareRootFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitMinFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Min( SqlNode.Parameter( "b" ) );
        _sut.VisitMinFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitMaxFunction_ShouldVisitArguments()
    {
        var node = SqlNode.Parameter( "a" ).Max( SqlNode.Parameter( "b" ) );
        _sut.VisitMaxFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCustomFunction_ShouldVisitArguments()
    {
        var node = new FunctionMock( SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitCustomFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitMinAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Min().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitMinAggregateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitMaxAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Max().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitMaxAggregateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitAverageAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Average().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitAverageAggregateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSumAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Sum().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitSumAggregateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCountAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" ).Count().AndWhere( SqlNode.RawCondition( "foo.a > @b", SqlNode.Parameter( "b" ) ) );
        _sut.VisitCountAggregateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitStringConcatAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = SqlNode.Parameter( "a" )
            .StringConcat( SqlNode.Parameter( "b" ) )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitStringConcatAggregateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCustomAggregateFunction_ShouldVisitArgumentsAndTraits()
    {
        var node = new AggregateFunctionMock(
                new SqlExpressionNode[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) },
                Chain<SqlTraitNode>.Empty )
            .AndWhere( SqlNode.RawCondition( "foo.a > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitCustomAggregateFunction( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitRawCondition_ShouldVisitParameters()
    {
        var node = SqlNode.RawCondition( "@a > @b", SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitRawCondition( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTrue_ShouldDoNothing()
    {
        _sut.VisitTrue( SqlNode.True() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitFalse_ShouldDoNothing()
    {
        _sut.VisitFalse( SqlNode.False() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitEqualTo( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitNotEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsNotEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitNotEqualTo( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitGreaterThan_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsGreaterThan( SqlNode.Parameter( "b" ) );
        _sut.VisitGreaterThan( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitLessThan_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsLessThan( SqlNode.Parameter( "b" ) );
        _sut.VisitLessThan( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitGreaterThanOrEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsGreaterThanOrEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitGreaterThanOrEqualTo( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitLessThanOrEqualTo_ShouldVisitOperands()
    {
        var node = SqlNode.Parameter( "a" ).IsLessThanOrEqualTo( SqlNode.Parameter( "b" ) );
        _sut.VisitLessThanOrEqualTo( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitAnd_ShouldVisitOperands()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) )
            .And( SqlNode.RawCondition( "foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitAnd( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitOr_ShouldVisitOperands()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) )
            .Or( SqlNode.RawCondition( "foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitOr( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitConditionValue_ShouldVisitCondition()
    {
        var node = SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ).ToValue();
        _sut.VisitConditionValue( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBetween_ShouldVisitValueAndMinAndMax()
    {
        var node = SqlNode.Parameter( "a" ).IsBetween( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitBetween( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitExists_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).Exists();
        _sut.VisitExists( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitLike_ShouldVisitValueAndPatternAndEscape()
    {
        var node = SqlNode.Parameter( "a" ).Like( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitLike( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitIn_ShouldVisitValueAndExpressions()
    {
        var node = (SqlInConditionNode)SqlNode.Parameter( "a" ).In( SqlNode.Parameter( "b" ), SqlNode.Parameter( "c" ) );
        _sut.VisitIn( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitInQuery_ShouldVisitValueAndQuery()
    {
        var node = SqlNode.Parameter( "a" )
            .InQuery( SqlNode.RawQuery( "SELECT foo.b FROM foo WHERE foo.b > @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitInQuery( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitRawRecordSet_ShouldDoNothing()
    {
        var node = SqlNode.RawRecordSet( "foo" );
        _sut.VisitRawRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTableRecordSet_ShouldRegisterError()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        table.SetPrimaryKey( table.Columns.Create( "C" ).Asc() );
        var node = new SqliteDatabaseMock( _db ).Schemas.Default.Objects.GetTable( "T" ).ToRecordSet();

        _sut.VisitTableRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTableBuilderRecordSet_ShouldBeSuccessful_WhenTableBelongsToTheSameDatabaseAndIsNotRemoved()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        var node = table.ToRecordSet();

        _sut.VisitTableBuilderRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Values.Should().BeEquivalentTo( table );
        }
    }

    [Fact]
    public void VisitTableBuilderRecordSet_ShouldRegisterError_WhenTableDoesNotBelongToTheSameDatabase()
    {
        var table = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var node = table.ToRecordSet();

        _sut.VisitTableBuilderRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTableBuilderRecordSet_ShouldRegisterError_WhenTableBelongsToTheSameDatabaseAndIsRemoved()
    {
        var table = _db.Schemas.Default.Objects.CreateTable( "T" );
        var node = table.ToRecordSet();
        table.Remove();

        _sut.VisitTableBuilderRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitViewRecordSet_ShouldRegisterError()
    {
        _db.Schemas.Default.Objects.CreateView( "V", SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.GetAll() } ) );
        var node = new SqliteDatabaseMock( _db ).Schemas.Default.Objects.GetView( "V" ).ToRecordSet();

        _sut.VisitViewRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitViewBuilderRecordSet_ShouldBeSuccessful_WhenViewBelongsToTheSameDatabaseAndIsNotRemoved()
    {
        var view = _db.Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.ToRecordSet();

        _sut.VisitViewBuilderRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Values.Should().BeEquivalentTo( view );
        }
    }

    [Fact]
    public void VisitViewBuilderRecordSet_ShouldRegisterError_WhenViewDoesNotBelongToTheSameDatabase()
    {
        var view = SqliteDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.ToRecordSet();

        _sut.VisitViewBuilderRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitViewBuilderRecordSet_ShouldRegisterError_WhenViewBelongsToTheSameDatabaseAndIsRemoved()
    {
        var view = _db.Schemas.Default.Objects.CreateView( "V", SqlNode.RawQuery( "SELECT * FROM foo" ) );
        var node = view.ToRecordSet();
        view.Remove();

        _sut.VisitViewBuilderRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitQueryRecordSet_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).AsSet( "bar" );
        _sut.VisitQueryRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCommonTableExpressionRecordSet_ShouldDoNothing()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet;
        _sut.VisitCommonTableExpressionRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTemporaryTableRecordSet_ShouldRegisterError()
    {
        var node = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "a" ) ).AsSet( "bar" );
        _sut.VisitTemporaryTableRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitJoinOn_ShouldVisitInnerRecordSetAndOnExpression()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) )
            .AsSet( "bar" )
            .LeftOn( SqlNode.RawCondition( "qux.x = @b", SqlNode.Parameter( "b" ) ) );

        _sut.VisitJoinOn( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 4 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitDataSource_ShouldVisitTraits_WhenDataSourceIsDummy()
    {
        var node = SqlNode.DummyDataSource().AndWhere( SqlNode.RawCondition( "@a > 10", SqlNode.Parameter( "a" ) ) );
        _sut.VisitDataSource( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSelectField_ShouldVisitExpression()
    {
        var node = SqlNode.RawExpression( "foo.a + @a", SqlNode.Parameter( "a" ) ).As( "bar" );
        _sut.VisitSelectField( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSelectCompoundField_ShouldDoNothing()
    {
        var node = (SqlSelectCompoundFieldNode)SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .CompoundWith( SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ).ToUnionAll() )
            .Selection.Span[0];

        _sut.VisitSelectCompoundField( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSelectRecordSet_ShouldDoNothing()
    {
        var node = SqlNode.RawRecordSet( "foo" ).GetAll();
        _sut.VisitSelectRecordSet( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSelectAll_ShouldDoNothing()
    {
        var node = SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll();
        _sut.VisitSelectAll( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSelectExpression_ShouldVisitSelection()
    {
        var node = SqlNode.RawExpression( "foo.a + @a", SqlNode.Parameter( "a" ) ).As( "bar" ).ToExpression();
        _sut.VisitSelectExpression( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Values.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitRawQuery_ShouldVisitParameters()
    {
        var node = SqlNode.RawQuery(
            "SELECT * FROM foo WHERE foo.a > @a AND foo.b > @b",
            SqlNode.Parameter( "a" ),
            SqlNode.Parameter( "b" ) );

        _sut.VisitRawQuery( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitDataSourceQuery_ShouldVisitSelectionsAndDataSourceAndTraits()
    {
        var node = SqlNode.RawRecordSet( "foo" )
            .Join( SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.RawCondition( "bar.x IN (@a, foo.x)", SqlNode.Parameter( "a" ) ) ) )
            .Select( s => new[] { s["foo"]["a"].AsSelf(), (s["foo"]["b"] + s["bar"]["b"] + SqlNode.Parameter( "b" )).As( "b" ) } )
            .AndWhere( SqlNode.RawCondition( "bar.y > @c", SqlNode.Parameter( "c" ) ) );

        _sut.VisitDataSourceQuery( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
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

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 3 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCompoundQueryComponent_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).ToUnionAll();
        _sut.VisitCompoundQueryComponent( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitDistinctTrait_ShouldDoNothing()
    {
        _sut.VisitDistinctTrait( SqlNode.DistinctTrait() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitFilterTrait_ShouldVisitFilter()
    {
        var node = SqlNode.FilterTrait( SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ), isConjunction: true );
        _sut.VisitFilterTrait( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitAggregationTrait_ShouldVisitExpressions()
    {
        var node = SqlNode.AggregationTrait( SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) );
        _sut.VisitAggregationTrait( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitAggregationFilterTrait_ShouldVisitFilter()
    {
        var node = SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter( "a" ) ), isConjunction: true );
        _sut.VisitAggregationFilterTrait( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitSortTrait_ShouldVisitOrderings()
    {
        var node = SqlNode.SortTrait( SqlNode.Parameter( "a" ).Asc(), SqlNode.Parameter( "b" ).Desc() );
        _sut.VisitSortTrait( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitLimitTrait_ShouldVisitValue()
    {
        var node = SqlNode.LimitTrait( SqlNode.Parameter( "a" ) );
        _sut.VisitLimitTrait( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitOffsetTrait_ShouldVisitValue()
    {
        var node = SqlNode.OffsetTrait( SqlNode.Parameter( "a" ) );
        _sut.VisitOffsetTrait( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCommonTableExpressionTrait_ShouldVisitCommonTableExpressions()
    {
        var node = SqlNode.CommonTableExpressionTrait(
            SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).ToCte( "x" ),
            SqlNode.RawQuery( "SELECT * FROM bar WHERE bar.b > @b", SqlNode.Parameter( "b" ) ).ToCte( "y" ) );

        _sut.VisitCommonTableExpressionTrait( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 2 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitOrderBy_ShouldVisitExpression()
    {
        var node = SqlNode.Parameter( "a" ).Asc();
        _sut.VisitOrderBy( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCommonTableExpression_ShouldVisitQuery()
    {
        var node = SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a > @a", SqlNode.Parameter( "a" ) ).ToCte( "bar" );
        _sut.VisitCommonTableExpression( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitTypeCast_ShouldVisitValue()
    {
        var node = SqlNode.Parameter( "a" ).CastTo<int>();
        _sut.VisitTypeCast( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitValues_ShouldRegisterError()
    {
        var node = SqlNode.Values( SqlNode.Literal( 10 ) );
        _sut.VisitValues( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitInsertInto_ShouldRegisterError()
    {
        var node = SqlNode.InsertInto(
            SqlNode.RawQuery( "SELECT a FROM foo" ),
            SqlNode.RawRecordSet( "bar" ),
            SqlNode.RawRecordSet( "bar" ).GetField( "a" ) );

        _sut.VisitInsertInto( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitUpdate_ShouldRegisterError()
    {
        var node = SqlNode.Update(
            SqlNode.RawRecordSet( "foo" ).ToDataSource(),
            SqlNode.ValueAssignment( SqlNode.RawRecordSet( "foo" ).GetField( "a" ), SqlNode.Literal( 10 ) ) );

        _sut.VisitUpdate( node );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitValueAssignment_ShouldRegisterError()
    {
        _sut.VisitValueAssignment( SqlNode.ValueAssignment( SqlNode.RawRecordSet( "foo" ).GetField( "a" ), SqlNode.Literal( 10 ) ) );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitDeleteFrom_ShouldRegisterError()
    {
        _sut.VisitDeleteFrom( SqlNode.DeleteFrom( SqlNode.RawRecordSet( "foo" ).ToDataSource() ) );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitColumnDefinition_ShouldRegisterError()
    {
        _sut.VisitColumnDefinition( SqlNode.ColumnDefinition<int>( "a" ) );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCreateTemporaryTable_ShouldRegisterError()
    {
        _sut.VisitCreateTemporaryTable( SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitDropTemporaryTable_ShouldRegisterError()
    {
        _sut.VisitDropTemporaryTable( SqlNode.DropTempTable( "foo" ) );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitStatementBatch_ShouldRegisterError()
    {
        _sut.VisitStatementBatch( SqlNode.Batch() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitBeginTransaction_ShouldRegisterError()
    {
        _sut.VisitBeginTransaction( SqlNode.BeginTransaction( IsolationLevel.Serializable ) );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCommitTransaction_ShouldRegisterError()
    {
        _sut.VisitCommitTransaction( SqlNode.CommitTransaction() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitRollbackTransaction_ShouldRegisterError()
    {
        _sut.VisitRollbackTransaction( SqlNode.RollbackTransaction() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().HaveCount( 1 );
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void VisitCustom_ShouldDoNothing()
    {
        _sut.VisitCustom( new NodeMock() );

        using ( new AssertionScope() )
        {
            _sut.GetErrors().Should().BeEmpty();
            _sut.ReferencedObjects.Should().BeEmpty();
        }
    }

    private sealed class FunctionMock : SqlFunctionExpressionNode
    {
        public FunctionMock(params SqlExpressionNode[] arguments)
            : base( SqlFunctionType.Custom, arguments ) { }
    }

    private sealed class AggregateFunctionMock : SqlAggregateFunctionExpressionNode
    {
        public AggregateFunctionMock(SqlExpressionNode[] arguments, Chain<SqlTraitNode> traits)
            : base( SqlFunctionType.Custom, arguments, traits ) { }

        public override SqlAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
        {
            return new AggregateFunctionMock( Arguments.ToArray(), Traits.Extend( trait ) );
        }
    }

    private sealed class NodeMock : SqlNodeBase
    {
        public NodeMock()
            : base( SqlNodeType.Unknown ) { }
    }
}
