﻿using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests;

public class MySqlNodeInterpreterTests : TestsBase
{
    private readonly MySqlColumnTypeDefinitionProvider _typeDefinitions =
        new MySqlColumnTypeDefinitionProvider( new MySqlDataTypeProvider() );

    private readonly MySqlNodeInterpreter _sut;

    public MySqlNodeInterpreterTests()
    {
        _sut = new MySqlNodeInterpreter( _typeDefinitions, SqlNodeInterpreterContext.Create() );
    }

    [Fact]
    public void Interpreter_ShouldUseBackticksAsNameDelimiters()
    {
        using ( new AssertionScope() )
        {
            _sut.BeginNameDelimiter.Should().Be( '`' );
            _sut.EndNameDelimiter.Should().Be( '`' );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretRawExpression()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a + @bar", SqlNode.Parameter<int>( "bar" ) ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Sql.ToString().Should().Be( "foo.a + @bar" );
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "bar", (TypeNullability?)TypeNullability.Create<int>() ) );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawExpressionWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a + @bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a + @bar)" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawDataField()
    {
        _sut.Visit( SqlNode.RawDataField( SqlNode.RawRecordSet( "foo" ), "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "foo.`bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawDataFieldWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawDataField( SqlNode.RawRecordSet( "foo" ), "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "foo.`bar`" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`.`qux`" )]
    [InlineData( true, "`foo`.`qux`" )]
    public void Visit_ShouldInterpretRawDataField_FromNewTable(bool isTemporary, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "qux" ) } ).AsSet().GetField( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Visit_ShouldInterpretRawDataField_FromAliasedNewTable(bool isTemporary)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "qux" ) } ).AsSet( "lorem" ).GetField( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( "`lorem`.`qux`" );
    }

    [Fact]
    public void Visit_ShouldInterpretNull()
    {
        _sut.Visit( SqlNode.Null() );
        _sut.Context.Sql.ToString().Should().Be( "NULL" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretNullWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.Null() );
        _sut.Context.Sql.ToString().Should().Be( "NULL" );
    }

    [Fact]
    public void Visit_ShouldInterpretLiteral()
    {
        _sut.Visit( SqlNode.Literal( "fo'o" ) );
        _sut.Context.Sql.ToString().Should().Be( "'fo''o'" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretLiteralWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) );
        _sut.Context.Sql.ToString().Should().Be( "25" );
    }

    [Fact]
    public void Visit_ShouldInterpretParameter()
    {
        _sut.Visit( SqlNode.Parameter<int>( "a" ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Sql.ToString().Should().Be( "@a" );
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "a", (TypeNullability?)TypeNullability.Create<int>() ) );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretParameterWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.Parameter<string>( "b", isNullable: true ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Sql.ToString().Should().Be( "@b" );
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "b", (TypeNullability?)TypeNullability.Create<string>( isNullable: true ) ) );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretColumn()
    {
        var table = CreateTable( string.Empty, "foo", "bar" ).ToRecordSet();
        _sut.Visit( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretColumnWithoutParentheses()
    {
        var table = SqlNode.Table( CreateTable( string.Empty, "foo", "bar" ) );
        _sut.VisitChild( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnBuilder()
    {
        var table = SqlNode.Table( CreateTableBuilder( string.Empty, "foo", "bar" ) );
        _sut.Visit( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretColumnBuilderWithoutParentheses()
    {
        var table = SqlNode.Table( CreateTableBuilder( string.Empty, "foo", "bar" ) );
        _sut.VisitChild( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretQueryDataField()
    {
        var query = SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["bar"].AsSelf() } ).AsSet( "qux" );
        _sut.Visit( query.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`qux`.`bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretQueryDataFieldWithoutParentheses()
    {
        var query = SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["bar"].AsSelf() } ).AsSet( "qux" );
        _sut.VisitChild( query.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`qux`.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretViewDataField()
    {
        var view = SqlNode.View(
            CreateView(
                string.Empty,
                "foo",
                SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["qux"].AsSelf() } ) ) );

        _sut.Visit( view.GetField( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo`.`qux`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewDataFieldWithoutParentheses()
    {
        var view = SqlNode.View(
            CreateView(
                string.Empty,
                "foo",
                SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["qux"].AsSelf() } ) ) );

        _sut.VisitChild( view.GetField( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo`.`qux`" );
    }

    [Fact]
    public void Visit_ShouldInterpretNegate_WithValueWrappedInParentheses()
    {
        _sut.Visit( SqlNode.Literal( -25 ).Negate() );
        _sut.Context.Sql.ToString().Should().Be( "-(-25)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretNegateWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ).Negate() );
        _sut.Context.Sql.ToString().Should().Be( "(-(25))" );
    }

    [Fact]
    public void Visit_ShouldInterpretAdd_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) + SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) + (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretAdd_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) + SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 + 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretAddWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) + SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 + 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretConcat_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Concat( SqlNode.RawExpression( "foo.b" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) || (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretConcat_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( "foo" ).Concat( SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "'foo' || 'bar'" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretConcatWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( "foo" ).Concat( SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "('foo' || 'bar')" );
    }

    [Fact]
    public void Visit_ShouldInterpretSubtract_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) - SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) - (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSubtract_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) - SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 - 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSubtractWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) - SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 - 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMultiply_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) * SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) * (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMultiply_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) * SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 * 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretMultiplyWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) * SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 * 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDivide_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) / SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) / (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDivide_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) / SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 / 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretDivideWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) / SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 / 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretModulo_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) % SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) % (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretModulo_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) % SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 % 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretModuloWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) % SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 % 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseNot_WithValueWrappedInParentheses()
    {
        _sut.Visit( SqlNode.Literal( -25 ).BitwiseNot() );
        _sut.Context.Sql.ToString().Should().Be( "~(-25)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBitwiseNotWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ).BitwiseNot() );
        _sut.Context.Sql.ToString().Should().Be( "(~(25))" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseAnd_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) & SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) & (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseAnd_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) & SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 & 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBitwiseAndWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) & SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 & 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseOr_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) | SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) | (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseOr_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) | SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 | 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBitwiseOrWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) | SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 | 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseXor_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) ^ SqlNode.RawExpression( "foo.b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) ^ (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseXor_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) ^ SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "25 ^ 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBitwiseXorWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) ^ SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 ^ 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseLeftShift_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).BitwiseLeftShift( SqlNode.RawExpression( "foo.b" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) << (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseLeftShift_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ).BitwiseLeftShift( SqlNode.Literal( 35 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "25 << 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBitwiseLeftShiftWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ).BitwiseLeftShift( SqlNode.Literal( 35 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 << 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseRightShift_WhenValuesRequireParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).BitwiseRightShift( SqlNode.RawExpression( "foo.b" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) >> (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseRightShift_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ).BitwiseRightShift( SqlNode.Literal( 35 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "25 >> 35" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBitwiseRightShiftWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ).BitwiseRightShift( SqlNode.Literal( 35 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 >> 35)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSwitchCase_WhenValueRequiresParentheses()
    {
        _sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.RawExpression( "foo.b" ) ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WHEN foo.a > 10
  THEN (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSwitchCase_WhenValueDoesNotRequireParentheses()
    {
        _sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.Literal( 25 ) ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WHEN foo.a > 10
  THEN 25" );
    }

    [Fact]
    public void Visit_ShouldInterpretSwitch()
    {
        _sut.Visit(
            SqlNode.Switch(
                new[]
                {
                    SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.RawExpression( "foo.b" ) ),
                    SqlNode.RawCondition( "foo.a > 5" ).Then( SqlNode.Parameter<int>( "a" ) )
                },
                SqlNode.Literal( 25 ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"CASE
  WHEN foo.a > 10
    THEN (foo.b)
  WHEN foo.a > 5
    THEN @a
  ELSE 25
END" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSwitchWithParentheses()
    {
        _sut.VisitChild(
            SqlNode.Switch(
                new[]
                {
                    SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.RawExpression( "foo.b" ) ),
                    SqlNode.RawCondition( "foo.a > 5" ).Then( SqlNode.Parameter<int>( "a" ) )
                },
                SqlNode.Literal( 25 ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  CASE
    WHEN foo.a > 10
      THEN (foo.b)
    WHEN foo.a > 5
      THEN @a
    ELSE 25
  END
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretNamedFunction()
    {
        _sut.Visit(
            SqlNode.Functions.Named(
                SqlSchemaObjectName.Create( "foo", "bar" ),
                SqlNode.Parameter<int>( "a" ),
                SqlNode.RawExpression( "qux.a" ) ) );

        _sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`(@a, (qux.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretRecordsAffectedFunction()
    {
        _sut.Visit( SqlNode.Functions.RecordsAffected() );
        _sut.Context.Sql.ToString().Should().Be( "ROW_COUNT()" );
    }

    [Fact]
    public void Visit_ShouldInterpretCoalesceFunction()
    {
        _sut.Visit( SqlNode.Null().Coalesce( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "COALESCE(NULL, @a, (foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretCoalesceFunctionWithOneParameterThatRequiresParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Coalesce() );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCoalesceFunctionWithOneParameterThatDoesNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Coalesce() );
        _sut.Context.Sql.ToString().Should().Be( "25" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCoalesceFunctionWithOneParameterThatRequiresParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ).Coalesce() );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCoalesceFunctionWithOneParameterThatDoesNotRequireParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ).Coalesce() );
        _sut.Context.Sql.ToString().Should().Be( "25" );
    }

    [Fact]
    public void Visit_ShouldInterpretCurrentDateFunction()
    {
        _sut.Visit( SqlNode.Functions.CurrentDate() );
        _sut.Context.Sql.ToString().Should().Be( "CURRENT_DATE()" );
    }

    [Fact]
    public void Visit_ShouldInterpretCurrentTimeFunction()
    {
        _sut.Visit( SqlNode.Functions.CurrentTime() );
        _sut.Context.Sql.ToString().Should().Be( "CURRENT_TIME(6)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCurrentDateTimeFunction()
    {
        _sut.Visit( SqlNode.Functions.CurrentDateTime() );
        _sut.Context.Sql.ToString().Should().Be( "NOW(6)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCurrentTimestampFunction()
    {
        _sut.Visit( SqlNode.Functions.CurrentTimestamp() );
        _sut.Context.Sql.ToString().Should().Be( "CAST(UNIX_TIMESTAMP(NOW(6)) * 10000000 AS SIGNED)" );
    }

    [Fact]
    public void Visit_ShouldInterpretNewGuidFunction()
    {
        _sut.Visit( SqlNode.Functions.NewGuid() );
        _sut.Context.Sql.ToString().Should().Be( "GUID()" );
    }

    [Fact]
    public void Visit_ShouldInterpretLengthFunction()
    {
        _sut.Visit( SqlNode.Functions.Length( SqlNode.Literal( "foo" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "CHAR_LENGTH('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretByteLengthFunction()
    {
        _sut.Visit( SqlNode.Functions.ByteLength( SqlNode.Literal( "foo" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LENGTH('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretToLowerFunction()
    {
        _sut.Visit( SqlNode.Functions.ToLower( SqlNode.Literal( "FOO" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LOWER('FOO')" );
    }

    [Fact]
    public void Visit_ShouldInterpretToUpperFunction()
    {
        _sut.Visit( SqlNode.Functions.ToUpper( SqlNode.Literal( "foo" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "UPPER('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimStartFunction_WithOneArgument()
    {
        _sut.Visit( SqlNode.Functions.TrimStart( SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LTRIM((foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimStartFunction_WithTwoArguments()
    {
        _sut.Visit( SqlNode.Functions.TrimStart( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TRIM(LEADING 'bar' FROM (foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimEndFunction_WithOneArgument()
    {
        _sut.Visit( SqlNode.Functions.TrimEnd( SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "RTRIM((foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimEndFunction_WithTwoArguments()
    {
        _sut.Visit( SqlNode.Functions.TrimEnd( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TRIM(TRAILING 'bar' FROM (foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimFunction_WithOneArgument()
    {
        _sut.Visit( SqlNode.Functions.Trim( SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TRIM((foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimFunction_WithTwoArguments()
    {
        _sut.Visit( SqlNode.Functions.Trim( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TRIM(BOTH 'bar' FROM (foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretSubstringFunction()
    {
        _sut.Visit( SqlNode.Functions.Substring( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( 10 ), SqlNode.Literal( 5 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "SUBSTRING((foo.a), 10, 5)" );
    }

    [Fact]
    public void Visit_ShouldInterpretReplaceFunction()
    {
        _sut.Visit( SqlNode.Functions.Replace( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "REPLACE((foo.a), 'foo', 'bar')" );
    }

    [Fact]
    public void Visit_ShouldInterpretReverseFunction()
    {
        _sut.Visit( SqlNode.Functions.Reverse( SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "REVERSE((foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretIndexOfFunction()
    {
        _sut.Visit( SqlNode.Functions.IndexOf( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "INSTR((foo.a), 'bar')" );
    }

    [Fact]
    public void Visit_ShouldInterpretLastIndexOfFunction()
    {
        _sut.Visit( SqlNode.Functions.LastIndexOf( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be( "CHAR_LENGTH((foo.a)) - CHAR_LENGTH(SUBSTRING_INDEX((foo.a), 'bar', -1)) - CHAR_LENGTH('bar') + 1" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretLastIndexOfFunctionWithParentheses()
    {
        _sut.VisitChild( SqlNode.Functions.LastIndexOf( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be( "(CHAR_LENGTH((foo.a)) - CHAR_LENGTH(SUBSTRING_INDEX((foo.a), 'bar', -1)) - CHAR_LENGTH('bar') + 1)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSignFunction()
    {
        _sut.Visit( SqlNode.Functions.Sign( SqlNode.Parameter<int>( "a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "SIGN(@a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretAbsFunction()
    {
        _sut.Visit( SqlNode.Functions.Abs( SqlNode.Parameter<int>( "a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "ABS(@a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCeilingFunction()
    {
        _sut.Visit( SqlNode.Functions.Ceiling( SqlNode.Parameter<int>( "a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "CEIL(@a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretFloorFunction()
    {
        _sut.Visit( SqlNode.Functions.Floor( SqlNode.Parameter<int>( "a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "FLOOR(@a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTruncateFunction()
    {
        _sut.Visit( SqlNode.Functions.Truncate( SqlNode.Parameter<int>( "a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TRUNCATE(@a, 0)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTruncateFunction_WithPrecision()
    {
        _sut.Visit( SqlNode.Functions.Truncate( SqlNode.Parameter<int>( "a" ), SqlNode.Parameter<int>( "p" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TRUNCATE(@a, @p)" );
    }

    [Fact]
    public void Visit_ShouldInterpretRoundFunction()
    {
        _sut.Visit( SqlNode.Functions.Round( SqlNode.Parameter<int>( "a" ), SqlNode.Parameter<int>( "p" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "ROUND(@a, @p)" );
    }

    [Fact]
    public void Visit_ShouldInterpretPowerFunction()
    {
        _sut.Visit( SqlNode.Functions.Power( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "POW(@a, (foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretSquareRootFunction()
    {
        _sut.Visit( SqlNode.Functions.SquareRoot( SqlNode.Parameter<int>( "a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "SQRT(@a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMinFunction()
    {
        _sut.Visit( SqlNode.Null().Min( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LEAST(NULL, @a, (foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretMinFunctionWithOneParameterThatRequiresParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Min( Array.Empty<SqlExpressionNode>() ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMinFunctionWithOneParameterThatDoesNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Min( Array.Empty<SqlExpressionNode>() ) );
        _sut.Context.Sql.ToString().Should().Be( "25" );
    }

    [Fact]
    public void Visit_ShouldInterpretMaxFunction()
    {
        _sut.Visit( SqlNode.Null().Max( SqlNode.Parameter<int>( "a" ), SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "GREATEST(NULL, @a, (foo.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretMaxFunctionWithOneParameterThatRequiresParentheses()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Max( Array.Empty<SqlExpressionNode>() ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMaxFunctionWithOneParameterThatDoesNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Max( Array.Empty<SqlExpressionNode>() ) );
        _sut.Context.Sql.ToString().Should().Be( "25" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSimpleFunctionWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.Functions.RecordsAffected() );
        _sut.Context.Sql.ToString().Should().Be( "ROW_COUNT()" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenFunctionIsCustom()
    {
        var function = new FunctionMock();

        var action = Lambda.Of( () => _sut.Visit( function ) );

        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, function ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretNamedAggregateFunction()
    {
        _sut.Visit(
            SqlNode.AggregateFunctions.Named(
                SqlSchemaObjectName.Create( "foo", "bar" ),
                SqlNode.Parameter<int>( "a" ),
                SqlNode.RawExpression( "qux.a" ) ) );

        _sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`(@a, (qux.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretNamedAggregateFunctionWithTraits()
    {
        _sut.Visit(
            SqlNode.AggregateFunctions.Named(
                    SqlSchemaObjectName.Create( "foo", "bar" ),
                    SqlNode.Parameter<int>( "a" ),
                    SqlNode.RawExpression( "qux.a" ) )
                .Distinct()
                .AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );

        _sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`(DISTINCT CASE WHEN foo.a > 10 THEN @a ELSE NULL END, (qux.a))" );
    }

    [Fact]
    public void Visit_ShouldInterpretMinAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Min() );
        _sut.Context.Sql.ToString().Should().Be( "MIN(25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMinAggregateFunctionWithTraits()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Min().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "MIN(DISTINCT CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMaxAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Max() );
        _sut.Context.Sql.ToString().Should().Be( "MAX(25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMaxAggregateFunctionWithTraits()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Max().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "MAX(DISTINCT CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END)" );
    }

    [Fact]
    public void Visit_ShouldInterpretAverageAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Average() );
        _sut.Context.Sql.ToString().Should().Be( "AVG(25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretAverageAggregateFunctionWithTraits()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Average().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "AVG(DISTINCT CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSumAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Sum() );
        _sut.Context.Sql.ToString().Should().Be( "SUM(25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSumAggregateFunctionWithTraits()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Sum().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "SUM(DISTINCT CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCountAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Count() );
        _sut.Context.Sql.ToString().Should().Be( "COUNT(25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCountAggregateFunctionWithTraits()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Count().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "COUNT(DISTINCT CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END)" );
    }

    [Fact]
    public void Visit_ShouldInterpretStringConcatAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( "foo" ).StringConcat() );
        _sut.Context.Sql.ToString().Should().Be( "GROUP_CONCAT('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretStringConcatAggregateFunction_WithSeparator()
    {
        _sut.Visit( SqlNode.Literal( "foo" ).StringConcat( SqlNode.Literal( " - " ) ) );
        _sut.Context.Sql.ToString().Should().Be( "GROUP_CONCAT('foo' SEPARATOR ' - ')" );
    }

    [Fact]
    public void Visit_ShouldInterpretStringConcatAggregateFunctionWithTraits()
    {
        var window = SqlNode.WindowDefinition( "wnd", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit(
            SqlNode.RawExpression( "foo.a" )
                .StringConcat()
                .Distinct()
                .AndWhere( SqlNode.RawCondition( "foo.b > 10" ) )
                .Over( window )
                .AddTrait( SqlNode.SortTrait( SqlNode.RawExpression( "foo.c" ).Asc(), SqlNode.RawExpression( "foo.d" ).Desc() ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be( "GROUP_CONCAT(DISTINCT CASE WHEN foo.b > 10 THEN (foo.a) ELSE NULL END ORDER BY (foo.c) ASC, (foo.d) DESC) OVER `wnd`" );
    }

    [Fact]
    public void Visit_ShouldInterpretStringConcatAggregateFunctionWithTraitsAndSeparator()
    {
        var window = SqlNode.WindowDefinition( "wnd", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit(
            SqlNode.RawExpression( "foo.a" )
                .StringConcat( SqlNode.Literal( " - " ) )
                .Distinct()
                .AndWhere( SqlNode.RawCondition( "foo.b > 10" ) )
                .Over( window )
                .AddTrait( SqlNode.SortTrait( SqlNode.RawExpression( "foo.c" ).Asc(), SqlNode.RawExpression( "foo.d" ).Desc() ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                "GROUP_CONCAT(DISTINCT CASE WHEN foo.b > 10 THEN (foo.a) ELSE NULL END ORDER BY (foo.c) ASC, (foo.d) DESC SEPARATOR ' - ') OVER `wnd`" );
    }

    [Fact]
    public void Visit_ShouldInterpretRowNumberWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.WindowFunctions.RowNumber().Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "ROW_NUMBER() OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretRankWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.WindowFunctions.Rank().Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "RANK() OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretDenseRankWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.WindowFunctions.DenseRank().Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "DENSE_RANK() OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretCumulativeDistributionWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.WindowFunctions.CumulativeDistribution().Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "CUME_DIST() OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretNTileWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.RawExpression( "bar.a" ).NTile().Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "NTILE((bar.a)) OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretLagWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.RawExpression( "bar.a" ).Lag( SqlNode.Literal( 3 ), SqlNode.Literal( "x" ) ).Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "LAG((bar.a), 3, 'x') OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretLeadWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.RawExpression( "bar.a" ).Lead( SqlNode.Literal( 3 ), SqlNode.Literal( "x" ) ).Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "LEAD((bar.a), 3, 'x') OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretFirstValueWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.RawExpression( "bar.a" ).FirstValue().Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "FIRST_VALUE((bar.a)) OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretLastValueWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.RawExpression( "bar.a" ).LastValue().Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "LAST_VALUE((bar.a)) OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretNthValueWindowFunction()
    {
        var window = SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "a" ).Asc() } );
        _sut.Visit( SqlNode.RawExpression( "bar.a" ).NthValue( SqlNode.Literal( 5 ) ).Over( window ) );
        _sut.Context.Sql.ToString().Should().Be( "NTH_VALUE((bar.a), 5) OVER `foo`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretAggregateFunctionWithoutParentheses_WhenTraitsAreEmpty()
    {
        _sut.VisitChild( SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "COUNT((foo.a))" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretAggregateFunctionWithoutParentheses_WhenTraitsOnlyContainsDistinct()
    {
        _sut.VisitChild( SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) ).Distinct() );
        _sut.Context.Sql.ToString().Should().Be( "COUNT(DISTINCT (foo.a))" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretAggregateFunctionWithoutParentheses_WhenTraitsOnlyContainsFilter()
    {
        _sut.VisitChild(
            SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) ).AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );

        _sut.Context.Sql.ToString().Should().Be( "COUNT(CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretAggregateFunctionWithoutParentheses_WhenTraitsOnlyContainsDistinctAndFilter()
    {
        _sut.VisitChild(
            SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) )
                .Distinct()
                .AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );

        _sut.Context.Sql.ToString().Should().Be( "COUNT(DISTINCT CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretAggregateFunctionWithParentheses_WhenTraitsContainsWindow()
    {
        _sut.VisitChild(
            SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) )
                .Distinct()
                .AndWhere( SqlNode.RawCondition( "foo.a > 10" ) )
                .Over( SqlNode.WindowDefinition( "wnd", new[] { SqlNode.RawExpression( "foo.a" ).Asc() } ) ) );

        _sut.Context.Sql.ToString().Should().Be( "(COUNT(DISTINCT CASE WHEN foo.a > 10 THEN (foo.a) ELSE NULL END) OVER `wnd`)" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenAggregateFunctionIsCustom()
    {
        var function = new AggregateFunctionMock();

        var action = Lambda.Of( () => _sut.Visit( function ) );

        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, function ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretRawCondition()
    {
        _sut.Visit( SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "a", (TypeNullability?)TypeNullability.Create<int>() ) );

            _sut.Context.Sql.ToString().Should().Be( "foo.a > @a" );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawConditionWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a > 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrue()
    {
        _sut.Visit( SqlNode.True() );
        _sut.Context.Sql.ToString().Should().Be( "TRUE" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTrueWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.True() );
        _sut.Context.Sql.ToString().Should().Be( "TRUE" );
    }

    [Fact]
    public void Visit_ShouldInterpretFalse()
    {
        _sut.Visit( SqlNode.False() );
        _sut.Context.Sql.ToString().Should().Be( "FALSE" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretFalseWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.False() );
        _sut.Context.Sql.ToString().Should().Be( "FALSE" );
    }

    [Fact]
    public void Visit_ShouldInterpretEqualTo()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) == SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) = 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretEqualTo_WhenRightOperandIsNull()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) == SqlNode.Null() );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) IS NULL" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretEqualToWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ) == SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) = 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretNotEqualTo()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) != SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) <> 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretNotEqualTo_WhenRightOperandIsNull()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) != SqlNode.Null() );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) IS NOT NULL" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretNotEqualToWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ) != SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) <> 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretGreaterThan()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) > SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) > 10" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretGreaterThanWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ) > SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) > 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretLessThan()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) < SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) < 10" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretLessThanWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ) < SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) < 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretGreaterThanOrEqualTo()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) >= SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) >= 10" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretGreaterThanOrEqualToWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ) >= SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) >= 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretLessThanOrEqualTo()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ) <= SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) <= 10" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretLessThanOrEqualToWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ) <= SqlNode.Literal( 10 ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) <= 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretAnd()
    {
        _sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).And( SqlNode.RawCondition( "foo.b < 20" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a > 10) AND (foo.b < 20)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretAndWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ).And( SqlNode.RawCondition( "foo.b < 20" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a > 10) AND (foo.b < 20))" );
    }

    [Fact]
    public void Visit_ShouldInterpretOr()
    {
        _sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).Or( SqlNode.RawCondition( "foo.b < 20" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a > 10) OR (foo.b < 20)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretOrWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ).Or( SqlNode.RawCondition( "foo.b < 20" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a > 10) OR (foo.b < 20))" );
    }

    [Fact]
    public void Visit_ShouldInterpretConditionValue()
    {
        _sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).ToValue() );
        _sut.Context.Sql.ToString().Should().Be( "foo.a > 10" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretConditionValueWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ).ToValue() );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a > 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretBetween()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).IsBetween( SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) BETWEEN 10 AND 20" );
    }

    [Fact]
    public void Visit_ShouldInterpretNotBetween()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).IsNotBetween( SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) NOT BETWEEN 10 AND 20" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBetweenWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ).IsBetween( SqlNode.Literal( 10 ), SqlNode.Literal( 20 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) BETWEEN 10 AND 20)" );
    }

    [Fact]
    public void Visit_ShouldInterpretExists()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).Exists() );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"EXISTS (
  SELECT * FROM foo
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretNotExists()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).NotExists() );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"NOT EXISTS (
  SELECT * FROM foo
)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretExistsWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ).Exists() );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(EXISTS (
    SELECT * FROM foo
  ))" );
    }

    [Fact]
    public void Visit_ShouldInterpretLike()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Like( SqlNode.Literal( "\\%bar%" ), SqlNode.Literal( "\\" ) ) );
        _sut.Context.Sql.ToString().Should().Be( @"(foo.a) LIKE '\%bar%' ESCAPE '\'" );
    }

    [Fact]
    public void Visit_ShouldInterpretNotLike()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).NotLike( SqlNode.Literal( "%bar%" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) NOT LIKE '%bar%'" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretLikeWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ).Like( SqlNode.Literal( "%bar%" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) LIKE '%bar%')" );
    }

    [Fact]
    public void Visit_ShouldInterpretIn()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).In( SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) IN ('foo', 'bar')" );
    }

    [Fact]
    public void Visit_ShouldInterpretNotIn()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).NotIn( SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) NOT IN ('foo', 'bar')" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretInWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ).In( SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) IN ('foo', 'bar'))" );
    }

    [Fact]
    public void Visit_ShouldInterpretInQuery()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).InQuery( SqlNode.RawQuery( "SELECT qux FROM bar" ) ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(foo.a) IN (
  SELECT qux FROM bar
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretNotInQuery()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).NotInQuery( SqlNode.RawQuery( "SELECT qux FROM bar" ) ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(foo.a) NOT IN (
  SELECT qux FROM bar
)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretInQueryWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ).InQuery( SqlNode.RawQuery( "SELECT qux FROM bar" ) ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"((foo.a) IN (
    SELECT qux FROM bar
  ))" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawRecordSet()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo", "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "foo AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawRecordSetWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTable()
    {
        _sut.Visit( CreateTable( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTableWithParentheses()
    {
        _sut.VisitChild( CreateTable( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTableBuilder()
    {
        _sut.Visit( CreateTableBuilder( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTableBuilderWithParentheses()
    {
        _sut.VisitChild( CreateTableBuilder( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretView()
    {
        _sut.Visit( CreateView( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewWithParentheses()
    {
        _sut.VisitChild( CreateView( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretViewBuilder()
    {
        _sut.Visit( CreateViewBuilder( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewBuilderWithParentheses()
    {
        _sut.VisitChild( CreateViewBuilder( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretQueryRecordSet()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).AsSet( "bar" ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  SELECT * FROM foo
) AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretQueryRecordSetWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ).AsSet( "bar" ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"((
    SELECT * FROM foo
  ) AS `bar`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpressionRecordSet()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet.As( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( "`bar` AS `qux`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCommonTableExpressionRecordSetWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet );
        _sut.Context.Sql.ToString().Should().Be( "(`bar`)" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar` AS `qux`" )]
    [InlineData( true, "`foo` AS `qux`" )]
    public void Visit_ShouldInterpretNewTable(bool isTemporary, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() ).AsSet( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "(`foo`.`bar`)" )]
    [InlineData( true, "(`foo`)" )]
    public void VisitChild_ShouldInterpretNewTableWithParentheses(bool isTemporary, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.VisitChild( SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() ).AsSet() );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar` AS `qux`" )]
    [InlineData( true, "`foo` AS `qux`" )]
    public void Visit_ShouldInterpretNewView(bool isTemporary, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM lorem" ) ).AsSet( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "(`foo`.`bar`)" )]
    [InlineData( true, "(`foo`)" )]
    public void VisitChild_ShouldInterpretNewViewWithParentheses(bool isTemporary, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.VisitChild( SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM bar" ) ).AsSet() );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Fact]
    public void Visit_ShouldInterpretLeftJoinOn()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).LeftOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LEFT JOIN foo ON bar.a = foo.a" );
    }

    [Fact]
    public void Visit_ShouldInterpretRightJoinOn()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).RightOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "RIGHT JOIN foo ON bar.a = foo.a" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenInterpretingFullJoinOn()
    {
        var node = SqlNode.RawRecordSet( "foo" ).FullOn( SqlNode.RawCondition( "bar.a = foo.a" ) );
        var action = Lambda.Of( () => _sut.Visit( node ) );
        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretInnerJoinOn()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).InnerOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "INNER JOIN foo ON bar.a = foo.a" );
    }

    [Fact]
    public void Visit_ShouldInterpretCrossJoin()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).Cross() );
        _sut.Context.Sql.ToString().Should().Be( "CROSS JOIN foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretDummyDataSource()
    {
        _sut.Visit( SqlNode.DummyDataSource() );
        _sut.Context.Sql.ToString().Should().Be( string.Empty );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceWithoutJoins()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource() );
        _sut.Context.Sql.ToString().Should().Be( "FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceWithJoins()
    {
        _sut.Visit(
            SqlNode.RawRecordSet( "foo" )
                .Join(
                    SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.RawCondition( "bar.a = foo.a" ) ),
                    SqlNode.RawRecordSet( "qux" ).LeftOn( SqlNode.RawCondition( "qux.b = foo.b" ) ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"FROM foo
INNER JOIN bar ON bar.a = foo.a
LEFT JOIN qux ON qux.b = foo.b" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretDataSourceWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" ).ToDataSource() );
        _sut.Context.Sql.ToString().Should().Be( "(FROM foo)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectField()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).As( "b" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) AS `b`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectFieldWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" )["a"].AsSelf() );
        _sut.Context.Sql.ToString().Should().Be( "foo.`a`" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectCompoundField()
    {
        var query = CreateTable( string.Empty, "foo", "a" )
            .ToRecordSet()
            .ToDataSource()
            .Select( t => new[] { t.From["a"].AsSelf() } )
            .CompoundWith(
                CreateTable( string.Empty, "bar", "a" )
                    .ToRecordSet()
                    .ToDataSource()
                    .Select( t => new[] { t.From["a"].AsSelf() } )
                    .ToUnionAll() );

        _sut.Visit( query.Selection.Span[0] );

        _sut.Context.Sql.ToString().Should().Be( "`a`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectCompoundFieldWithoutParentheses()
    {
        var query = CreateTable( string.Empty, "foo", "a" )
            .ToRecordSet()
            .ToDataSource()
            .Select( t => new[] { t.From["a"].AsSelf() } )
            .CompoundWith(
                CreateTable( string.Empty, "bar", "a" )
                    .ToRecordSet()
                    .ToDataSource()
                    .Select( t => new[] { t.From["a"].AsSelf() } )
                    .ToUnionAll() );

        _sut.VisitChild( query.Selection.Span[0] );

        _sut.Context.Sql.ToString().Should().Be( "`a`" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectRecordSet()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).GetAll() );
        _sut.Context.Sql.ToString().Should().Be( "foo.*" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectRecordSetWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" ).GetAll() );
        _sut.Context.Sql.ToString().Should().Be( "foo.*" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectAll()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll() );
        _sut.Context.Sql.ToString().Should().Be( "*" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectAllWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll() );
        _sut.Context.Sql.ToString().Should().Be( "*" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectExpression_WhenSelectionIsField()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource().From["a"].As( "b" ).ToExpression() );
        _sut.Context.Sql.ToString().Should().Be( "`b`" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectExpression_WhenSelectionIsNotField()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll() );
        _sut.Context.Sql.ToString().Should().Be( "*" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectExpressionWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" ).ToDataSource().From["a"].AsSelf().ToExpression() );
        _sut.Context.Sql.ToString().Should().Be( "`a`" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawQuery()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a = @a", SqlNode.Parameter<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "a", (TypeNullability?)TypeNullability.Create<int>() ) );

            _sut.Context.Sql.ToString().Should().Be( "SELECT * FROM foo WHERE foo.a = @a" );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawQueryWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ) );
        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  SELECT * FROM foo
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithoutTraits()
    {
        var foo = CreateTable( string.Empty, "foo", "a", "b" ).ToRecordSet();
        var bar = CreateTable( string.Empty, "bar", "c", "d" ).ToRecordSet( "lorem" );
        var qux = CreateTable( string.Empty, "qux", "e", "f" ).ToRecordSet();

        var query = foo
            .Join( bar.InnerOn( bar["c"] == foo["a"] ), qux.LeftOn( qux["e"] == foo["b"] ) )
            .Select(
                s => new SqlSelectNode[]
                {
                    s["foo"]["a"].AsSelf(),
                    s["foo"]["b"].As( "x" ),
                    s["lorem"].GetAll(),
                    s["qux"]["e"].AsSelf(),
                    s["qux"]["f"].As( "y" ),
                    SqlNode.Parameter<int>( "p" ).As( "z" )
                } );

        _sut.Visit( query );

        using ( new AssertionScope() )
        {
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "p", (TypeNullability?)TypeNullability.Create<int>() ) );

            _sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"SELECT
  `foo`.`a`,
  `foo`.`b` AS `x`,
  `lorem`.*,
  `qux`.`e`,
  `qux`.`f` AS `y`,
  @p AS `z`
FROM `foo`
INNER JOIN `bar` AS `lorem` ON `lorem`.`c` = `foo`.`a`
LEFT JOIN `qux` ON `qux`.`e` = `foo`.`b`" );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithAllTraits()
    {
        var cba = SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" );
        var zyx = SqlNode.RawQuery( "SELECT * FROM xyz JOIN cba ON cba.h = xyz.h" ).ToCte( "zyx" );

        var foo = CreateTable( string.Empty, "foo", "a", "b" ).ToRecordSet();
        var bar = CreateTable( string.Empty, "bar", "c", "d" ).ToRecordSet( "lorem" );
        var qux = CreateTable( string.Empty, "qux", "e", "f" ).ToRecordSet();

        var wnd1 = SqlNode.WindowDefinition( "wnd1", new SqlExpressionNode[] { foo["a"], qux["e"] }, new[] { foo["b"].Asc() } );
        var wnd2 = SqlNode.WindowDefinition(
            "wnd2",
            new[] { qux["e"].Asc(), qux["f"].Desc() },
            SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.CurrentRow ) );

        var query = foo
            .Join( bar.InnerOn( bar["c"] == foo["a"] ), qux.LeftOn( qux["e"] == foo["b"] ) )
            .With( cba, zyx )
            .Distinct()
            .AndWhere( s => s["qux"]["f"] > SqlNode.Literal( 50 ) )
            .AndWhere(
                s => s["foo"]["a"].InQuery( zyx.RecordSet.ToDataSource().Select( z => new[] { z.From.GetUnsafeField( "h" ).AsSelf() } ) ) )
            .GroupBy( s => new[] { s["foo"]["b"] } )
            .GroupBy( s => new[] { s["lorem"]["c"] } )
            .AndHaving( s => s["foo"]["b"] < SqlNode.Literal( 100 ) )
            .OrHaving( s => s["lorem"]["c"].IsBetween( SqlNode.Literal( 0 ), SqlNode.Literal( 75 ) ) )
            .Window( wnd1, wnd2 )
            .Select(
                s => new SqlSelectNode[]
                {
                    s["foo"]["b"].As( "x" ),
                    s["lorem"]["c"].AsSelf(),
                    SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).As( "v" ),
                    SqlNode.AggregateFunctions.Sum( s["foo"]["a"] ).Over( wnd1 ).As( "w" )
                } )
            .OrderBy( s => new[] { s.DataSource["foo"]["b"].Asc() } )
            .OrderBy( s => new[] { s.DataSource["lorem"]["c"].Desc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `cba` AS (
  SELECT * FROM abc
),
`zyx` AS (
  SELECT * FROM xyz JOIN cba ON cba.h = xyz.h
)
SELECT DISTINCT
  `foo`.`b` AS `x`,
  `lorem`.`c`,
  COUNT(*) AS `v`,
  (SUM(`foo`.`a`) OVER `wnd1`) AS `w`
FROM `foo`
INNER JOIN `bar` AS `lorem` ON `lorem`.`c` = `foo`.`a`
LEFT JOIN `qux` ON `qux`.`e` = `foo`.`b`
WHERE (`qux`.`f` > 50) AND (`foo`.`a` IN (
    SELECT
      `zyx`.`h`
    FROM `zyx`
  ))
GROUP BY `foo`.`b`, `lorem`.`c`
HAVING (`foo`.`b` < 100) OR (`lorem`.`c` BETWEEN 0 AND 75)
WINDOW `wnd1` AS (PARTITION BY `foo`.`a`, `qux`.`e` ORDER BY `foo`.`b` ASC),
  `wnd2` AS (ORDER BY `qux`.`e` ASC, `qux`.`f` DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
ORDER BY `foo`.`b` ASC, `lorem`.`c` DESC
LIMIT 50 OFFSET 100" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithRecursiveCommonTableExpression()
    {
        var cba = SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" );
        var zyx = SqlNode.RawQuery( "SELECT * FROM xyz JOIN cba ON cba.h = xyz.h" )
            .ToCte( "zyx" )
            .ToRecursive( SqlNode.RawQuery( "SELECT * FROM zyx" ).ToUnion() );

        var query = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .With( cba, zyx );

        _sut.Visit( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH RECURSIVE `cba` AS (
  SELECT * FROM abc
),
`zyx` AS (
  (
    SELECT * FROM xyz JOIN cba ON cba.h = xyz.h
  )
  UNION
  (
    SELECT * FROM zyx
  )
)
SELECT
  foo.`a`
FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithLimitOnly()
    {
        var query = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .Limit( SqlNode.Literal( 100 ) );

        _sut.Visit( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"SELECT
  foo.`a`
FROM foo
LIMIT 100" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithOffsetOnly()
    {
        var query = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"SELECT
  foo.`a`
FROM foo
LIMIT 18446744073709551615 OFFSET 100" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretDataSourceQueryWithParentheses()
    {
        var query = CreateTable( string.Empty, "foo", "a" )
            .ToRecordSet()
            .ToDataSource()
            .Select( s => new[] { s.GetAll() } );

        _sut.VisitChild( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  SELECT
    *
  FROM `foo`
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCompoundQuery_WithoutTraits()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" )
            .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT * FROM qux" ).ToUnion() );

        _sut.Visit( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  SELECT * FROM foo
)
UNION ALL
(
  SELECT * FROM bar
)
UNION
(
  SELECT * FROM qux
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCompoundQuery_WithAllTraits()
    {
        var query = SqlNode.RawQuery( "SELECT foo.* FROM foo JOIN x ON x.a = foo.a" )
            .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT * FROM qux" ).ToUnion() )
            .With( SqlNode.RawQuery( "SELECT * FROM lorem" ).ToCte( "x" ) )
            .OrderBy( SqlNode.RawExpression( "a" ).Asc() )
            .OrderBy( SqlNode.RawExpression( "b" ).Desc() )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 75 ) );

        _sut.Visit( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `x` AS (
  SELECT * FROM lorem
)
(
  SELECT foo.* FROM foo JOIN x ON x.a = foo.a
)
UNION ALL
(
  SELECT * FROM bar
)
UNION
(
  SELECT * FROM qux
)
ORDER BY (a) ASC, (b) DESC
LIMIT 50 OFFSET 75" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCompoundQueryWithParentheses()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" )
            .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT * FROM qux" ).ToUnion() );

        _sut.VisitChild( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  (
    SELECT * FROM foo
  )
  UNION ALL
  (
    SELECT * FROM bar
  )
  UNION
  (
    SELECT * FROM qux
  )
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCompoundQueryComponent()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM qux" ).ToExcept() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"EXCEPT
(
  SELECT * FROM qux
)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCompoundQueryComponentWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM qux" ).ToIntersect() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INTERSECT
(
  SELECT * FROM qux
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDistinctTrait()
    {
        _sut.Visit( SqlNode.DistinctTrait() );
        _sut.Context.Sql.ToString().Should().Be( "DISTINCT" );
    }

    [Fact]
    public void Visit_ShouldInterpretFilterTrait()
    {
        _sut.Visit( SqlNode.FilterTrait( SqlNode.RawCondition( "foo.a > 10" ), isConjunction: true ) );
        _sut.Context.Sql.ToString().Should().Be( "WHERE foo.a > 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretAggregationTrait()
    {
        _sut.Visit( SqlNode.AggregationTrait( SqlNode.RawExpression( "foo.a" ), SqlNode.RawExpression( "foo.b" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "GROUP BY (foo.a), (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretAggregationFilterTrait()
    {
        _sut.Visit( SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "foo.a > 10" ), isConjunction: true ) );
        _sut.Context.Sql.ToString().Should().Be( "HAVING foo.a > 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretSortTrait()
    {
        _sut.Visit( SqlNode.SortTrait( SqlNode.RawExpression( "foo.a" ).Asc(), SqlNode.RawExpression( "foo.b" ).Desc() ) );
        _sut.Context.Sql.ToString().Should().Be( "ORDER BY (foo.a) ASC, (foo.b) DESC" );
    }

    [Fact]
    public void Visit_ShouldInterpretLimitTrait()
    {
        _sut.Visit( SqlNode.LimitTrait( SqlNode.Literal( 10 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LIMIT 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretOffsetTrait()
    {
        _sut.Visit( SqlNode.OffsetTrait( SqlNode.Literal( 10 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "OFFSET 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpressionTrait()
    {
        _sut.Visit(
            SqlNode.CommonTableExpressionTrait(
                SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "A" ),
                SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "B" ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `A` AS (
  SELECT * FROM foo
),
`B` AS (
  SELECT * FROM bar
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpressionTrait_WithRecursive()
    {
        _sut.Visit(
            SqlNode.CommonTableExpressionTrait(
                SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "A" ),
                SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "B" ).ToRecursive( SqlNode.RawQuery( "SELECT * FROM B" ).ToUnion() ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH RECURSIVE `A` AS (
  SELECT * FROM foo
),
`B` AS (
  (
    SELECT * FROM bar
  )
  UNION
  (
    SELECT * FROM B
  )
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretEmptyCommonTableExpressionTrait()
    {
        _sut.Visit( SqlNode.CommonTableExpressionTrait() );
        _sut.Context.Sql.ToString().Should().Be( "WITH" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowDefinitionTrait()
    {
        _sut.Visit(
            SqlNode.WindowDefinitionTrait(
                SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "qux.a" ).Asc() } ),
                SqlNode.WindowDefinition(
                    "bar",
                    new SqlExpressionNode[] { SqlNode.RawExpression( "qux.a" ) },
                    new[] { SqlNode.RawExpression( "qux.b" ).Desc() },
                    SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.CurrentRow ) ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WINDOW `foo` AS (ORDER BY (qux.a) ASC),
  `bar` AS (PARTITION BY (qux.a) ORDER BY (qux.b) DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowTrait()
    {
        _sut.Visit( SqlNode.WindowTrait( SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "qux.a" ).Asc() } ) ) );
        _sut.Context.Sql.ToString().Should().Be( "OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretOrderBy()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Asc() );
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) ASC" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpression()
    {
        _sut.Visit(
            SqlNode.RawQuery( "SELECT * FROM foo" )
                .ToCte( "A" )
                .ToRecursive( SqlNode.RawQuery( "SELECT * FROM A WHERE A.depth < 10" ).ToUnionAll() ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"`A` AS (
  (
    SELECT * FROM foo
  )
  UNION ALL
  (
    SELECT * FROM A WHERE A.depth < 10
  )
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowDefinition()
    {
        _sut.Visit(
            SqlNode.WindowDefinition(
                "foo",
                new SqlExpressionNode[] { SqlNode.RawExpression( "qux.a" ), SqlNode.RawExpression( "qux.b" ) },
                new[] { SqlNode.RawExpression( "qux.c" ).Asc(), SqlNode.RawExpression( "qux.d" ).Desc() },
                SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.UnboundedFollowing ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                "`foo` AS (PARTITION BY (qux.a), (qux.b) ORDER BY (qux.c) ASC, (qux.d) DESC ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING)" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowFrame()
    {
        _sut.Visit(
            SqlNode.RangeWindowFrame(
                SqlWindowFrameBoundary.Preceding( SqlNode.Literal( 3 ) ),
                SqlWindowFrameBoundary.Following( SqlNode.Literal( 5 ) ) ) );

        _sut.Context.Sql.ToString().Should().Be( "RANGE BETWEEN 3 PRECEDING AND 5 FOLLOWING" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenNodeIsCustomWindowFrame()
    {
        var node = new WindowFrameMock();

        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Theory]
    [InlineData( typeof( bool ), "SIGNED" )]
    [InlineData( typeof( sbyte ), "SIGNED" )]
    [InlineData( typeof( short ), "SIGNED" )]
    [InlineData( typeof( int ), "SIGNED" )]
    [InlineData( typeof( TypeMock<int> ), "SIGNED" )]
    [InlineData( typeof( long ), "SIGNED" )]
    [InlineData( typeof( byte ), "UNSIGNED" )]
    [InlineData( typeof( ushort ), "UNSIGNED" )]
    [InlineData( typeof( uint ), "UNSIGNED" )]
    [InlineData( typeof( TypeMock<uint> ), "UNSIGNED" )]
    [InlineData( typeof( ulong ), "UNSIGNED" )]
    [InlineData( typeof( float ), "FLOAT" )]
    [InlineData( typeof( double ), "DOUBLE" )]
    [InlineData( typeof( decimal ), "DECIMAL(29, 10)" )]
    [InlineData( typeof( char ), "CHAR(1)" )]
    [InlineData( typeof( TypeMock<IEnumerable<char>> ), "CHAR" )]
    [InlineData( typeof( TypeMock<ICollection<char>> ), "CHAR(65535)" )]
    [InlineData( typeof( TypeMock<IList<char>> ), "CHAR" )]
    [InlineData( typeof( TypeMock<char[]> ), "CHAR" )]
    [InlineData( typeof( TypeMock<IReadOnlyList<char>> ), "CHAR" )]
    [InlineData( typeof( string ), "CHAR" )]
    [InlineData( typeof( Guid ), "BINARY(16)" )]
    [InlineData( typeof( TypeMock<ICollection<byte>> ), "BINARY(65535)" )]
    [InlineData( typeof( TypeMock<IList<byte>> ), "BINARY" )]
    [InlineData( typeof( TypeMock<IEnumerable<byte>> ), "BINARY" )]
    [InlineData( typeof( TypeMock<IReadOnlyList<byte>> ), "BINARY" )]
    [InlineData( typeof( byte[] ), "BINARY" )]
    [InlineData( typeof( DateOnly ), "DATE" )]
    [InlineData( typeof( TimeOnly ), "TIME(6)" )]
    [InlineData( typeof( DateTime ), "DATETIME(6)" )]
    public void Visit_ShouldInterpretTypeCast(Type type, string expectedDbType)
    {
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<int>>( MySqlDbType.Int24 ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<uint>>( MySqlDbType.UInt24 ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IEnumerable<char>>>( MySqlDbType.VarString ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<ICollection<char>>>( MySqlDataType.VarChar ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IList<char>>>( MySqlDbType.TinyText ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<char[]>>( MySqlDbType.Text ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IReadOnlyList<char>>>( MySqlDbType.MediumText ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<ICollection<byte>>>( MySqlDataType.VarBinary ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IList<byte>>>( MySqlDbType.TinyBlob ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IEnumerable<byte>>>( MySqlDbType.Blob ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IReadOnlyList<byte>>>( MySqlDbType.MediumBlob ) );

        _sut.Visit( SqlNode.RawExpression( "foo.a" ).CastTo( type ) );
        _sut.Context.Sql.ToString().Should().Be( $"CAST((foo.a) AS {expectedDbType})" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTypeCastWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ).CastTo<int>() );
        _sut.Context.Sql.ToString().Should().Be( "CAST((foo.a) AS SIGNED)" );
    }

    [Fact]
    public void Visit_ShouldInterpretValues()
    {
        _sut.Visit(
            SqlNode.Values(
                new[,]
                {
                    { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                    { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"VALUES
('foo', 5),
((bar.a), 25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawStatement()
    {
        _sut.Visit(
            SqlNode.RawStatement(
                @"INSERT INTO foo (a, b)
VALUES (@a, 1)",
                SqlNode.Parameter<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO foo (a, b)
VALUES (@a, 1)" );

            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "a", (TypeNullability?)TypeNullability.Create<int>() ) );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithValues()
    {
        _sut.Visit(
            SqlNode.Values(
                    new[,]
                    {
                        { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) },
                        { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                    } )
                .ToInsertInto( SqlNode.RawRecordSet( "qux" ), r => new[] { r["a"], r["b"] } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INSERT INTO qux (`a`, `b`)
VALUES
('foo', 5),
((bar.a), 25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithRawQuery()
    {
        _sut.Visit(
            SqlNode.RawQuery( "SELECT a, b FROM foo" ).ToInsertInto( SqlNode.RawRecordSet( "qux" ), r => new[] { r["a"], r["b"] } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INSERT INTO qux (`a`, `b`)
SELECT a, b FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithDataSourceQuery()
    {
        var foo = CreateTable( string.Empty, "foo", "a", "b" ).ToRecordSet();
        var bar = CreateTable( string.Empty, "bar", "c", "d" ).ToRecordSet();
        var wnd = SqlNode.WindowDefinition( "wnd", new[] { foo["a"].Asc() } );

        var query = foo
            .Join( bar.InnerOn( bar["c"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["foo"]["b"] } )
            .AndHaving( s => s["foo"]["b"] < SqlNode.Literal( 100 ) )
            .Window( wnd )
            .Select(
                s => new SqlSelectNode[]
                {
                    s["foo"]["b"].As( "a" ),
                    SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).Over( wnd ).As( "b" )
                } )
            .OrderBy( s => new[] { s.DataSource["foo"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( query.ToInsertInto( SqlNode.RawRecordSet( "qux" ), r => new[] { r["a"], r["b"] } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INSERT INTO qux (`a`, `b`)
WITH `cba` AS (
  SELECT * FROM abc
)
SELECT DISTINCT
  `foo`.`b` AS `a`,
  (COUNT(*) OVER `wnd`) AS `b`
FROM `foo`
INNER JOIN `bar` ON `bar`.`c` = `foo`.`a`
WHERE `bar`.`c` IN (
  SELECT cba.c FROM cba
)
GROUP BY `foo`.`b`
HAVING `foo`.`b` < 100
WINDOW `wnd` AS (ORDER BY `foo`.`a` ASC)
ORDER BY `foo`.`b` ASC
LIMIT 50 OFFSET 100" );
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithCompoundQuery()
    {
        var query = SqlNode.RawQuery( "SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a" )
            .CompoundWith( SqlNode.RawQuery( "SELECT a, b FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT a, b FROM qux" ).ToUnion() )
            .With( SqlNode.RawQuery( "SELECT * FROM ipsum" ).ToCte( "x" ) )
            .OrderBy( SqlNode.RawExpression( "a" ).Asc() )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 75 ) );

        _sut.Visit( query.ToInsertInto( SqlNode.RawRecordSet( "lorem" ), r => new[] { r["a"], r["b"] } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INSERT INTO lorem (`a`, `b`)
WITH `x` AS (
  SELECT * FROM ipsum
)
(
  SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
)
UNION ALL
(
  SELECT a, b FROM bar
)
UNION
(
  SELECT a, b FROM qux
)
ORDER BY (a) ASC
LIMIT 50 OFFSET 75" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateSingleDataSource_WithoutTraits()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["foo"]["a"].Assign( SqlNode.Literal( "bar" ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"UPDATE foo SET
  `a` = 'bar'" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateSingleDataSource_WithWhere()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .AndWhere( s => s["foo"]["a"] < SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["foo"]["a"].Assign( SqlNode.Literal( "bar" ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"UPDATE foo SET
  `a` = 'bar'
WHERE foo.`a` < 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateSimpleDataSource_WithWhereAndAlias()
    {
        var dataSource = CreateTable( string.Empty, "foo", "a" )
            .ToRecordSet( "bar" )
            .ToDataSource()
            .AndWhere( s => s["bar"]["a"] < SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["bar"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"UPDATE `foo` AS `bar` SET
  `a` = 10
WHERE `bar`.`a` < 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateSingleDataSource_WithCteAndWhereAndOrderByAndLimit()
    {
        var dataSource = CreateTable( string.Empty, "foo", "a" )
            .RecordSet
            .ToDataSource()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["foo"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .OrderBy( s => new[] { s["foo"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["foo"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
UPDATE `foo` SET
  `a` = 10
WHERE `foo`.`a` IN (
  SELECT cba.c FROM cba
)
ORDER BY `foo`.`a` ASC
LIMIT 5" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateSingleDataSource_WithAllTraits()
    {
        var table = CreateTable( "s", "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .ToDataSource()
            .Distinct()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] > SqlNode.Literal( 20 ) )
            .Window( SqlNode.WindowDefinition( "wnd", Array.Empty<SqlOrderByNode>() ) )
            .OrderBy( s => new[] { s["f"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) )
            .Offset( SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
UPDATE `s`.`foo` SET
  `a` = 10
WHERE `s`.`foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT DISTINCT
      `f`.`a`
    FROM `s`.`foo` AS `f`
    WHERE `f`.`a` IN (
      SELECT cba.c FROM cba
    )
    GROUP BY `f`.`b`
    HAVING `f`.`b` > 20
    WINDOW `wnd` AS ()
    ORDER BY `f`.`a` ASC
    LIMIT 5 OFFSET 10
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateSimpleDataSource_WithSubQueryInAssignment()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[]
                {
                    s["foo"]["b"]
                        .Assign(
                            SqlNode.RawRecordSet( "bar" )
                                .ToDataSource()
                                .AndWhere( b => b.From["x"] == s["foo"]["a"] )
                                .Select( b => new[] { b.From["y"].AsSelf() } ) )
                } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"UPDATE foo SET
  `b` = (
    SELECT
      bar.`y`
    FROM bar
    WHERE bar.`x` = foo.`a`
  )" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateSimpleDataSource_WithDataSourceFieldsAsAssignedValues()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[]
                {
                    s["foo"]["a"].Assign( s["foo"]["a"] + SqlNode.Literal( 1 ) ),
                    s["foo"]["b"].Assign( s["foo"]["c"] * s["foo"]["d"] )
                } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"UPDATE foo SET
  `a` = (foo.`a` + 1),
  `b` = (foo.`c` * foo.`d`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateMultiDataSource_WithoutTraits()
    {
        var foo = CreateTable( string.Empty, "foo", "a" ).ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );
        var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"UPDATE `foo` AS `f`
INNER JOIN bar ON `f`.`a` = bar.`a` SET
  `f`.`a` = 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateMultiDataSource_WithCteAndWhere()
    {
        var foo = CreateTable( string.Empty, "foo", "a" ).ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar", "b" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"] < SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
UPDATE `foo` AS `f`
INNER JOIN bar AS `b` ON `f`.`a` = `b`.`a` SET
  `f`.`a` = 10
WHERE `f`.`a` < 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateMultiDataSource_WithCteAndWhereAndOrderByAndLimit()
    {
        var table = CreateTable( "s", "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .OrderBy( s => new[] { s["f"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
UPDATE `s`.`foo` SET
  `a` = 10
WHERE `s`.`foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM `s`.`foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    WHERE `f`.`a` IN (
      SELECT cba.c FROM cba
    )
    ORDER BY `f`.`a` ASC
    LIMIT 5
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateMultiDataSource_WithAllTraits()
    {
        var table = CreateTable( "s", "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .Distinct()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] > SqlNode.Literal( 20 ) )
            .Window( SqlNode.WindowDefinition( "wnd", Array.Empty<SqlOrderByNode>() ) )
            .OrderBy( s => new[] { s["f"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) )
            .Offset( SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
UPDATE `s`.`foo` SET
  `a` = 10
WHERE `s`.`foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT DISTINCT
      `f`.`a`
    FROM `s`.`foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    WHERE `f`.`a` IN (
      SELECT cba.c FROM cba
    )
    GROUP BY `f`.`b`
    HAVING `f`.`b` > 20
    WINDOW `wnd` AS ()
    ORDER BY `f`.`a` ASC
    LIMIT 5 OFFSET 10
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableWithSingleColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"UPDATE `foo` SET
  `a` = 10
WHERE `foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableWithMultiColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"UPDATE `foo` SET
  `a` = 10
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`a`) AND (`foo`.`b` = `_{GUID}`.`b`)
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableBuilderWithSingleColumnPrimaryKey()
    {
        var table = CreateTableBuilder<int>( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"UPDATE `foo` SET
  `a` = 10
WHERE `foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableBuilderWithMultiColumnPrimaryKey()
    {
        var table = CreateTableBuilder( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"UPDATE `foo` SET
  `a` = 10
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`a`) AND (`foo`.`b` = `_{GUID}`.`b`)
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsTableBuilderWithoutPrimaryKey()
    {
        var table = CreateTableBuilderWithoutPrimaryKey( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .AndWhere( s => s["f"]["a"] > SqlNode.Literal( 10 ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"UPDATE `foo` SET
  `a` = 10
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    WHERE `f`.`a` > 10
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`a`) AND (`foo`.`b` = `_{GUID}`.`b`)
);" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsNewTableWithSingleColumnPrimaryKey(
        bool isTemporary,
        string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var table = SqlNode.CreateTable(
            info,
            new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) },
            constraintsProvider: t => SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK", t["a"].Asc() ) ) );

        var foo = table.RecordSet.As( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                $@"UPDATE {expectedName} SET
  `a` = 10
WHERE {expectedName}.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM {expectedName} AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{{GUID}}`
);" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsNewTableWithMultiColumnPrimaryKey(
        bool isTemporary,
        string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var table = SqlNode.CreateTable(
            info,
            new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) },
            constraintsProvider: t =>
                SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK", t["a"].Asc(), t["b"].Asc() ) ) );

        var foo = table.RecordSet.As( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                $@"UPDATE {expectedName} SET
  `a` = 10
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM {expectedName} AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{{GUID}}`
  WHERE ({expectedName}.`a` = `_{{GUID}}`.`a`) AND ({expectedName}.`b` = `_{{GUID}}`.`b`)
);" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetIsNewTableWithoutPrimaryKey(bool isTemporary, string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } );
        var foo = table.RecordSet.As( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                $@"UPDATE {expectedName} SET
  `a` = 10
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM {expectedName} AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{{GUID}}`
  WHERE ({expectedName}.`a` = `_{{GUID}}`.`a`) AND ({expectedName}.`b` = `_{{GUID}}`.`b`)
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetHasSingleColumnPrimaryKey_WithDataSourceFieldsAsAssignedValues()
    {
        var table = CreateTableBuilder<int>( string.Empty, "foo", new[] { "a", "b", "c", "d" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[]
                {
                    s["f"]["a"]
                        .Assign(
                            table.ToRecordSet( "x" )
                                .ToDataSource()
                                .AndWhere( t => t["x"]["a"] > dataSource["f"]["a"] )
                                .Select( t => new[] { t["x"]["b"].AsSelf() } ) ),
                    s["f"]["b"].Assign( s["f"]["c"] * s["f"]["d"] )
                } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"UPDATE `foo` SET
  `a` = (
    SELECT
      `x`.`b`
    FROM `foo` AS `x`
    WHERE `x`.`a` > `foo`.`a`
  ),
  `b` = (`foo`.`c` * `foo`.`d`)
WHERE `foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WhenTargetHasMultiColumnPrimaryKey_WithDataSourceFieldsAsAssignedValues()
    {
        var table = CreateTableBuilder<int>( string.Empty, "foo", new[] { "a", "b", "c", "d" }, "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[]
                {
                    s["f"]["a"]
                        .Assign(
                            table.ToRecordSet( "x" )
                                .ToDataSource()
                                .AndWhere( t => t["x"]["a"] > dataSource["f"]["a"] )
                                .Select( t => new[] { t["x"]["b"].AsSelf() } ) ),
                    s["f"]["b"].Assign( s["f"]["c"] * s["f"]["d"] )
                } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"UPDATE `foo` SET
  `a` = (
    SELECT
      `x`.`b`
    FROM `foo` AS `x`
    WHERE `x`.`a` > `foo`.`a`
  ),
  `b` = (`foo`.`c` * `foo`.`d`)
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`a`) AND (`foo`.`b` = `_{GUID}`.`b`)
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WithComplexAssignmentAndSingleColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["b"].Assign( s["f"]["b"] + s["bar"]["b"] ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `_{GUID}` AS (
  SELECT
    `f`.`a` AS `ID_a_0`,
    (`f`.`b` + bar.`b`) AS `VAL_b_0`
  FROM `foo` AS `f`
  INNER JOIN bar ON bar.`a` = `f`.`a`
  GROUP BY `f`.`b`
)
UPDATE `foo` SET
  `b` = (
    SELECT
      `_{GUID}`.`VAL_b_0`
    FROM `_{GUID}`
    WHERE `foo`.`a` = `_{GUID}`.`ID_a_0`
    LIMIT 1
  )
WHERE `foo`.`a` IN (
  SELECT
    `_{GUID}`.`ID_a_0`
    FROM `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WithComplexAssignmentAndMultipleColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b", "c" }, "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["c"].Assign( s["f"]["c"] + s["bar"]["c"] ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `_{GUID}` AS (
  SELECT
    `f`.`a` AS `ID_a_0`,
    `f`.`b` AS `ID_b_1`,
    (`f`.`c` + bar.`c`) AS `VAL_c_0`
  FROM `foo` AS `f`
  INNER JOIN bar ON bar.`a` = `f`.`a`
  GROUP BY `f`.`b`
)
UPDATE `foo` SET
  `c` = (
    SELECT
      `_{GUID}`.`VAL_c_0`
    FROM `_{GUID}`
    WHERE (`foo`.`a` = `_{GUID}`.`ID_a_0`) AND (`foo`.`b` = `_{GUID}`.`ID_b_1`)
    LIMIT 1
  )
WHERE EXISTS (
  SELECT
    *
  FROM `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`ID_a_0`) AND (`foo`.`b` = `_{GUID}`.`ID_b_1`)
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateComplexDataSource_WithCteAndComplexAssignment()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var view = CreateView(
                string.Empty,
                "v",
                SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf(), s.From["b"].AsSelf() } ) )
            .RecordSet;

        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( view, x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM lorem" ).ToCte( "ipsum" ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["b"].Assign( s["f"]["b"] + s["v"]["b"] ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `ipsum` AS (
  SELECT * FROM lorem
),
`_{GUID}` AS (
  SELECT
    `f`.`a` AS `ID_a_0`,
    (`f`.`b` + `v`.`b`) AS `VAL_b_0`
  FROM `foo` AS `f`
  INNER JOIN `v` ON `v`.`a` = `f`.`a`
  GROUP BY `f`.`b`
)
UPDATE `foo` SET
  `b` = (
    SELECT
      `_{GUID}`.`VAL_b_0`
    FROM `_{GUID}`
    WHERE `foo`.`a` = `_{GUID}`.`ID_a_0`
    LIMIT 1
  )
WHERE `foo`.`a` IN (
  SELECT
    `_{GUID}`.`ID_a_0`
  FROM `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexAssignments_WithMultipleAssignments()
    {
        var table = CreateTable( "s", "foo", new[] { "a", "b", "c" }, "a" );
        var subQuery = SqlNode.RawRecordSet( "U" )
            .ToDataSource()
            .Select( x => new[] { x.From["x"].AsSelf(), x.From["y"].AsSelf() } )
            .AsSet( "lorem" );

        var foo = table.ToRecordSet( "f" );
        var dataSource = foo.Join( SqlJoinDefinition.Inner( subQuery, x => x.Inner["x"] == foo["b"] ) )
            .GroupBy( s => new[] { s["lorem"]["y"] } );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[]
                {
                    s["f"]["b"].Assign( s["lorem"]["x"] + SqlNode.Literal( 1 ) ),
                    s["f"]["c"].Assign( s["f"]["c"] * s["lorem"]["y"] )
                } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `_{GUID}` AS (
  SELECT
    `f`.`a` AS `ID_a_0`,
    (`lorem`.`x` + 1) AS `VAL_b_0`,
    (`f`.`c` * `lorem`.`y`) AS `VAL_c_1`
  FROM `s`.`foo` AS `f`
  INNER JOIN (
    SELECT
      U.`x`,
      U.`y`
    FROM U
  ) AS `lorem` ON `lorem`.`x` = `f`.`b`
  GROUP BY `lorem`.`y`
)
UPDATE `s`.`foo` SET
  `b` = (
    SELECT
      `_{GUID}`.`VAL_b_0`
    FROM `_{GUID}`
    WHERE `s`.`foo`.`a` = `_{GUID}`.`ID_a_0`
    LIMIT 1
  ),
  `c` = (
    SELECT
      `_{GUID}`.`VAL_c_1`
    FROM `_{GUID}`
    WHERE `s`.`foo`.`a` = `_{GUID}`.`ID_a_0`
    LIMIT 1
  )
WHERE `s`.`foo`.`a` IN (
  SELECT
    `_{GUID}`.`ID_a_0`
  FROM `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNotTable()
    {
        var node = SqlNode.RawRecordSet( "foo" ).ToDataSource().GroupBy( s => new[] { s["foo"]["x"] } ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsTableWithoutAlias()
    {
        var foo = CreateTable( string.Empty, "foo" ).ToRecordSet();
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsTableBuilderWithoutColumns()
    {
        var foo = CreateTableBuilderWithoutPrimaryKey( string.Empty, "foo" ).ToRecordSet( "f" );
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void
        Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNewTableWithoutPrimaryKeyAndColumns()
    {
        var foo = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() ).AsSet( "f" );
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void
        Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyWithoutColumns()
    {
        var foo = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "foo" ),
                new[] { SqlNode.Column<int>( "a" ) },
                constraintsProvider: _ => SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK" ) ) )
            .AsSet( "f" );

        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void
        Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyContainingNonDataFieldColumn()
    {
        var foo = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "foo" ),
                new[] { SqlNode.Column<int>( "a" ) },
                constraintsProvider: t =>
                    SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK", (t["a"] + SqlNode.Literal( 1 )).Asc() ) ) )
            .AsSet( "f" );

        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).GroupBy( s => new[] { s["bar"]["x"] } ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretValueAssignment()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" )["a"].Assign( SqlNode.Literal( 50 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "`a` = 50" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithoutTraits()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        _sut.Visit( dataSource.ToDeleteFrom() );
        _sut.Context.Sql.ToString().Should().Be( "DELETE FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithWhere()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .AndWhere( s => s["foo"]["a"] < SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"DELETE FROM foo
WHERE foo.`a` < 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromSimpleDataSource_WithWhereAndAlias()
    {
        var dataSource = SqlNode.RawRecordSet( "foo", "bar" )
            .ToDataSource()
            .AndWhere( s => s["bar"]["a"] < SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"DELETE `bar`
FROM foo AS `bar`
WHERE `bar`.`a` < 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithCteAndWhereAndOrderByAndLimit()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["foo"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .OrderBy( s => new[] { s["foo"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
DELETE FROM foo
WHERE foo.`a` IN (
  SELECT cba.c FROM cba
)
ORDER BY foo.`a` ASC
LIMIT 5" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromSingleDataSource_WithAllTraits()
    {
        var table = CreateTable( "s", "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .ToDataSource()
            .Distinct()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] > SqlNode.Literal( 20 ) )
            .Window( SqlNode.WindowDefinition( "wnd", Array.Empty<SqlOrderByNode>() ) )
            .OrderBy( s => new[] { s["f"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) )
            .Offset( SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
DELETE FROM `s`.`foo`
WHERE `s`.`foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT DISTINCT
      `f`.`a`
    FROM `s`.`foo` AS `f`
    WHERE `f`.`a` IN (
      SELECT cba.c FROM cba
    )
    GROUP BY `f`.`b`
    HAVING `f`.`b` > 20
    WINDOW `wnd` AS ()
    ORDER BY `f`.`a` ASC
    LIMIT 5 OFFSET 10
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithoutTraits()
    {
        var foo = SqlNode.RawRecordSet( "foo" );
        var other = SqlNode.RawRecordSet( "bar" );
        var dataSource = foo.Join( other.InnerOn( foo["a"] == other["a"] ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"DELETE foo
FROM foo
INNER JOIN bar ON foo.`a` = bar.`a`" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithCteAndWhere()
    {
        var foo = SqlNode.RawRecordSet( "foo", "f" );
        var other = SqlNode.RawRecordSet( "bar", "b" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"] < SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
DELETE `f`
FROM foo AS `f`
INNER JOIN bar AS `b` ON `f`.`a` = `b`.`a`
WHERE `f`.`a` < 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithCteAndWhereAndOrderByAndLimit()
    {
        var table = CreateTable( "s", "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .OrderBy( s => new[] { s["f"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
DELETE FROM `s`.`foo`
WHERE `s`.`foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM `s`.`foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    WHERE `f`.`a` IN (
      SELECT cba.c FROM cba
    )
    ORDER BY `f`.`a` ASC
    LIMIT 5
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromMultiDataSource_WithAllTraits()
    {
        var table = CreateTable( "s", "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .Distinct()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["f"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] > SqlNode.Literal( 20 ) )
            .Window( SqlNode.WindowDefinition( "wnd", Array.Empty<SqlOrderByNode>() ) )
            .OrderBy( s => new[] { s["f"]["a"].Asc() } )
            .Limit( SqlNode.Literal( 5 ) )
            .Offset( SqlNode.Literal( 10 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"WITH `cba` AS (
  SELECT * FROM abc
)
DELETE FROM `s`.`foo`
WHERE `s`.`foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT DISTINCT
      `f`.`a`
    FROM `s`.`foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    WHERE `f`.`a` IN (
      SELECT cba.c FROM cba
    )
    GROUP BY `f`.`b`
    HAVING `f`.`b` > 20
    WINDOW `wnd` AS ()
    ORDER BY `f`.`a` ASC
    LIMIT 5 OFFSET 10
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableWithSingleColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"DELETE FROM `foo`
WHERE `foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableWithMultiColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"DELETE FROM `foo`
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`a`) AND (`foo`.`b` = `_{GUID}`.`b`)
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableBuilderWithSingleColumnPrimaryKey()
    {
        var table = CreateTableBuilder<int>( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"DELETE FROM `foo`
WHERE `foo`.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableBuilderWithMultiColumnPrimaryKey()
    {
        var table = CreateTableBuilder( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"DELETE FROM `foo`
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`a`) AND (`foo`.`b` = `_{GUID}`.`b`)
);" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsTableBuilderWithoutPrimaryKey()
    {
        var table = CreateTableBuilderWithoutPrimaryKey( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .AndWhere( s => s["f"]["a"] > SqlNode.Literal( 10 ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                @"DELETE FROM `foo`
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM `foo` AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    WHERE `f`.`a` > 10
    GROUP BY `f`.`b`
  ) AS `_{GUID}`
  WHERE (`foo`.`a` = `_{GUID}`.`a`) AND (`foo`.`b` = `_{GUID}`.`b`)
);" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsNewTableWithSingleColumnPrimaryKey(
        bool isTemporary,
        string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var table = SqlNode.CreateTable(
            info,
            new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) },
            constraintsProvider: t => SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK", t["a"].Asc() ) ) );

        var foo = table.RecordSet.As( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                $@"DELETE FROM {expectedName}
WHERE {expectedName}.`a` IN (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`
    FROM {expectedName} AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{{GUID}}`
);" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsNewTableWithMultiColumnPrimaryKey(
        bool isTemporary,
        string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var table = SqlNode.CreateTable(
            info,
            new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) },
            constraintsProvider: t =>
                SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK", t["a"].Asc(), t["b"].Asc() ) ) );

        var foo = table.RecordSet.As( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                $@"DELETE FROM {expectedName}
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM {expectedName} AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{{GUID}}`
  WHERE ({expectedName}.`a` = `_{{GUID}}`.`a`) AND ({expectedName}.`b` = `_{{GUID}}`.`b`)
);" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretDeleteFromComplexDataSource_WhenTargetIsNewTableWithoutPrimaryKey(
        bool isTemporary,
        string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } );
        var foo = table.RecordSet.As( "f" );
        var other = SqlNode.RawRecordSet( "bar" );

        var dataSource = foo
            .Join( other.InnerOn( foo["a"] == other["a"] ) )
            .GroupBy( s => new[] { s["f"]["b"] } );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .SatisfySql(
                $@"DELETE FROM {expectedName}
WHERE EXISTS (
  SELECT
    *
  FROM (
    SELECT
      `f`.`a`,
      `f`.`b`
    FROM {expectedName} AS `f`
    INNER JOIN bar ON `f`.`a` = bar.`a`
    GROUP BY `f`.`b`
  ) AS `_{{GUID}}`
  WHERE ({expectedName}.`a` = `_{{GUID}}`.`a`) AND ({expectedName}.`b` = `_{{GUID}}`.`b`)
);" );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNotTable()
    {
        var foo = SqlNode.RawRecordSet( "foo" );
        var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsTableWithoutAlias()
    {
        var foo = CreateTable( string.Empty, "foo" ).ToRecordSet();
        var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsTableBuilderWithoutColumns()
    {
        var foo = CreateTableBuilderWithoutPrimaryKey( string.Empty, "foo" ).ToRecordSet( "f" );
        var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void
        Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNewTableWithoutPrimaryKeyAndColumns()
    {
        var foo = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() ).AsSet( "f" );
        var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void
        Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyWithoutColumns()
    {
        var foo = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "foo" ),
                new[] { SqlNode.Column<int>( "a" ) },
                constraintsProvider: _ => SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK" ) ) )
            .AsSet( "f" );

        var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void
        Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNewTableWithPrimaryKeyContainingNonDataFieldColumn()
    {
        var foo = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "foo" ),
                new[] { SqlNode.Column<int>( "a" ) },
                constraintsProvider: t =>
                    SqlCreateTableConstraints.Empty.WithPrimaryKey( SqlNode.PrimaryKey( "PK", (t["a"] + SqlNode.Literal( 1 )).Asc() ) ) )
            .AsSet( "f" );

        var node = foo.ToDataSource().GroupBy( foo.GetUnsafeField( "a" ) ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretTruncate()
    {
        var table = CreateTable( "foo", "bar" );
        var node = table.ToRecordSet().ToTruncate();

        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( "TRUNCATE TABLE `foo`.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnDefinition_WhenNullable()
    {
        _sut.Visit( SqlNode.Column<int>( "a", isNullable: true ) );
        _sut.Context.Sql.ToString().Should().Be( "`a` INT" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnDefinition_WhenNonNullable()
    {
        _sut.Visit( SqlNode.Column<string>( "a", isNullable: false ) );
        _sut.Context.Sql.ToString().Should().Be( "`a` LONGTEXT NOT NULL" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnDefinition_WithDefaultLiteralValue()
    {
        _sut.Visit( SqlNode.Column<int>( "a", isNullable: false, defaultValue: SqlNode.Literal( 10 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "`a` INT NOT NULL DEFAULT 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnDefinition_WithDefaultNullValue()
    {
        _sut.Visit( SqlNode.Column<int>( "a", isNullable: true, defaultValue: SqlNode.Null() ) );
        _sut.Context.Sql.ToString().Should().Be( "`a` INT DEFAULT NULL" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnDefinition_WithDefaultExpressionValue()
    {
        _sut.Visit( SqlNode.Column<int>( "a", isNullable: false, defaultValue: SqlNode.Literal( 10 ) + SqlNode.Literal( 20 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "`a` INT NOT NULL DEFAULT (10 + 20)" );
    }

    [Theory]
    [InlineData( typeof( TypeMock<IList<char>> ), "TINYTEXT" )]
    [InlineData( typeof( TypeMock<char[]> ), "TEXT" )]
    [InlineData( typeof( TypeMock<IReadOnlyList<char>> ), "MEDIUMTEXT" )]
    [InlineData( typeof( string ), "LONGTEXT" )]
    [InlineData( typeof( TypeMock<IList<byte>> ), "TINYBLOB" )]
    [InlineData( typeof( TypeMock<IEnumerable<byte>> ), "BLOB" )]
    [InlineData( typeof( TypeMock<IReadOnlyList<byte>> ), "MEDIUMBLOB" )]
    [InlineData( typeof( byte[] ), "LONGBLOB" )]
    public void Visit_ShouldInterpretColumnDefinition_WithDefaultLiteralValueAsExpression(Type type, string expectedType)
    {
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IList<char>>>( MySqlDbType.TinyText ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<char[]>>( MySqlDbType.Text ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IReadOnlyList<char>>>( MySqlDbType.MediumText ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IList<byte>>>( MySqlDbType.TinyBlob ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IEnumerable<byte>>>( MySqlDbType.Blob ) );
        _typeDefinitions.RegisterDefinition( new TypeDefinition<TypeMock<IReadOnlyList<byte>>>( MySqlDbType.MediumBlob ) );

        _sut.Visit( SqlNode.Column( "a", TypeNullability.Create( type, isNullable: false ), defaultValue: SqlNode.Literal( "foo" ) ) );
        _sut.Context.Sql.ToString().Should().Be( $"`a` {expectedType} NOT NULL DEFAULT ('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretPrimaryKeyDefinition()
    {
        var table = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "foo", "bar" ),
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } )
            .RecordSet;

        var node = SqlNode.PrimaryKey( "PK_foobar", table["a"].Asc(), table["b"].Desc() );
        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( "CONSTRAINT `PK_foobar` PRIMARY KEY (`foo`.`bar`.`a` ASC, `foo`.`bar`.`b` DESC)" );
    }

    [Theory]
    [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Restrict )]
    [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Restrict )]
    public void Visit_ShouldInterpretForeignKeyDefinition(ReferenceBehavior.Values onDelete, ReferenceBehavior.Values onUpdate)
    {
        var onDeleteBehavior = onDelete == ReferenceBehavior.Values.Cascade ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;
        var onUpdateBehavior = onUpdate == ReferenceBehavior.Values.Cascade ? ReferenceBehavior.Cascade : ReferenceBehavior.Restrict;

        var qux = CreateTable( string.Empty, "qux", "a", "b" ).ToRecordSet();
        var table = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "foo", "bar" ),
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } )
            .RecordSet;

        var node = SqlNode.ForeignKey(
            "FK_foobar_REF_qux",
            new SqlDataFieldNode[] { table["a"], table["b"] },
            qux,
            new SqlDataFieldNode[] { qux["a"], qux["b"] },
            onDeleteBehavior,
            onUpdateBehavior );

        _sut.Visit( node );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                $"CONSTRAINT `FK_foobar_REF_qux` FOREIGN KEY (`a`, `b`) REFERENCES `qux` (`a`, `b`) ON DELETE {onDeleteBehavior.Name} ON UPDATE {onUpdateBehavior.Name}" );
    }

    [Fact]
    public void Visit_ShouldInterpretCheckDefinition()
    {
        var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo", "bar" ), new[] { SqlNode.Column<int>( "a" ) } ).RecordSet;
        var node = SqlNode.Check( "CHK_foobar", table["a"] > SqlNode.Literal( 10 ) );
        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( "CONSTRAINT `CHK_foobar` CHECK (`foo`.`bar`.`a` > 10)" );
    }

    [Theory]
    [InlineData( false, false, "`foo`.`bar`" )]
    [InlineData( true, false, "`foo`" )]
    [InlineData( false, true, "`foo`.`bar`" )]
    [InlineData( true, true, "`foo`" )]
    public void Visit_ShouldInterpretCreateTable(bool isTemporary, bool ifNotExists, string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var node = SqlNode.CreateTable(
            info,
            new[]
            {
                SqlNode.Column<int>( "x" ),
                SqlNode.Column<string>( "y", isNullable: true ),
                SqlNode.Column<double>( "z", defaultValue: SqlNode.Literal( 10.5 ) )
            },
            ifNotExists: ifNotExists,
            constraintsProvider: t =>
            {
                var qux = SqlNode.RawRecordSet( "qux" );
                return SqlCreateTableConstraints.Empty
                    .WithPrimaryKey( SqlNode.PrimaryKey( "PK_foobar", t["x"].Asc() ) )
                    .WithForeignKeys(
                        SqlNode.ForeignKey(
                            "FK_foobar_REF_qux",
                            new SqlDataFieldNode[] { t["y"] },
                            qux,
                            new SqlDataFieldNode[] { qux["y"] } ) )
                    .WithChecks( SqlNode.Check( "CHK_foobar", t["z"] > SqlNode.Literal( 100.0 ) ) );
            } );

        _sut.Visit( node );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                $@"CREATE{(isTemporary ? " TEMPORARY" : string.Empty)} TABLE{(ifNotExists ? " IF NOT EXISTS" : string.Empty)} {expectedName} (
  `x` INT NOT NULL,
  `y` LONGTEXT,
  `z` DOUBLE NOT NULL DEFAULT 10.5,
  CONSTRAINT `PK_foobar` PRIMARY KEY (`x` ASC),
  CONSTRAINT `FK_foobar_REF_qux` FOREIGN KEY (`y`) REFERENCES qux (`y`) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CONSTRAINT `CHK_foobar` CHECK (`z` > 100.0)
)" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretCreateView(bool isTemporary, string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var node = SqlNode.CreateView( info, replaceIfExists: false, source: SqlNode.RawQuery( "SELECT * FROM qux" ) );
        _sut.Visit( node );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                $@"CREATE VIEW {expectedName} AS
SELECT * FROM qux" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`" )]
    [InlineData( true, "`foo`" )]
    public void Visit_ShouldInterpretCreateView_WithReplaceIfExists(bool isTemporary, string expectedName)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var node = SqlNode.CreateView( info, replaceIfExists: true, source: SqlNode.RawQuery( "SELECT * FROM qux" ) );
        _sut.Visit( node );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                $@"CREATE OR REPLACE VIEW {expectedName} AS
SELECT * FROM qux" );
    }

    [Theory]
    [InlineData( false, "INDEX" )]
    [InlineData( true, "UNIQUE INDEX" )]
    public void Visit_ShouldInterpretCreateIndex(bool isUnique, string expectedType)
    {
        var qux = CreateTable( "foo", "qux", new[] { "a", "b" }, "a" ).RecordSet;
        var node = SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( "foo", "bar" ),
            isUnique: isUnique,
            replaceIfExists: false,
            table: qux,
            columns: new[] { qux["a"].Asc(), qux["b"].Desc() } );

        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( $"CREATE {expectedType} `bar` ON `foo`.`qux` (`a` ASC, `b` DESC)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCreateIndex_WithTemporaryTable()
    {
        var qux = SqlNode.CreateTable(
                SqlRecordSetInfo.CreateTemporary( "qux" ),
                new[] { SqlNode.Column<int>( "a" ), SqlNode.Column<int>( "b" ) } )
            .RecordSet;

        var node = SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( "bar" ),
            isUnique: false,
            replaceIfExists: false,
            table: qux,
            columns: new[] { qux["a"].Asc(), qux["b"].Desc() } );

        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( "CREATE INDEX `bar` ON `qux` (`a` ASC, `b` DESC)" );
    }

    [Theory]
    [InlineData( false, "INDEX" )]
    [InlineData( true, "UNIQUE INDEX" )]
    public void Visit_ShouldInterpretCreateIndex_WithReplaceIfExists(bool isUnique, string expectedType)
    {
        var qux = CreateTable( "foo", "qux", new[] { "a", "b" }, "a" ).RecordSet;
        var node = SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( "foo", "bar" ),
            isUnique: isUnique,
            replaceIfExists: true,
            table: qux,
            columns: new[] { qux["a"].Asc(), qux["b"].Desc() } );

        _sut.Visit( node );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                $@"CALL `_DROP_INDEX_IF_EXISTS`('foo', 'qux', 'bar');
CREATE {expectedType} `bar` ON `foo`.`qux` (`a` ASC, `b` DESC)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCreateIndex_WithFilter()
    {
        var qux = SqlNode.RawRecordSet( "qux" );
        var node = SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( "bar" ),
            isUnique: false,
            replaceIfExists: false,
            table: qux,
            columns: new[] { qux["a"].Asc(), qux["b"].Desc() },
            filter: qux["a"] != null );

        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( "CREATE INDEX `bar` ON qux (`a` ASC, `b` DESC) WHERE (`a` IS NOT NULL)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCreateIndex_WithPrefixedColumnType()
    {
        var qux = CreateTableBuilder<string>( "foo", "qux", new[] { "a" }, "a" ).RecordSet;
        var node = SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( "foo", "bar" ),
            isUnique: false,
            replaceIfExists: false,
            table: qux,
            columns: new[] { qux["a"].Asc() } );

        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( "CREATE INDEX `bar` ON `foo`.`qux` (`a`(500) ASC)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCreateIndex_WithExpression()
    {
        var qux = SqlNode.RawRecordSet( "qux" );
        var node = SqlNode.CreateIndex(
            SqlSchemaObjectName.Create( "foo", "bar" ),
            isUnique: false,
            replaceIfExists: false,
            table: qux,
            columns: new[] { (qux["a"] + qux["b"]).Asc() } );

        _sut.Visit( node );

        _sut.Context.Sql.ToString().Should().Be( "CREATE INDEX `bar` ON qux ((`a` + `b`) ASC)" );
    }

    [Theory]
    [InlineData( false, "ALTER TABLE `foo`.`bar` RENAME TO `qux`.`lorem`" )]
    [InlineData( true, "ALTER TABLE `foo` RENAME TO `qux`.`lorem`" )]
    public void Visit_ShouldInterpretRenameTable(bool isTemporary, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.RenameTable( info, SqlSchemaObjectName.Create( "qux", "lorem" ) ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "ALTER TABLE `foo`.`bar` RENAME COLUMN `qux` TO `lorem`" )]
    [InlineData( true, "ALTER TABLE `foo` RENAME COLUMN `qux` TO `lorem`" )]
    public void Visit_ShouldInterpretRenameColumn(bool isTableTemporary, string expected)
    {
        var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.RenameColumn( info, "qux", "lorem" ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "ALTER TABLE `foo`.`bar` ADD COLUMN `qux` INT NOT NULL DEFAULT 10" )]
    [InlineData( true, "ALTER TABLE `foo` ADD COLUMN `qux` INT NOT NULL DEFAULT 10" )]
    public void Visit_ShouldInterpretAddColumn(bool isTableTemporary, string expected)
    {
        var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.AddColumn( info, SqlNode.Column<int>( "qux", defaultValue: SqlNode.Literal( 10 ) ) ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "ALTER TABLE `foo`.`bar` DROP COLUMN `qux`" )]
    [InlineData( true, "ALTER TABLE `foo` DROP COLUMN `qux`" )]
    public void Visit_ShouldInterpretDropColumn(bool isTableTemporary, string expected)
    {
        var info = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.DropColumn( info, "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, false, "DROP TABLE `foo`.`bar`" )]
    [InlineData( true, false, "DROP TEMPORARY TABLE `foo`" )]
    [InlineData( false, true, "DROP TABLE IF EXISTS `foo`.`bar`" )]
    [InlineData( true, true, "DROP TEMPORARY TABLE IF EXISTS `foo`" )]
    public void Visit_ShouldInterpretDropTable(bool isTemporary, bool ifExists, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.DropTable( info, ifExists ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, false, "DROP VIEW `foo`.`bar`" )]
    [InlineData( true, false, "DROP VIEW `foo`" )]
    [InlineData( false, true, "DROP VIEW IF EXISTS `foo`.`bar`" )]
    [InlineData( true, true, "DROP VIEW IF EXISTS `foo`" )]
    public void Visit_ShouldInterpretDropView(bool isTemporary, bool ifExists, string expected)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        _sut.Visit( SqlNode.DropView( info, ifExists ) );
        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, false, "DROP INDEX `bar` ON `foo`.`qux`" )]
    [InlineData( true, false, "CALL `_DROP_INDEX_IF_EXISTS`('foo', 'qux', 'bar')" )]
    [InlineData( false, true, "DROP INDEX `bar` ON `qux`" )]
    [InlineData( true, true, "CALL `_DROP_INDEX_IF_EXISTS`(NULL, 'qux', 'bar')" )]
    public void Visit_ShouldInterpretDropIndex(bool ifExists, bool isRecordSetTemporary, string expected)
    {
        var recordSet = isRecordSetTemporary ? SqlRecordSetInfo.CreateTemporary( "qux" ) : SqlRecordSetInfo.Create( "foo", "qux" );
        var name = isRecordSetTemporary ? SqlSchemaObjectName.Create( "bar" ) : SqlSchemaObjectName.Create( "foo", "bar" );

        _sut.Visit( SqlNode.DropIndex( recordSet, name, ifExists ) );

        _sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Fact]
    public void Visit_ShouldInterpretStatementBatch()
    {
        _sut.Visit(
            SqlNode.Batch(
                SqlNode.BeginTransaction( IsolationLevel.Serializable ),
                SqlNode.Batch( SqlNode.DropTable( SqlRecordSetInfo.CreateTemporary( "bar" ) ) ),
                SqlNode.RawQuery( "SELECT * FROM foo" ),
                SqlNode.RawQuery( "SELECT * FROM qux" ),
                SqlNode.CommitTransaction() ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"SET SESSION TRANSACTION ISOLATION LEVEL SERIALIZABLE;
START TRANSACTION;

DROP TEMPORARY TABLE `bar`;

SELECT * FROM foo;

SELECT * FROM qux;

COMMIT;" );
    }

    [Theory]
    [InlineData( IsolationLevel.Unspecified, "REPEATABLE READ", "START TRANSACTION" )]
    [InlineData( IsolationLevel.Chaos, "REPEATABLE READ", "START TRANSACTION" )]
    [InlineData( IsolationLevel.ReadUncommitted, "READ UNCOMMITTED", "START TRANSACTION" )]
    [InlineData( IsolationLevel.ReadCommitted, "READ COMMITTED", "START TRANSACTION" )]
    [InlineData( IsolationLevel.RepeatableRead, "REPEATABLE READ", "START TRANSACTION" )]
    [InlineData( IsolationLevel.Serializable, "SERIALIZABLE", "START TRANSACTION" )]
    [InlineData( IsolationLevel.Snapshot, "REPEATABLE READ", "START TRANSACTION WITH CONSISTENT SNAPSHOT" )]
    public void Visit_ShouldInterpretBeginTransaction(
        IsolationLevel isolationLevel,
        string expectedSetSession,
        string expectedStartTransaction)
    {
        _sut.Visit( SqlNode.BeginTransaction( isolationLevel ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                $@"SET SESSION TRANSACTION ISOLATION LEVEL {expectedSetSession};
{expectedStartTransaction}" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommitTransaction()
    {
        _sut.Visit( SqlNode.CommitTransaction() );
        _sut.Context.Sql.ToString().Should().Be( "COMMIT" );
    }

    [Fact]
    public void Visit_ShouldInterpretRollbackTransaction()
    {
        _sut.Visit( SqlNode.RollbackTransaction() );
        _sut.Context.Sql.ToString().Should().Be( "ROLLBACK" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenNodeIsCustom()
    {
        var node = new NodeMock();

        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    // TODO:
    // replace all those mocks with MySql objects & remove reference to Sql.Core.Tests
    [Pure]
    private static ISqlTableBuilder CreateTableBuilder(string schemaName, string tableName, params string[] columnNames)
    {
        return CreateTableBuilder<int>( schemaName, tableName, columnNames, Array.Empty<string>() );
    }

    [Pure]
    private static ISqlTableBuilder CreateTableBuilder<TColumnType>(
        string schemaName,
        string tableName,
        string[] columnNames,
        params string[] pkColumnNames)
        where TColumnType : notnull
    {
        if ( columnNames.Length == 0 )
            columnNames = new[] { "X" };

        if ( pkColumnNames.Length == 0 )
            pkColumnNames = columnNames;

        var schema = SchemaMock.CreateBuilder( schemaName );
        var columns = ColumnMock.CreateManyBuilders<TColumnType>( areNullable: true, columnNames );
        var table = TableMock.CreateBuilder(
            tableName,
            schema,
            c => PrimaryKeyMock.CreateBuilder( pkColumnNames.Select( n => c.Get( n ).Asc() ).ToArray() ),
            columns );

        return table;
    }

    [Pure]
    private static ISqlTableBuilder CreateTableBuilderWithoutPrimaryKey(string schemaName, string tableName, params string[] columnNames)
    {
        var schema = SchemaMock.CreateBuilder( schemaName );
        var columns = ColumnMock.CreateManyBuilders<int>( areNullable: true, columnNames );
        var table = TableMock.CreateBuilder( tableName, schema, null, columns );
        return table;
    }

    [Pure]
    private static ISqlTable CreateTable(string schemaName, string tableName, params string[] columnNames)
    {
        return CreateTable( schemaName, tableName, columnNames, Array.Empty<string>() );
    }

    [Pure]
    private static ISqlTable CreateTable(string schemaName, string tableName, string[] columnNames, params string[] pkColumnNames)
    {
        if ( columnNames.Length == 0 )
            columnNames = new[] { "X" };

        if ( pkColumnNames.Length == 0 )
            pkColumnNames = columnNames;

        var schema = SchemaMock.Create( schemaName );
        var columns = ColumnMock.CreateMany<int>( areNullable: true, columnNames );
        var table = TableMock.Create(
            tableName,
            schema,
            c => PrimaryKeyMock.Create( pkColumnNames.Select( n => c.Get( n ).Asc() ).ToArray() ),
            columns );

        return table;
    }

    [Pure]
    private static ISqlViewBuilder CreateViewBuilder(string schemaName, string viewName, SqlQueryExpressionNode? source = null)
    {
        var schema = SchemaMock.CreateBuilder( schemaName );
        var view = ViewMock.CreateBuilder( viewName, schema, source ?? SqlNode.RawQuery( "SELECT * FROM foo" ) );
        return view;
    }

    [Pure]
    private static ISqlView CreateView(string schemaName, string viewName, SqlQueryExpressionNode? source = null)
    {
        var schema = SchemaMock.Create( schemaName );
        var view = ViewMock.Create( viewName, schema, source ?? SqlNode.RawQuery( "SELECT * FROM foo" ) );
        return view;
    }

    private sealed class FunctionMock : SqlFunctionExpressionNode
    {
        public FunctionMock()
            : base( Array.Empty<SqlExpressionNode>() ) { }
    }

    private sealed class AggregateFunctionMock : SqlAggregateFunctionExpressionNode
    {
        public AggregateFunctionMock()
            : base( ReadOnlyMemory<SqlExpressionNode>.Empty, Chain<SqlTraitNode>.Empty ) { }

        public override SqlAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
        {
            return new AggregateFunctionMock();
        }
    }

    private sealed class NodeMock : SqlNodeBase { }

    private sealed class WindowFrameMock : SqlWindowFrameNode
    {
        public WindowFrameMock()
            : base( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.UnboundedFollowing ) { }
    }

    public readonly struct TypeMock<T> { }

    private sealed class TypeDefinition<T> : MySqlColumnTypeDefinition<T>
        where T : struct
    {
        public TypeDefinition(MySqlDbType dbType)
            : this( MySqlDataType.Custom( dbType.ToString().ToUpperInvariant(), dbType, DbType.Object ) ) { }

        public TypeDefinition(MySqlDataType type)
            : base( type, default, (_, _) => default! ) { }

        public override string ToDbLiteral(T value)
        {
            return value.ToString() ?? string.Empty;
        }

        public override object ToParameterValue(T value)
        {
            return value;
        }
    }
}
