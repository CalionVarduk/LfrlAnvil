using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests;

public partial class MySqlNodeInterpreterTests : TestsBase
{
    [Fact]
    public void Interpreter_ShouldUseBackticksAsNameDelimiters()
    {
        var sut = CreateInterpreter();
        using ( new AssertionScope() )
        {
            sut.BeginNameDelimiter.Should().Be( '`' );
            sut.EndNameDelimiter.Should().Be( '`' );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretRawExpression()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawExpression( "foo.a + @bar", SqlNode.Parameter<int>( "bar" ) ) );

        using ( new AssertionScope() )
        {
            sut.Context.Sql.ToString().Should().Be( "foo.a + @bar" );
            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( "bar", TypeNullability.Create<int>(), null ) );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawExpressionWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawExpression( "foo.a + @bar" ) );
        sut.Context.Sql.ToString().Should().Be( "(foo.a + @bar)" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawDataField()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawDataField( SqlNode.RawRecordSet( "foo" ), "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "foo.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawDataField_FromRawRecordSetWithInfo()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawDataField( SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "foo", "bar" ) ), "qux" ) );
        sut.Context.Sql.ToString().Should().Be( "`foo`.`bar`.`qux`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawDataFieldWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawDataField( SqlNode.RawRecordSet( "foo" ), "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "foo.`bar`" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar`.`qux`" )]
    [InlineData( true, "`foo`.`qux`" )]
    public void Visit_ShouldInterpretRawDataField_FromNewTable(bool isTemporary, string expected)
    {
        var sut = CreateInterpreter();
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        sut.Visit( SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "qux" ) } ).AsSet().GetField( "qux" ) );
        sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Visit_ShouldInterpretRawDataField_FromAliasedNewTable(bool isTemporary)
    {
        var sut = CreateInterpreter();
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        sut.Visit( SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "qux" ) } ).AsSet( "lorem" ).GetField( "qux" ) );
        sut.Context.Sql.ToString().Should().Be( "`lorem`.`qux`" );
    }

    [Fact]
    public void Visit_ShouldInterpretNull()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.Null() );
        sut.Context.Sql.ToString().Should().Be( "NULL" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretNullWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.Null() );
        sut.Context.Sql.ToString().Should().Be( "NULL" );
    }

    [Fact]
    public void Visit_ShouldInterpretLiteral()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.Literal( "fo'o" ) );
        sut.Context.Sql.ToString().Should().Be( "'fo''o'" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretLiteralWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.Literal( 25 ) );
        sut.Context.Sql.ToString().Should().Be( "25" );
    }

    [Fact]
    public void Visit_ShouldInterpretParameter()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.Parameter<int>( "a" ) );

        using ( new AssertionScope() )
        {
            sut.Context.Sql.ToString().Should().Be( "@a" );
            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( "a", TypeNullability.Create<int>(), null ) );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretPositionalParameterAsNamed()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.Parameter<int>( "a", index: 1 ) );

        using ( new AssertionScope() )
        {
            sut.Context.Sql.ToString().Should().Be( "@a" );
            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( "a", TypeNullability.Create<int>(), null ) );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretParameterWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.Parameter<string>( "b", isNullable: true ) );

        using ( new AssertionScope() )
        {
            sut.Context.Sql.ToString().Should().Be( "@b" );
            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo(
                    new SqlNodeInterpreterContextParameter( "b", TypeNullability.Create<string>( isNullable: true ), null ) );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretColumn()
    {
        var sut = CreateInterpreter();
        var table = SqlTableMock.Create<int>( "foo", new[] { "bar" } ).ToRecordSet();
        sut.Visit( table.GetField( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo`.`bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretColumnWithoutParentheses()
    {
        var sut = CreateInterpreter();
        var table = SqlNode.Table( SqlTableMock.Create<int>( "foo", new[] { "bar" } ) );
        sut.VisitChild( table.GetField( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo`.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretColumnBuilder()
    {
        var sut = CreateInterpreter();
        var table = SqlNode.Table( SqlTableBuilderMock.Create<int>( "foo", new[] { "bar" } ) );
        sut.Visit( table.GetField( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo`.`bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretColumnBuilderWithoutParentheses()
    {
        var sut = CreateInterpreter();
        var table = SqlNode.Table( SqlTableBuilderMock.Create<int>( "foo", new[] { "bar" } ) );
        sut.VisitChild( table.GetField( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo`.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretQueryDataField()
    {
        var sut = CreateInterpreter();
        var query = SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["bar"].AsSelf() } ).AsSet( "qux" );
        sut.Visit( query.GetField( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`qux`.`bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretQueryDataFieldWithoutParentheses()
    {
        var sut = CreateInterpreter();
        var query = SqlNode.RawRecordSet( "foo" ).ToDataSource().Select( s => new[] { s.From["bar"].AsSelf() } ).AsSet( "qux" );
        sut.VisitChild( query.GetField( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`qux`.`bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretViewDataField()
    {
        var sut = CreateInterpreter();
        var view = SqlNode.View(
            SqlViewMock.Create( "foo", SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["qux"].AsSelf() } ) ) );

        sut.Visit( view.GetField( "qux" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo`.`qux`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewDataFieldWithoutParentheses()
    {
        var sut = CreateInterpreter();
        var view = SqlNode.View(
            SqlViewMock.Create( "foo", SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["qux"].AsSelf() } ) ) );

        sut.VisitChild( view.GetField( "qux" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo`.`qux`" );
    }

    [Fact]
    public void Visit_ShouldInterpretSwitchCase_WhenValueRequiresParentheses()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.RawExpression( "foo.b" ) ) );
        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WHEN foo.a > 10
  THEN (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSwitchCase_WhenValueDoesNotRequireParentheses()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.Literal( 25 ) ) );
        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WHEN foo.a > 10
  THEN 25" );
    }

    [Fact]
    public void Visit_ShouldInterpretSwitch()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.Switch(
                new[]
                {
                    SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.RawExpression( "foo.b" ) ),
                    SqlNode.RawCondition( "foo.a > 5" ).Then( SqlNode.Parameter<int>( "a" ) )
                },
                SqlNode.Literal( 25 ) ) );

        sut.Context.Sql.ToString()
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
        var sut = CreateInterpreter();
        sut.VisitChild(
            SqlNode.Switch(
                new[]
                {
                    SqlNode.RawCondition( "foo.a > 10" ).Then( SqlNode.RawExpression( "foo.b" ) ),
                    SqlNode.RawCondition( "foo.a > 5" ).Then( SqlNode.Parameter<int>( "a" ) )
                },
                SqlNode.Literal( 25 ) ) );

        sut.Context.Sql.ToString()
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
    public void Visit_ShouldInterpretRawCondition()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawCondition( "foo.a > @a", SqlNode.Parameter<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( "a", TypeNullability.Create<int>(), null ) );

            sut.Context.Sql.ToString().Should().Be( "foo.a > @a" );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawConditionWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ) );
        sut.Context.Sql.ToString().Should().Be( "(foo.a > 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTrue()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.True() );
        sut.Context.Sql.ToString().Should().Be( "TRUE" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTrueWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.True() );
        sut.Context.Sql.ToString().Should().Be( "TRUE" );
    }

    [Fact]
    public void Visit_ShouldInterpretFalse()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.False() );
        sut.Context.Sql.ToString().Should().Be( "FALSE" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretFalseWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.False() );
        sut.Context.Sql.ToString().Should().Be( "FALSE" );
    }

    [Fact]
    public void Visit_ShouldInterpretConditionValue()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawCondition( "foo.a > 10" ).ToValue() );
        sut.Context.Sql.ToString().Should().Be( "foo.a > 10" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretConditionValueWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawCondition( "foo.a > 10" ).ToValue() );
        sut.Context.Sql.ToString().Should().Be( "(foo.a > 10)" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawRecordSet()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo", "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "foo AS `bar`" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawRecordSet_WithInfo()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "foo", "bar" ), "qux" ) );
        sut.Context.Sql.ToString().Should().Be( "`foo`.`bar` AS `qux`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawRecordSetWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawRecordSet( "foo" ) );
        sut.Context.Sql.ToString().Should().Be( "(foo)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTable()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTableWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlTableMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet() );
        sut.Context.Sql.ToString().Should().Be( "(`common`.`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretTableBuilder()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlTableBuilderMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTableBuilderWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlTableBuilderMock.Create<int>( "foo", new[] { "a" } ).ToRecordSet() );
        sut.Context.Sql.ToString().Should().Be( "(`common`.`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretView()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlViewMock.Create( "foo" ).ToRecordSet( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlViewMock.Create( "foo" ).ToRecordSet() );
        sut.Context.Sql.ToString().Should().Be( "(`common`.`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretViewBuilder()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlViewBuilderMock.Create( "foo" ).ToRecordSet( "bar" ) );
        sut.Context.Sql.ToString().Should().Be( "`common`.`foo` AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretViewBuilderWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlViewBuilderMock.Create( "foo" ).ToRecordSet() );
        sut.Context.Sql.ToString().Should().Be( "(`common`.`foo`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretQueryRecordSet()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).AsSet( "bar" ) );
        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  SELECT * FROM foo
) AS `bar`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretQueryRecordSetWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ).AsSet( "bar" ) );
        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"((
    SELECT * FROM foo
  ) AS `bar`)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpressionRecordSet()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet.As( "qux" ) );
        sut.Context.Sql.ToString().Should().Be( "`bar` AS `qux`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCommonTableExpressionRecordSetWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "bar" ).RecordSet );
        sut.Context.Sql.ToString().Should().Be( "(`bar`)" );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar` AS `qux`" )]
    [InlineData( true, "`foo` AS `qux`" )]
    public void Visit_ShouldInterpretNewTable(bool isTemporary, string expected)
    {
        var sut = CreateInterpreter();
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        sut.Visit( SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() ).AsSet( "qux" ) );
        sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "(`foo`.`bar`)" )]
    [InlineData( true, "(`foo`)" )]
    public void VisitChild_ShouldInterpretNewTableWithParentheses(bool isTemporary, string expected)
    {
        var sut = CreateInterpreter();
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        sut.VisitChild( SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() ).AsSet() );
        sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "`foo`.`bar` AS `qux`" )]
    [InlineData( true, "`foo` AS `qux`" )]
    public void Visit_ShouldInterpretNewView(bool isTemporary, string expected)
    {
        var sut = CreateInterpreter();
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        sut.Visit( SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM lorem" ) ).AsSet( "qux" ) );
        sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Theory]
    [InlineData( false, "(`foo`.`bar`)" )]
    [InlineData( true, "(`foo`)" )]
    public void VisitChild_ShouldInterpretNewViewWithParentheses(bool isTemporary, string expected)
    {
        var sut = CreateInterpreter();
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        sut.VisitChild( SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM bar" ) ).AsSet() );
        sut.Context.Sql.ToString().Should().Be( expected );
    }

    [Fact]
    public void Visit_ShouldInterpretLeftJoinOn()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).LeftOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        sut.Context.Sql.ToString().Should().Be( "LEFT JOIN foo ON bar.a = foo.a" );
    }

    [Fact]
    public void Visit_ShouldInterpretRightJoinOn()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).RightOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        sut.Context.Sql.ToString().Should().Be( "RIGHT JOIN foo ON bar.a = foo.a" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenInterpretingFullJoinOn()
    {
        var sut = CreateInterpreter();
        var node = SqlNode.RawRecordSet( "foo" ).FullOn( SqlNode.RawCondition( "bar.a = foo.a" ) );
        var action = Lambda.Of( () => sut.Visit( node ) );
        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
    }

    [Fact]
    public void Visit_ShouldInterpretFullJoinOn_WhenFullJoinParsingIsEnabled()
    {
        var sut = CreateInterpreter( MySqlNodeInterpreterOptions.Default.EnableFullJoinParsing() );
        sut.Visit( SqlNode.RawRecordSet( "foo" ).FullOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        sut.Context.Sql.ToString().Should().Be( "FULL JOIN foo ON bar.a = foo.a" );
    }

    [Fact]
    public void Visit_ShouldInterpretInnerJoinOn()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).InnerOn( SqlNode.RawCondition( "bar.a = foo.a" ) ) );
        sut.Context.Sql.ToString().Should().Be( "INNER JOIN foo ON bar.a = foo.a" );
    }

    [Fact]
    public void Visit_ShouldInterpretCrossJoin()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).Cross() );
        sut.Context.Sql.ToString().Should().Be( "CROSS JOIN foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretDummyDataSource()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.DummyDataSource() );
        sut.Context.Sql.ToString().Should().Be( string.Empty );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceWithoutJoins()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource() );
        sut.Context.Sql.ToString().Should().Be( "FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceWithJoins()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.RawRecordSet( "foo" )
                .Join(
                    SqlNode.RawRecordSet( "bar" ).InnerOn( SqlNode.RawCondition( "bar.a = foo.a" ) ),
                    SqlNode.RawRecordSet( "qux" ).LeftOn( SqlNode.RawCondition( "qux.b = foo.b" ) ) ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"FROM foo
INNER JOIN bar ON bar.a = foo.a
LEFT JOIN qux ON qux.b = foo.b" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretDataSourceWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawRecordSet( "foo" ).ToDataSource() );
        sut.Context.Sql.ToString().Should().Be( "(FROM foo)" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectField()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawExpression( "foo.a" ).As( "b" ) );
        sut.Context.Sql.ToString().Should().Be( "(foo.a) AS `b`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectFieldWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawRecordSet( "foo" )["a"].AsSelf() );
        sut.Context.Sql.ToString().Should().Be( "foo.`a`" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectCompoundField()
    {
        var sut = CreateInterpreter();
        var query = SqlTableMock.Create<int>( "foo", new[] { "a" } )
            .ToRecordSet()
            .ToDataSource()
            .Select( t => new[] { t.From["a"].AsSelf() } )
            .CompoundWith(
                SqlTableMock.Create<int>( "bar", new[] { "a" } )
                    .ToRecordSet()
                    .ToDataSource()
                    .Select( t => new[] { t.From["a"].AsSelf() } )
                    .ToUnionAll() );

        sut.Visit( query.Selection[0] );

        sut.Context.Sql.ToString().Should().Be( "`a`" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectCompoundFieldWithoutParentheses()
    {
        var sut = CreateInterpreter();
        var query = SqlTableMock.Create<int>( "foo", new[] { "a" } )
            .ToRecordSet()
            .ToDataSource()
            .Select( t => new[] { t.From["a"].AsSelf() } )
            .CompoundWith(
                SqlTableMock.Create<int>( "bar", new[] { "a" } )
                    .ToRecordSet()
                    .ToDataSource()
                    .Select( t => new[] { t.From["a"].AsSelf() } )
                    .ToUnionAll() );

        sut.VisitChild( query.Selection[0] );

        sut.Context.Sql.ToString().Should().Be( "`a`" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectRecordSet()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).GetAll() );
        sut.Context.Sql.ToString().Should().Be( "foo.*" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectRecordSetWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawRecordSet( "foo" ).GetAll() );
        sut.Context.Sql.ToString().Should().Be( "foo.*" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectAll()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll() );
        sut.Context.Sql.ToString().Should().Be( "*" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectAllWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll() );
        sut.Context.Sql.ToString().Should().Be( "*" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectExpression_WhenSelectionIsField()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource().From["a"].As( "b" ).ToExpression() );
        sut.Context.Sql.ToString().Should().Be( "`b`" );
    }

    [Fact]
    public void Visit_ShouldInterpretSelectExpression_WhenSelectionIsNotField()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" ).ToDataSource().GetAll() );
        sut.Context.Sql.ToString().Should().Be( "*" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretSelectExpressionWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawRecordSet( "foo" ).ToDataSource().From["a"].AsSelf().ToExpression() );
        sut.Context.Sql.ToString().Should().Be( "`a`" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawQuery()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawQuery( "SELECT * FROM foo WHERE foo.a = @a", SqlNode.Parameter<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( "a", TypeNullability.Create<int>(), null ) );

            sut.Context.Sql.ToString().Should().Be( "SELECT * FROM foo WHERE foo.a = @a" );
        }
    }

    [Fact]
    public void VisitChild_ShouldInterpretRawQueryWithParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM foo" ) );
        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  SELECT * FROM foo
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithoutTraits()
    {
        var sut = CreateInterpreter();
        var foo = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).ToRecordSet();
        var bar = SqlTableMock.Create<int>( "bar", new[] { "c", "d" } ).ToRecordSet( "lorem" );
        var qux = SqlTableMock.Create<int>( "qux", new[] { "e", "f" } ).ToRecordSet();

        var query = foo
            .Join( bar.InnerOn( bar["c"] == foo["a"] ), qux.LeftOn( qux["e"] == foo["b"] ) )
            .Select(
                s => new SqlSelectNode[]
                {
                    s["common.foo"]["a"].AsSelf(),
                    s["common.foo"]["b"].As( "x" ),
                    s["lorem"].GetAll(),
                    s["common.qux"]["e"].AsSelf(),
                    s["common.qux"]["f"].As( "y" ),
                    SqlNode.Parameter<int>( "p" ).As( "z" )
                } );

        sut.Visit( query );

        using ( new AssertionScope() )
        {
            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( "p", TypeNullability.Create<int>(), null ) );

            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"SELECT
  `common`.`foo`.`a`,
  `common`.`foo`.`b` AS `x`,
  `lorem`.*,
  `common`.`qux`.`e`,
  `common`.`qux`.`f` AS `y`,
  @p AS `z`
FROM `common`.`foo`
INNER JOIN `common`.`bar` AS `lorem` ON `lorem`.`c` = `common`.`foo`.`a`
LEFT JOIN `common`.`qux` ON `common`.`qux`.`e` = `common`.`foo`.`b`" );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithAllTraits()
    {
        var sut = CreateInterpreter();
        var cba = SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" );
        var zyx = SqlNode.RawQuery( "SELECT * FROM xyz JOIN cba ON cba.h = xyz.h" ).ToCte( "zyx" );

        var foo = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).ToRecordSet();
        var bar = SqlTableMock.Create<int>( "bar", new[] { "c", "d" } ).ToRecordSet( "lorem" );
        var qux = SqlTableMock.Create<int>( "qux", new[] { "e", "f" } ).ToRecordSet();

        var wnd1 = SqlNode.WindowDefinition( "wnd1", new SqlExpressionNode[] { foo["a"], qux["e"] }, new[] { foo["b"].Asc() } );
        var wnd2 = SqlNode.WindowDefinition(
            "wnd2",
            new[] { qux["e"].Asc(), qux["f"].Desc() },
            SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.CurrentRow ) );

        var query = foo
            .Join( bar.InnerOn( bar["c"] == foo["a"] ), qux.LeftOn( qux["e"] == foo["b"] ) )
            .With( cba, zyx )
            .Distinct()
            .AndWhere( s => s["common.qux"]["f"] > SqlNode.Literal( 50 ) )
            .AndWhere(
                s => s["common.foo"]["a"]
                    .InQuery( zyx.RecordSet.ToDataSource().Select( z => new[] { z.From.GetUnsafeField( "h" ).AsSelf() } ) ) )
            .GroupBy( s => new[] { s["common.foo"]["b"] } )
            .GroupBy( s => new[] { s["lorem"]["c"] } )
            .AndHaving( s => s["common.foo"]["b"] < SqlNode.Literal( 100 ) )
            .OrHaving( s => s["lorem"]["c"].IsBetween( SqlNode.Literal( 0 ), SqlNode.Literal( 75 ) ) )
            .Window( wnd1, wnd2 )
            .Select(
                s => new SqlSelectNode[]
                {
                    s["common.foo"]["b"].As( "x" ),
                    s["lorem"]["c"].AsSelf(),
                    SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).As( "v" ),
                    SqlNode.AggregateFunctions.Sum( s["common.foo"]["a"] ).Over( wnd1 ).As( "w" )
                } )
            .OrderBy( s => new[] { s.DataSource["common.foo"]["b"].Asc() } )
            .OrderBy( s => new[] { s.DataSource["lorem"]["c"].Desc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        sut.Visit( query );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `cba` AS (
  SELECT * FROM abc
),
`zyx` AS (
  SELECT * FROM xyz JOIN cba ON cba.h = xyz.h
)
SELECT DISTINCT
  `common`.`foo`.`b` AS `x`,
  `lorem`.`c`,
  COUNT(*) AS `v`,
  (SUM(`common`.`foo`.`a`) OVER `wnd1`) AS `w`
FROM `common`.`foo`
INNER JOIN `common`.`bar` AS `lorem` ON `lorem`.`c` = `common`.`foo`.`a`
LEFT JOIN `common`.`qux` ON `common`.`qux`.`e` = `common`.`foo`.`b`
WHERE (`common`.`qux`.`f` > 50) AND (`common`.`foo`.`a` IN (
    SELECT
      `zyx`.`h`
    FROM `zyx`
  ))
GROUP BY `common`.`foo`.`b`, `lorem`.`c`
HAVING (`common`.`foo`.`b` < 100) OR (`lorem`.`c` BETWEEN 0 AND 75)
WINDOW `wnd1` AS (PARTITION BY `common`.`foo`.`a`, `common`.`qux`.`e` ORDER BY `common`.`foo`.`b` ASC),
  `wnd2` AS (ORDER BY `common`.`qux`.`e` ASC, `common`.`qux`.`f` DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
ORDER BY `common`.`foo`.`b` ASC, `lorem`.`c` DESC
LIMIT 50 OFFSET 100" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithRecursiveCommonTableExpression()
    {
        var sut = CreateInterpreter();
        var cba = SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" );
        var zyx = SqlNode.RawQuery( "SELECT * FROM xyz JOIN cba ON cba.h = xyz.h" )
            .ToCte( "zyx" )
            .ToRecursive( SqlNode.RawQuery( "SELECT * FROM zyx" ).ToUnion() );

        var query = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .With( cba, zyx );

        sut.Visit( query );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH RECURSIVE `cba` AS (
  SELECT * FROM abc
),
`zyx` AS (
  
  SELECT * FROM xyz JOIN cba ON cba.h = xyz.h

  UNION
  
  SELECT * FROM zyx

)
SELECT
  foo.`a`
FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretDataSourceQuery_WithLimitOnly()
    {
        var sut = CreateInterpreter();
        var query = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .Limit( SqlNode.Literal( 100 ) );

        sut.Visit( query );

        sut.Context.Sql.ToString()
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
        var sut = CreateInterpreter();
        var query = SqlNode.RawRecordSet( "foo" )
            .ToDataSource()
            .Select( s => new[] { s.From["a"].AsSelf() } )
            .Offset( SqlNode.Literal( 100 ) );

        sut.Visit( query );

        sut.Context.Sql.ToString()
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
        var sut = CreateInterpreter();
        var query = SqlTableMock.Create<int>( "foo", new[] { "a" } )
            .ToRecordSet()
            .ToDataSource()
            .Select( s => new[] { s.GetAll() } );

        sut.VisitChild( query );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  SELECT
    *
  FROM `common`.`foo`
)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCompoundQuery_WithoutTraits()
    {
        var sut = CreateInterpreter();
        var query = SqlNode.RawQuery( "SELECT * FROM foo" )
            .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT * FROM qux" ).ToUnion() );

        sut.Visit( query );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"SELECT * FROM foo
UNION ALL
SELECT * FROM bar
UNION
SELECT * FROM qux" );
    }

    [Fact]
    public void Visit_ShouldInterpretCompoundQuery_WithAllTraits()
    {
        var sut = CreateInterpreter();
        var query = SqlNode.RawQuery( "SELECT foo.* FROM foo JOIN x ON x.a = foo.a" )
            .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT * FROM qux" ).ToUnion() )
            .With( SqlNode.RawQuery( "SELECT * FROM lorem" ).ToCte( "x" ) )
            .OrderBy( SqlNode.RawExpression( "a" ).Asc() )
            .OrderBy( SqlNode.RawExpression( "b" ).Desc() )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 75 ) );

        sut.Visit( query );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH `x` AS (
  SELECT * FROM lorem
)
SELECT foo.* FROM foo JOIN x ON x.a = foo.a
UNION ALL
SELECT * FROM bar
UNION
SELECT * FROM qux
ORDER BY (a) ASC, (b) DESC
LIMIT 50 OFFSET 75" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCompoundQueryWithParentheses()
    {
        var sut = CreateInterpreter();
        var query = SqlNode.RawQuery( "SELECT * FROM foo" )
            .CompoundWith( SqlNode.RawQuery( "SELECT * FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT * FROM qux" ).ToUnion() );

        sut.VisitChild( query );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"(
  
  SELECT * FROM foo

  UNION ALL
  
  SELECT * FROM bar

  UNION
  
  SELECT * FROM qux

)" );
    }

    [Fact]
    public void Visit_ShouldInterpretCompoundQueryComponent()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawQuery( "SELECT * FROM qux" ).ToExcept() );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"EXCEPT
SELECT * FROM qux" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretCompoundQueryComponentWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawQuery( "SELECT * FROM qux" ).ToIntersect() );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INTERSECT
SELECT * FROM qux" );
    }

    [Fact]
    public void Visit_ShouldInterpretDistinctTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.DistinctTrait() );
        sut.Context.Sql.ToString().Should().Be( "DISTINCT" );
    }

    [Fact]
    public void Visit_ShouldInterpretFilterTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.FilterTrait( SqlNode.RawCondition( "foo.a > 10" ), isConjunction: true ) );
        sut.Context.Sql.ToString().Should().Be( "WHERE foo.a > 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretAggregationTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.AggregationTrait( SqlNode.RawExpression( "foo.a" ), SqlNode.RawExpression( "foo.b" ) ) );
        sut.Context.Sql.ToString().Should().Be( "GROUP BY (foo.a), (foo.b)" );
    }

    [Fact]
    public void Visit_ShouldInterpretAggregationFilterTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.AggregationFilterTrait( SqlNode.RawCondition( "foo.a > 10" ), isConjunction: true ) );
        sut.Context.Sql.ToString().Should().Be( "HAVING foo.a > 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretSortTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.SortTrait( SqlNode.RawExpression( "foo.a" ).Asc(), SqlNode.RawExpression( "foo.b" ).Desc() ) );
        sut.Context.Sql.ToString().Should().Be( "ORDER BY (foo.a) ASC, (foo.b) DESC" );
    }

    [Fact]
    public void Visit_ShouldInterpretLimitTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.LimitTrait( SqlNode.Literal( 10 ) ) );
        sut.Context.Sql.ToString().Should().Be( "LIMIT 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretOffsetTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.OffsetTrait( SqlNode.Literal( 10 ) ) );
        sut.Context.Sql.ToString().Should().Be( "OFFSET 10" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpressionTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.CommonTableExpressionTrait(
                SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "A" ),
                SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "B" ) ) );

        sut.Context.Sql.ToString()
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
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.CommonTableExpressionTrait(
                SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "A" ),
                SqlNode.RawQuery( "SELECT * FROM bar" ).ToCte( "B" ).ToRecursive( SqlNode.RawQuery( "SELECT * FROM B" ).ToUnion() ) ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WITH RECURSIVE `A` AS (
  SELECT * FROM foo
),
`B` AS (
  
  SELECT * FROM bar

  UNION
  
  SELECT * FROM B

)" );
    }

    [Fact]
    public void Visit_ShouldInterpretEmptyCommonTableExpressionTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.CommonTableExpressionTrait() );
        sut.Context.Sql.ToString().Should().Be( "WITH" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowDefinitionTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.WindowDefinitionTrait(
                SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "qux.a" ).Asc() } ),
                SqlNode.WindowDefinition(
                    "bar",
                    new SqlExpressionNode[] { SqlNode.RawExpression( "qux.a" ) },
                    new[] { SqlNode.RawExpression( "qux.b" ).Desc() },
                    SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.CurrentRow ) ) ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"WINDOW `foo` AS (ORDER BY (qux.a) ASC),
  `bar` AS (PARTITION BY (qux.a) ORDER BY (qux.b) DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowTrait()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.WindowTrait( SqlNode.WindowDefinition( "foo", new[] { SqlNode.RawExpression( "qux.a" ).Asc() } ) ) );
        sut.Context.Sql.ToString().Should().Be( "OVER `foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretOrderBy()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawExpression( "foo.a" ).Asc() );
        sut.Context.Sql.ToString().Should().Be( "(foo.a) ASC" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommonTableExpression()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.RawQuery( "SELECT * FROM foo" )
                .ToCte( "A" )
                .ToRecursive( SqlNode.RawQuery( "SELECT * FROM A WHERE A.depth < 10" ).ToUnionAll() ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"`A` AS (
  
  SELECT * FROM foo

  UNION ALL
  
  SELECT * FROM A WHERE A.depth < 10

)" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowDefinition()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.WindowDefinition(
                "foo",
                new SqlExpressionNode[] { SqlNode.RawExpression( "qux.a" ), SqlNode.RawExpression( "qux.b" ) },
                new[] { SqlNode.RawExpression( "qux.c" ).Asc(), SqlNode.RawExpression( "qux.d" ).Desc() },
                SqlNode.RowsWindowFrame( SqlWindowFrameBoundary.CurrentRow, SqlWindowFrameBoundary.UnboundedFollowing ) ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                "`foo` AS (PARTITION BY (qux.a), (qux.b) ORDER BY (qux.c) ASC, (qux.d) DESC ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING)" );
    }

    [Fact]
    public void Visit_ShouldInterpretWindowFrame()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.RangeWindowFrame(
                SqlWindowFrameBoundary.Preceding( SqlNode.Literal( 3 ) ),
                SqlWindowFrameBoundary.Following( SqlNode.Literal( 5 ) ) ) );

        sut.Context.Sql.ToString().Should().Be( "RANGE BETWEEN 3 PRECEDING AND 5 FOLLOWING" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenNodeIsCustomWindowFrame()
    {
        var sut = CreateInterpreter();
        var node = new SqlWindowFrameMock();

        var action = Lambda.Of( () => sut.Visit( node ) );

        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
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
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawExpression( "foo.a" ).CastTo( type ) );
        sut.Context.Sql.ToString().Should().Be( $"CAST((foo.a) AS {expectedDbType})" );
    }

    [Fact]
    public void VisitChild_ShouldInterpretTypeCastWithoutParentheses()
    {
        var sut = CreateInterpreter();
        sut.VisitChild( SqlNode.RawExpression( "foo.a" ).CastTo<int>() );
        sut.Context.Sql.ToString().Should().Be( "CAST((foo.a) AS SIGNED)" );
    }

    [Fact]
    public void Visit_ShouldInterpretValues()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.Values(
                new[,]
                {
                    { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) }, { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                } ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"VALUES
('foo', 5),
((bar.a), 25)" );
    }

    [Fact]
    public void Visit_ShouldInterpretRawStatement()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.RawStatement(
                @"INSERT INTO foo (a, b)
VALUES (@a, 1)",
                SqlNode.Parameter<int>( "a" ) ) );

        using ( new AssertionScope() )
        {
            sut.Context.Sql.ToString()
                .Should()
                .Be(
                    @"INSERT INTO foo (a, b)
VALUES (@a, 1)" );

            sut.Context.Parameters.Should()
                .BeSequentiallyEqualTo( new SqlNodeInterpreterContextParameter( "a", TypeNullability.Create<int>(), null ) );
        }
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithValues()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.Values(
                    new[,]
                    {
                        { SqlNode.Literal( "foo" ), SqlNode.Literal( 5 ) }, { SqlNode.RawExpression( "bar.a" ), SqlNode.Literal( 25 ) }
                    } )
                .ToInsertInto( SqlNode.RawRecordSet( "qux" ), r => new[] { r["a"], r["b"] } ) );

        sut.Context.Sql.ToString()
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
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.RawQuery( "SELECT a, b FROM foo" ).ToInsertInto( SqlNode.RawRecordSet( "qux" ), r => new[] { r["a"], r["b"] } ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INSERT INTO qux (`a`, `b`)
SELECT a, b FROM foo" );
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithDataSourceQuery()
    {
        var sut = CreateInterpreter();
        var foo = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).ToRecordSet();
        var bar = SqlTableMock.Create<int>( "bar", new[] { "c", "d" } ).ToRecordSet();
        var wnd = SqlNode.WindowDefinition( "wnd", new[] { foo["a"].Asc() } );

        var query = foo
            .Join( bar.InnerOn( bar["c"] == foo["a"] ) )
            .With( SqlNode.RawQuery( "SELECT * FROM abc" ).ToCte( "cba" ) )
            .Distinct()
            .AndWhere( s => s["common.bar"]["c"].InQuery( SqlNode.RawQuery( "SELECT cba.c FROM cba" ) ) )
            .GroupBy( s => new[] { s["common.foo"]["b"] } )
            .AndHaving( s => s["common.foo"]["b"] < SqlNode.Literal( 100 ) )
            .Window( wnd )
            .Select(
                s => new SqlSelectNode[]
                {
                    s["common.foo"]["b"].As( "a" ), SqlNode.AggregateFunctions.Count( s.GetAll().ToExpression() ).Over( wnd ).As( "b" )
                } )
            .OrderBy( s => new[] { s.DataSource["common.foo"]["b"].Asc() } )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 100 ) );

        sut.Visit( query.ToInsertInto( SqlNode.RawRecordSet( "qux" ), r => new[] { r["a"], r["b"] } ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INSERT INTO qux (`a`, `b`)
WITH `cba` AS (
  SELECT * FROM abc
)
SELECT DISTINCT
  `common`.`foo`.`b` AS `a`,
  (COUNT(*) OVER `wnd`) AS `b`
FROM `common`.`foo`
INNER JOIN `common`.`bar` ON `common`.`bar`.`c` = `common`.`foo`.`a`
WHERE `common`.`bar`.`c` IN (
  SELECT cba.c FROM cba
)
GROUP BY `common`.`foo`.`b`
HAVING `common`.`foo`.`b` < 100
WINDOW `wnd` AS (ORDER BY `common`.`foo`.`a` ASC)
ORDER BY `common`.`foo`.`b` ASC
LIMIT 50 OFFSET 100" );
    }

    [Fact]
    public void Visit_ShouldInterpretInsertIntoWithCompoundQuery()
    {
        var sut = CreateInterpreter();
        var query = SqlNode.RawQuery( "SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a" )
            .CompoundWith( SqlNode.RawQuery( "SELECT a, b FROM bar" ).ToUnionAll(), SqlNode.RawQuery( "SELECT a, b FROM qux" ).ToUnion() )
            .With( SqlNode.RawQuery( "SELECT * FROM ipsum" ).ToCte( "x" ) )
            .OrderBy( SqlNode.RawExpression( "a" ).Asc() )
            .Limit( SqlNode.Literal( 50 ) )
            .Offset( SqlNode.Literal( 75 ) );

        sut.Visit( query.ToInsertInto( SqlNode.RawRecordSet( "lorem" ), r => new[] { r["a"], r["b"] } ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                @"INSERT INTO lorem (`a`, `b`)
WITH `x` AS (
  SELECT * FROM ipsum
)
SELECT foo.a, foo.b FROM foo JOIN x ON x.a = foo.a
UNION ALL
SELECT a, b FROM bar
UNION
SELECT a, b FROM qux
ORDER BY (a) ASC
LIMIT 50 OFFSET 75" );
    }

    [Fact]
    public void Visit_ShouldInterpretValueAssignment()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RawRecordSet( "foo" )["a"].Assign( SqlNode.Literal( 50 ) ) );
        sut.Context.Sql.ToString().Should().Be( "`a` = 50" );
    }

    [Fact]
    public void Visit_ShouldInterpretTruncate()
    {
        var sut = CreateInterpreter();
        var table = SqlTableMock.Create<int>( "foo", new[] { "a" } );
        var node = table.ToRecordSet().ToTruncate();

        sut.Visit( node );

        sut.Context.Sql.ToString().Should().Be( "TRUNCATE TABLE `common`.`foo`" );
    }

    [Fact]
    public void Visit_ShouldInterpretStatementBatch()
    {
        var sut = CreateInterpreter();
        sut.Visit(
            SqlNode.Batch(
                SqlNode.BeginTransaction( IsolationLevel.Serializable ),
                SqlNode.Batch( SqlNode.DropTable( SqlRecordSetInfo.CreateTemporary( "bar" ) ) ),
                SqlNode.RawQuery( "SELECT * FROM foo" ),
                SqlNode.RawQuery( "SELECT * FROM qux" ),
                SqlNode.CommitTransaction() ) );

        sut.Context.Sql.ToString()
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
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.BeginTransaction( isolationLevel ) );

        sut.Context.Sql.ToString()
            .Should()
            .Be(
                $@"SET SESSION TRANSACTION ISOLATION LEVEL {expectedSetSession};
{expectedStartTransaction}" );
    }

    [Fact]
    public void Visit_ShouldInterpretCommitTransaction()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.CommitTransaction() );
        sut.Context.Sql.ToString().Should().Be( "COMMIT" );
    }

    [Fact]
    public void Visit_ShouldInterpretRollbackTransaction()
    {
        var sut = CreateInterpreter();
        sut.Visit( SqlNode.RollbackTransaction() );
        sut.Context.Sql.ToString().Should().Be( "ROLLBACK" );
    }

    [Fact]
    public void Visit_ShouldThrowUnrecognizedSqlNodeException_WhenNodeIsCustom()
    {
        var sut = CreateInterpreter();
        var node = new SqlNodeMock();

        var action = Lambda.Of( () => sut.Visit( node ) );

        action.Should()
            .ThrowExactly<UnrecognizedSqlNodeException>()
            .AndMatch( e => ReferenceEquals( e.Node, node ) && ReferenceEquals( e.Visitor, sut ) );
    }

    [Pure]
    private static MySqlNodeInterpreter CreateInterpreter(MySqlNodeInterpreterOptions? options = null)
    {
        return new MySqlNodeInterpreter(
            options ?? MySqlNodeInterpreterOptions.Default.SetTypeDefinitions( CreateTypeDefinitions() ),
            SqlNodeInterpreterContext.Create() );
    }

    [Pure]
    private static MySqlColumnTypeDefinitionProvider CreateTypeDefinitions()
    {
        var builder = new MySqlColumnTypeDefinitionProviderBuilder();
        builder.Register( new TypeDefinition<TypeMock<int>>( MySqlDbType.Int24 ) );
        builder.Register( new TypeDefinition<TypeMock<uint>>( MySqlDbType.UInt24 ) );
        builder.Register( new TypeDefinition<TypeMock<IEnumerable<char>>>( MySqlDbType.VarString ) );
        builder.Register( new TypeDefinition<TypeMock<ICollection<char>>>( MySqlDataType.VarChar ) );
        builder.Register( new TypeDefinition<TypeMock<IList<char>>>( MySqlDbType.TinyText ) );
        builder.Register( new TypeDefinition<TypeMock<char[]>>( MySqlDbType.Text ) );
        builder.Register( new TypeDefinition<TypeMock<IReadOnlyList<char>>>( MySqlDbType.MediumText ) );
        builder.Register( new TypeDefinition<TypeMock<ICollection<byte>>>( MySqlDataType.VarBinary ) );
        builder.Register( new TypeDefinition<TypeMock<IList<byte>>>( MySqlDbType.TinyBlob ) );
        builder.Register( new TypeDefinition<TypeMock<IEnumerable<byte>>>( MySqlDbType.Blob ) );
        builder.Register( new TypeDefinition<TypeMock<IReadOnlyList<byte>>>( MySqlDbType.MediumBlob ) );
        return builder.Build();
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
