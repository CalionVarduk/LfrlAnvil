using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests;

public class SqliteNodeInterpreterTests : TestsBase
{
    private readonly SqliteNodeInterpreter _sut = new SqliteNodeInterpreter(
        new SqliteColumnTypeDefinitionProvider(),
        SqlNodeInterpreterContext.Create() );

    [Fact]
    public void Visit_ShouldInterpretRawExpression()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a + @bar", SqlNode.Parameter<int>( "bar" ) ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Sql.ToString().Should().Be( "foo.a + @bar" );
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "bar", (SqlExpressionType?)SqlExpressionType.Create<int>() ) );
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
        _sut.Context.Sql.ToString().Should().Be( "foo.\"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawDataFieldWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawDataField( SqlNode.RawRecordSet( "foo" ), "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "foo.\"bar\"" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawDataField_FromTemporaryTableRecordSet()
    {
        _sut.Visit( SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "bar" ) ).AsSet().GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "temp.\"foo\".\"bar\"" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawDataField_FromAliasedTemporaryTableRecordSet()
    {
        _sut.Visit( SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "bar" ) ).AsSet( "qux" ).GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"qux\".\"bar\"" );
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
                .BeEquivalentTo( KeyValuePair.Create( "a", (SqlExpressionType?)SqlExpressionType.Create<int>() ) );
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
                .BeEquivalentTo( KeyValuePair.Create( "b", (SqlExpressionType?)SqlExpressionType.Create<string>( isNullable: true ) ) );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretColumn()
    {
        var table = CreateTable( string.Empty, "foo", "bar" ).ToRecordSet();
        _sut.Visit( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\".\"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretColumnWithoutParentheses()
    {
        var table = SqlNode.Table( CreateTable( string.Empty, "foo", "bar" ) );
        _sut.VisitChild( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\".\"bar\"" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnBuilder()
    {
        var table = SqlNode.Table( CreateTableBuilder( string.Empty, "foo", "bar" ) );
        _sut.Visit( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\".\"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretColumnBuilderWithoutParentheses()
    {
        var table = SqlNode.Table( CreateTableBuilder( string.Empty, "foo", "bar" ) );
        _sut.VisitChild( table.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\".\"bar\"" );
    }

    [Fact]
    public void Visit_ShouldInterpretQueryDataField()
    {
        var query = SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["bar"].AsSelf() } ).AsSet( "qux" );
        _sut.Visit( query.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"qux\".\"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretQueryDataFieldWithoutParentheses()
    {
        var query = SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["bar"].AsSelf() } ).AsSet( "qux" );
        _sut.VisitChild( query.GetField( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"qux\".\"bar\"" );
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
        _sut.Context.Sql.ToString().Should().Be( "\"foo\".\"qux\"" );
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
        _sut.Context.Sql.ToString().Should().Be( "\"foo\".\"qux\"" );
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
        _sut.Context.Sql.ToString().Should().Be( "MOD((foo.a), (foo.b))" );
    }

    [Fact]
    public void Visit_ShouldInterpretModulo_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) % SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "MOD(25, 35)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretModuloWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) % SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "MOD(25, 35)" );
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
        _sut.Context.Sql.ToString().Should().Be( "((foo.a) | (foo.b)) & ~((foo.a) & (foo.b))" );
    }

    [Fact]
    public void Visit_ShouldInterpretBitwiseXor_WhenValuesDoNotRequireParentheses()
    {
        _sut.Visit( SqlNode.Literal( 25 ) ^ SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "(25 | 35) & ~(25 & 35)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretBitwiseXorWithParentheses()
    {
        _sut.VisitChild( SqlNode.Literal( 25 ) ^ SqlNode.Literal( 35 ) );
        _sut.Context.Sql.ToString().Should().Be( "((25 | 35) & ~(25 & 35))" );
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
        _sut.Context.SetParentNode( SqlNode.Null() );
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
    public void Visit_ShouldInterpretRecordsAffectedFunction()
    {
        _sut.Visit( SqlNode.Functions.RecordsAffected() );
        _sut.Context.Sql.ToString().Should().Be( "CHANGES()" );
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
        _sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_DATE()" );
    }

    [Fact]
    public void Visit_ShouldInterpretCurrentTimeFunction()
    {
        _sut.Visit( SqlNode.Functions.CurrentTime() );
        _sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_TIME()" );
    }

    [Fact]
    public void Visit_ShouldInterpretCurrentDateTimeFunction()
    {
        _sut.Visit( SqlNode.Functions.CurrentDateTime() );
        _sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_DATETIME()" );
    }

    [Fact]
    public void Visit_ShouldInterpretCurrentTimestampFunction()
    {
        _sut.Visit( SqlNode.Functions.CurrentTimestamp() );
        _sut.Context.Sql.ToString().Should().Be( "GET_CURRENT_TIMESTAMP()" );
    }

    [Fact]
    public void Visit_ShouldInterpretNewGuidFunction()
    {
        _sut.Visit( SqlNode.Functions.NewGuid() );
        _sut.Context.Sql.ToString().Should().Be( "NEW_GUID()" );
    }

    [Fact]
    public void Visit_ShouldInterpretLengthFunction()
    {
        _sut.Visit( SqlNode.Functions.Length( SqlNode.Literal( "foo" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LENGTH('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretToLowerFunction()
    {
        _sut.Visit( SqlNode.Functions.ToLower( SqlNode.Literal( "FOO" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TO_LOWER('FOO')" );
    }

    [Fact]
    public void Visit_ShouldInterpretToUpperFunction()
    {
        _sut.Visit( SqlNode.Functions.ToUpper( SqlNode.Literal( "foo" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TO_UPPER('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimStartFunction()
    {
        _sut.Visit( SqlNode.Functions.TrimStart( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "LTRIM((foo.a), 'bar')" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimEndFunction()
    {
        _sut.Visit( SqlNode.Functions.TrimEnd( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "RTRIM((foo.a), 'bar')" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrimFunction()
    {
        _sut.Visit( SqlNode.Functions.Trim( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "TRIM((foo.a), 'bar')" );
    }

    [Fact]
    public void Visit_ShouldInterpretSubstringFunction()
    {
        _sut.Visit( SqlNode.Functions.Substring( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( 10 ), SqlNode.Literal( 5 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "SUBSTR((foo.a), 10, 5)" );
    }

    [Fact]
    public void Visit_ShouldInterpretReplaceFunction()
    {
        _sut.Visit( SqlNode.Functions.Replace( SqlNode.RawExpression( "foo.a" ), SqlNode.Literal( "foo" ), SqlNode.Literal( "bar" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "REPLACE((foo.a), 'foo', 'bar')" );
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
        _sut.Context.Sql.ToString().Should().Be( "INSTR_LAST((foo.a), 'bar')" );
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
        _sut.Context.Sql.ToString().Should().Be( "TRUNC(@a)" );
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
        _sut.Context.Sql.ToString().Should().Be( "MIN(NULL, @a, (foo.a))" );
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
        _sut.Context.Sql.ToString().Should().Be( "MAX(NULL, @a, (foo.a))" );
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
        _sut.Context.Sql.ToString().Should().Be( "CHANGES()" );
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
    public void Visit_ShouldInterpretMinAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( 25 ).Min() );
        _sut.Context.Sql.ToString().Should().Be( "MIN(25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretMinAggregateFunctionWithTraits()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).Min().Distinct().AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "MIN(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
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
        _sut.Context.Sql.ToString().Should().Be( "MAX(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
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
        _sut.Context.Sql.ToString().Should().Be( "AVG(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
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
        _sut.Context.Sql.ToString().Should().Be( "SUM(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
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
        _sut.Context.Sql.ToString().Should().Be( "COUNT(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretStringConcatAggregateFunction()
    {
        _sut.Visit( SqlNode.Literal( "foo" ).StringConcat() );
        _sut.Context.Sql.ToString().Should().Be( "GROUP_CONCAT('foo')" );
    }

    [Fact]
    public void Visit_ShouldInterpretStringConcatAggregateFunctionWithTraits()
    {
        _sut.Visit(
            SqlNode.RawExpression( "foo.a" )
                .StringConcat( SqlNode.Literal( " - " ) )
                .Distinct()
                .AndWhere( SqlNode.RawCondition( "foo.b > 10" ) ) );

        _sut.Context.Sql.ToString().Should().Be( "GROUP_CONCAT(DISTINCT (foo.a), ' - ') FILTER (WHERE foo.b > 10)" );
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
    public void VisitChild_ShouldInterpretAggregateFunctionWithParentheses_WhenTraitsContainsNonDistinct()
    {
        _sut.VisitChild(
            SqlNode.AggregateFunctions.Count( SqlNode.RawExpression( "foo.a" ) )
                .Distinct()
                .AndWhere( SqlNode.RawCondition( "foo.a > 10" ) ) );

        _sut.Context.Sql.ToString().Should().Be( "(COUNT(DISTINCT (foo.a)) FILTER (WHERE foo.a > 10))" );
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
                .BeEquivalentTo( KeyValuePair.Create( "a", (SqlExpressionType?)SqlExpressionType.Create<int>() ) );

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
        _sut.Context.Sql.ToString().Should().Be( "foo AS \"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawRecordSetWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" ) );
        _sut.Context.Sql.ToString().Should().Be( "(foo)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTableRecordSet()
    {
        _sut.Visit( CreateTable( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\" AS \"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTableRecordSetWithParentheses()
    {
        _sut.VisitChild( CreateTable( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(\"foo\")" );
    }

    [Fact]
    public void Visit_ShouldInterpretTableBuilderRecordSet()
    {
        _sut.Visit( CreateTableBuilder( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\" AS \"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTableBuilderRecordSetWithParentheses()
    {
        _sut.VisitChild( CreateTableBuilder( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(\"foo\")" );
    }

    [Fact]
    public void Visit_ShouldInterpretViewRecordSet()
    {
        _sut.Visit( CreateView( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\" AS \"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewRecordSetWithParentheses()
    {
        _sut.VisitChild( CreateView( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(\"foo\")" );
    }

    [Fact]
    public void Visit_ShouldInterpretViewBuilderRecordSet()
    {
        _sut.Visit( CreateViewBuilder( string.Empty, "foo" ).ToRecordSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"foo\" AS \"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewBuilderRecordSetWithParentheses()
    {
        _sut.VisitChild( CreateViewBuilder( string.Empty, "foo" ).ToRecordSet() );
        _sut.Context.Sql.ToString().Should().Be( "(\"foo\")" );
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
) AS ""bar""" );
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
  ) AS ""bar"")" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpressionRecordSet()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet.As( "qux" ) );
        _sut.Context.Sql.ToString().Should().Be( "\"bar\" AS \"qux\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCommonTableExpressionRecordSetWithParentheses()
    {
        _sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet );
        _sut.Context.Sql.ToString().Should().Be( "(\"bar\")" );
    }

    [Fact]
    public void Visit_ShouldInterpretTemporaryTableRecordSet()
    {
        _sut.Visit( SqlNode.CreateTempTable( "foo" ).AsSet( "bar" ) );
        _sut.Context.Sql.ToString().Should().Be( "temp.\"foo\" AS \"bar\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTemporaryTableRecordSetWithParentheses()
    {
        _sut.VisitChild( SqlNode.CreateTempTable( "foo" ).AsSet() );
        _sut.Context.Sql.ToString().Should().Be( "(temp.\"foo\")" );
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
    public void Visit_ShouldInterpretFullJoinOn()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" ).FullOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        _sut.Context.Sql.ToString().Should().Be( "FULL JOIN foo ON bar.a = foo.a" );
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
        _sut.Context.Sql.ToString().Should().Be( "(foo.a) AS \"b\"" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectFieldWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawRecordSet( "foo" )["a"].AsSelf() );
        _sut.Context.Sql.ToString().Should().Be( "foo.\"a\"" );
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

        _sut.Context.Sql.ToString().Should().Be( "\"a\"" );
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

        _sut.Context.Sql.ToString().Should().Be( "\"a\"" );
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
        _sut.Context.Sql.ToString().Should().Be( "\"b\"" );
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
        _sut.Context.Sql.ToString().Should().Be( "\"a\"" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawQuery()
    {
        _sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a = @a", SqlNode.Parameter<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            _sut.Context.Parameters.Should().HaveCount( 1 );
            _sut.Context.Parameters.Should()
                .BeEquivalentTo( KeyValuePair.Create( "a", (SqlExpressionType?)SqlExpressionType.Create<int>() ) );

            _sut.Context.Sql.ToString().Should().Be( "SELECT * FROM foo WHERE foo.a = @a" );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawQueryWithParentheses()
    {
        _sut.Context.SetParentNode( SqlNode.Null() );
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
                .BeEquivalentTo( KeyValuePair.Create( "p", (SqlExpressionType?)SqlExpressionType.Create<int>() ) );

            _sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"SELECT
  ""foo"".""a"",
  ""foo"".""b"" AS ""x"",
  ""lorem"".*,
  ""qux"".""e"",
  ""qux"".""f"" AS ""y"",
  @p AS ""z""
FROM ""foo""
INNER JOIN ""bar"" AS ""lorem"" ON ""lorem"".""c"" = ""foo"".""a""
LEFT JOIN ""qux"" ON ""qux"".""e"" = ""foo"".""b""" );
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
            .Select(
                s => new SqlSelectNode[]
                {
                    s["foo"]["b"].As( "x" ),
                    s["lorem"]["c"].AsSelf(),
                    SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).As( "v" )
                } )
            .OrderBy( s => new[] { s.DataSource["foo"]["b"].Asc() } )
            .OrderBy( s => new[] { s.DataSource["lorem"]["c"].Desc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( query );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
),
""zyx"" AS (
  SELECT * FROM xyz JOIN cba ON cba.h = xyz.h
)
SELECT DISTINCT
  ""foo"".""b"" AS ""x"",
  ""lorem"".""c"",
  COUNT(*) AS ""v""
FROM ""foo""
INNER JOIN ""bar"" AS ""lorem"" ON ""lorem"".""c"" = ""foo"".""a""
LEFT JOIN ""qux"" ON ""qux"".""e"" = ""foo"".""b""
WHERE (""qux"".""f"" > 50) AND (""foo"".""a"" IN (
    SELECT
      ""zyx"".""h""
    FROM ""zyx""
  ))
GROUP BY ""foo"".""b"", ""lorem"".""c""
HAVING (""foo"".""b"" < 100) OR (""lorem"".""c"" BETWEEN 0 AND 75)
ORDER BY ""foo"".""b"" ASC, ""lorem"".""c"" DESC
LIMIT 50 OFFSET 100" );
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
  foo.""a""
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
  foo.""a""
FROM foo
LIMIT -1 OFFSET 100" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretDataSourceQueryWithParentheses()
    {
        _sut.Context.SetParentNode( SqlNode.Null() );
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
  FROM ""foo""
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
                @"WITH ""x"" AS (
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
        _sut.Context.SetParentNode( SqlNode.Null() );
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
                @"WITH ""A"" AS (
  SELECT * FROM foo
),
""B"" AS (
  SELECT * FROM bar
)" );
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
                @"RECURSIVE ""A"" AS (
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
    public void Visit_ShouldInterpretTypeCast()
    {
        _sut.Visit( SqlNode.RawExpression( "foo.a" ).CastTo<string>() );
        _sut.Context.Sql.ToString().Should().Be( "CAST((foo.a) AS TEXT)" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTypeCastWithoutParentheses()
    {
        _sut.VisitChild( SqlNode.RawExpression( "foo.a" ).CastTo<int>() );
        _sut.Context.Sql.ToString().Should().Be( "CAST((foo.a) AS INTEGER)" );
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
                @"INSERT INTO qux (""a"", ""b"")
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
                @"INSERT INTO qux (""a"", ""b"")
SELECT a, b FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithDataSourceQuery()
    {
        var foo = CreateTable( string.Empty, "foo", "a", "b" ).ToRecordSet();
        var bar = CreateTable( string.Empty, "bar", "c", "d" ).ToRecordSet();

        var query = foo
            .Join( bar.InnerOn( bar["c"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["foo"]["b"] } )
            .AndHaving( s => s["foo"]["b"] < SqlNode.Literal( 100 ) )
            .Select(
                s => new SqlSelectNode[]
                {
                    s["foo"]["b"].As( "a" ),
                    SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).As( "b" )
                } )
            .OrderBy( s => new[] { s.DataSource["foo"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( query.ToInsertInto( SqlNode.RawRecordSet( "qux" ), r => new[] { r["a"], r["b"] } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
INSERT INTO qux (""a"", ""b"")
SELECT DISTINCT
  ""foo"".""b"" AS ""a"",
  COUNT(*) AS ""b""
FROM ""foo""
INNER JOIN ""bar"" ON ""bar"".""c"" = ""foo"".""a""
WHERE ""bar"".""c"" IN (
  SELECT cba.c FROM cba
)
GROUP BY ""foo"".""b""
HAVING ""foo"".""b"" < 100
ORDER BY ""foo"".""b"" ASC
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
                @"WITH ""x"" AS (
  SELECT * FROM ipsum
)
INSERT INTO lorem (""a"", ""b"")
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
    public void Visit_ShouldInterpretUpdateWithSimpleDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["foo"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["foo"]["b"].Assign( SqlNode.Literal( 10 ) ), s["foo"]["c"].Assign( SqlNode.Literal( "foo" ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE foo SET
  ""b"" = 10,
  ""c"" = 'foo'
WHERE foo.""a"" IN (
  SELECT cba.c FROM cba
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTableWithSingleColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ), s["f"]["b"].Assign( SqlNode.Literal( 20 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""foo"" SET
  ""a"" = 10,
  ""b"" = 20
WHERE ""foo"".""a"" IN (
  SELECT DISTINCT ""f"".""a""
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE bar.""c"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTableWithMultipleColumnPrimaryKey_WithFilter()
    {
        var table = CreateTable( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ), s["f"]["b"].Assign( SqlNode.Literal( 20 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""foo"" SET
  ""a"" = 10,
  ""b"" = 20
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"") AND (bar.""c"" IN (
      SELECT cba.c FROM cba
    ))
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTableWithMultipleColumnPrimaryKey_WithoutFilter()
    {
        var table = CreateTable( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ), s["f"]["b"].Assign( SqlNode.Literal( 20 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""foo"" SET
  ""a"" = 10,
  ""b"" = 20
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTableBuilderWithSingleColumnPrimaryKey()
    {
        var table = CreateTableBuilder( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ), s["f"]["b"].Assign( SqlNode.Literal( 20 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""foo"" SET
  ""a"" = 10,
  ""b"" = 20
WHERE ""foo"".""a"" IN (
  SELECT DISTINCT ""f"".""a""
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE bar.""c"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTableBuilderWithMultipleColumnPrimaryKey()
    {
        var table = CreateTableBuilder( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ), s["f"]["b"].Assign( SqlNode.Literal( 20 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""foo"" SET
  ""a"" = 10,
  ""b"" = 20
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTableBuilderWithoutPrimaryKey()
    {
        var table = CreateEmptyTableBuilder( string.Empty, "foo" );
        table.Columns.Create( "a" );
        table.Columns.Create( "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ), s["f"]["b"].Assign( SqlNode.Literal( 20 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE ""foo"" SET
  ""a"" = 10,
  ""b"" = 20
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTemporaryTableWithSingleColumn()
    {
        var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "a" ) );
        var foo = table.AsSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"].GetUnsafeField( "b" ) } )
            .AndHaving( s => s["f"].GetUnsafeField( "b" ) < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"].GetUnsafeField( "b" ).Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToUpdate( s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE temp.""foo"" SET
  ""a"" = 10
WHERE temp.""foo"".""a"" IN (
  SELECT DISTINCT ""f"".""a""
  FROM temp.""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE bar.""c"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretUpdateWithComplexDataSourceAndTargetTemporaryTableWithMultipleColumns()
    {
        var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "a" ), SqlNode.ColumnDefinition<int>( "b" ) );
        var foo = table.AsSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit(
            dataSource.ToUpdate(
                s => new[] { s["f"]["a"].Assign( SqlNode.Literal( 10 ) ), s["f"]["b"].Assign( SqlNode.Literal( 20 ) ) } ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
UPDATE temp.""foo"" SET
  ""a"" = 10,
  ""b"" = 20
WHERE EXISTS (
  SELECT DISTINCT *
  FROM temp.""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (temp.""foo"".""a"" = ""f"".""a"") AND (temp.""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsNotTableRecordSet()
    {
        var node = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsTableRecordSetWithoutAlias()
    {
        var foo = CreateTable( string.Empty, "foo" ).ToRecordSet();
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsTableBuilderRecordSetWithoutColumns()
    {
        var foo = CreateEmptyTableBuilder( string.Empty, "foo" ).ToRecordSet( "f" );
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenUpdateIsComplexAndDataSourceFromIsTemporaryTableRecordSetWithoutColumns()
    {
        var foo = SqlNode.CreateTempTable( "foo" ).AsSet( "f" );
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToUpdate();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretValueAssignment()
    {
        _sut.Visit( SqlNode.RawRecordSet( "foo" )["a"].Assign( SqlNode.Literal( 50 ) ) );
        _sut.Context.Sql.ToString().Should().Be( "\"a\" = 50" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithSimpleDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .AndWhere( s => s["foo"]["a"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM foo
WHERE foo.""a"" IN (
  SELECT cba.c FROM cba
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTableWithSingleColumnPrimaryKey()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""foo""
WHERE ""foo"".""a"" IN (
  SELECT DISTINCT ""f"".""a""
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE bar.""c"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTableWithMultipleColumnPrimaryKey_WithFilter()
    {
        var table = CreateTable( string.Empty, "foo", "a", "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""foo""
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"") AND (bar.""c"" IN (
      SELECT cba.c FROM cba
    ))
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTableWithMultipleColumnPrimaryKey_WithoutFilter()
    {
        var table = CreateTable( string.Empty, "foo", new[] { "a", "b", "c" }, "a", "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""foo""
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTableBuilderWithSingleColumnPrimaryKey()
    {
        var table = CreateTableBuilder( string.Empty, "foo", new[] { "a", "b" }, "a" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""foo""
WHERE ""foo"".""a"" IN (
  SELECT DISTINCT ""f"".""a""
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE bar.""c"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTableBuilderWithMultipleColumnPrimaryKey()
    {
        var table = CreateTableBuilder( string.Empty, "foo", new[] { "a", "b", "c" }, "a", "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""foo""
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTableBuilderWithoutPrimaryKey()
    {
        var table = CreateEmptyTableBuilder( string.Empty, "foo" );
        table.Columns.Create( "a" );
        table.Columns.Create( "b" );
        var foo = table.ToRecordSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM ""foo""
WHERE EXISTS (
  SELECT DISTINCT *
  FROM ""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (""foo"".""a"" = ""f"".""a"") AND (""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTemporaryTableWithSingleColumn()
    {
        var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "a" ) );
        var foo = table.AsSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["f"].GetUnsafeField( "b" ) } )
            .AndHaving( s => s["f"].GetUnsafeField( "b" ) < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"].GetUnsafeField( "b" ).Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM temp.""foo""
WHERE temp.""foo"".""a"" IN (
  SELECT DISTINCT ""f"".""a""
  FROM temp.""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE bar.""c"" IN (
    SELECT cba.c FROM cba
  )
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDeleteFromWithComplexDataSourceAndTargetTemporaryTableWithMultipleColumns()
    {
        var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "a" ), SqlNode.ColumnDefinition<int>( "b" ) );
        var foo = table.AsSet( "f" );

        var dataSource = foo
            .Join( SqlJoinDefinition.Inner( SqlNode.RawRecordSet( "bar" ), x => x.Inner["a"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .GroupBy( s => new[] { s["f"]["b"] } )
            .AndHaving( s => s["f"]["b"] < SqlNode.Literal( 100 ) )
            .OrderBy( s => new[] { s["f"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        _sut.Visit( dataSource.ToDeleteFrom() );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH ""cba"" AS (
  SELECT * FROM abc
)
DELETE FROM temp.""foo""
WHERE EXISTS (
  SELECT DISTINCT *
  FROM temp.""foo"" AS ""f""
  INNER JOIN bar ON bar.""a"" = ""f"".""a""
  WHERE (temp.""foo"".""a"" = ""f"".""a"") AND (temp.""foo"".""b"" = ""f"".""b"")
  GROUP BY ""f"".""b""
  HAVING ""f"".""b"" < 100
  ORDER BY ""f"".""b"" ASC
  LIMIT 50 OFFSET 100
)" );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsNotTableRecordSet()
    {
        var node = SqlNode.RawRecordSet( "foo" ).Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsTableRecordSetWithoutAlias()
    {
        var foo = CreateTable( string.Empty, "foo" ).ToRecordSet();
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsTableBuilderRecordSetWithoutColumns()
    {
        var foo = CreateEmptyTableBuilder( string.Empty, "foo" ).ToRecordSet( "f" );
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldThrowSqlNodeVisitorException_WhenDeleteFromIsComplexAndDataSourceFromIsTemporaryTableRecordSetWithoutColumns()
    {
        var foo = SqlNode.CreateTempTable( "foo" ).AsSet( "f" );
        var node = foo.Join( SqlNode.RawRecordSet( "bar" ).Cross() ).ToDeleteFrom();
        var action = Lambda.Of( () => _sut.Visit( node ) );

        action.Should()
            .ThrowExactly<SqlNodeVisitorException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, _sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnDefinition_WhenNullable()
    {
        _sut.Visit( SqlNode.ColumnDefinition<int>( "a", isNullable: true ) );
        _sut.Context.Sql.ToString().Should().Be( "\"a\" INTEGER" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnDefinition_WhenNonNullable()
    {
        _sut.Visit( SqlNode.ColumnDefinition<string>( "a", isNullable: false ) );
        _sut.Context.Sql.ToString().Should().Be( "\"a\" TEXT NOT NULL" );
    }

    [Fact]
    public void Visit_ShouldInterpretCreateTemporaryTable()
    {
        _sut.Visit(
            SqlNode.CreateTempTable(
                "foo",
                SqlNode.ColumnDefinition<int>( "a" ),
                SqlNode.ColumnDefinition<string>( "b", isNullable: true ) ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"CREATE TEMP TABLE ""foo"" (
  ""a"" INTEGER NOT NULL,
  ""b"" TEXT
) WITHOUT ROWID" );
    }

    [Fact]
    public void Visit_ShouldInterpretDropTemporaryTable()
    {
        _sut.Visit( SqlNode.DropTempTable( "foo" ) );
        _sut.Context.Sql.ToString().Should().Be( "DROP TABLE temp.\"foo\"" );
    }

    [Fact]
    public void Visit_ShouldInterpretStatementBatch()
    {
        _sut.Visit(
            SqlNode.Batch(
                SqlNode.BeginTransaction( IsolationLevel.Serializable ),
                SqlNode.DropTempTable( "bar" ),
                SqlNode.RawQuery( "SELECT * FROM foo" ),
                SqlNode.RawQuery( "SELECT * FROM qux" ),
                SqlNode.CommitTransaction() ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"PRAGMA read_uncommitted = 0;
BEGIN IMMEDIATE;

DROP TABLE temp.""bar"";

SELECT * FROM foo;

SELECT * FROM qux;

COMMIT;" );
    }

    [Theory]
    [InlineData( IsolationLevel.Unspecified )]
    [InlineData( IsolationLevel.Chaos )]
    [InlineData( IsolationLevel.ReadCommitted )]
    [InlineData( IsolationLevel.RepeatableRead )]
    [InlineData( IsolationLevel.Serializable )]
    [InlineData( IsolationLevel.Snapshot )]
    public void Visit_ShouldInterpretBeginTransaction_WithIsolationLevelOtherThanReadUncommitted(IsolationLevel isolationLevel)
    {
        _sut.Visit( SqlNode.BeginTransaction( isolationLevel ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"PRAGMA read_uncommitted = 0;
BEGIN IMMEDIATE" );
    }

    [Fact]
    public void Visit_ShouldInterpretBeginTransaction_WithReadUncommittedIsolationLevel()
    {
        _sut.Visit( SqlNode.BeginTransaction( IsolationLevel.ReadUncommitted ) );

        _sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"PRAGMA read_uncommitted = 1;
BEGIN" );
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

    [Pure]
    private static SqliteTableBuilder CreateTableBuilder(string schemaName, string tableName, params string[] columnNames)
    {
        return CreateTableBuilder( schemaName, tableName, columnNames, Array.Empty<string>() );
    }

    [Pure]
    private static SqliteTableBuilder CreateTableBuilder(
        string schemaName,
        string tableName,
        string[] columnNames,
        params string[] pkColumnNames)
    {
        var table = CreateEmptyTableBuilder( schemaName, tableName );

        if ( columnNames.Length == 0 )
            columnNames = new[] { "X" };

        foreach ( var c in columnNames )
            table.Columns.Create( c );

        if ( pkColumnNames.Length == 0 )
            pkColumnNames = columnNames;

        table.SetPrimaryKey( pkColumnNames.Select( n => table.Columns.Get( n ).Asc() ).ToArray() );
        return table;
    }

    [Pure]
    private static SqliteTableBuilder CreateEmptyTableBuilder(string schemaName, string tableName)
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var schema = db.Schemas.GetOrCreate( schemaName );
        var table = schema.Objects.CreateTable( tableName );
        return table;
    }

    [Pure]
    private static SqliteTable CreateTable(string schemaName, string tableName, params string[] columnNames)
    {
        return CreateTable( schemaName, tableName, columnNames, Array.Empty<string>() );
    }

    [Pure]
    private static SqliteTable CreateTable(string schemaName, string tableName, string[] columnNames, params string[] pkColumnNames)
    {
        var builder = CreateTableBuilder( schemaName, tableName, columnNames, pkColumnNames );
        var db = new SqliteDatabaseMock( builder.Database );
        return db.Schemas.Get( schemaName ).Objects.GetTable( tableName );
    }

    [Pure]
    private static SqliteViewBuilder CreateViewBuilder(string schemaName, string viewName, SqlQueryExpressionNode? source = null)
    {
        var db = SqliteDatabaseBuilderMock.Create();
        var schema = db.Schemas.GetOrCreate( schemaName );
        var view = schema.Objects.CreateView( viewName, source ?? SqlNode.RawQuery( "SELECT * FROM foo" ) );
        return view;
    }

    [Pure]
    private static SqliteView CreateView(string schemaName, string viewName, SqlQueryExpressionNode? source = null)
    {
        var builder = CreateViewBuilder( schemaName, viewName, source );
        var db = new SqliteDatabaseMock( builder.Database );
        return db.Schemas.Get( schemaName ).Objects.GetView( viewName );
    }

    private sealed class FunctionMock : SqlFunctionExpressionNode
    {
        public FunctionMock()
            : base( SqlFunctionType.Custom, Array.Empty<SqlExpressionNode>() ) { }
    }

    private sealed class AggregateFunctionMock : SqlAggregateFunctionExpressionNode
    {
        public AggregateFunctionMock()
            : base( SqlFunctionType.Custom, ReadOnlyMemory<SqlExpressionNode>.Empty, Chain<SqlTraitNode>.Empty ) { }

        public override SqlAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
        {
            return new AggregateFunctionMock();
        }
    }

    private sealed class NodeMock : SqlNodeBase
    {
        public NodeMock()
            : base( SqlNodeType.Unknown ) { }
    }
}
