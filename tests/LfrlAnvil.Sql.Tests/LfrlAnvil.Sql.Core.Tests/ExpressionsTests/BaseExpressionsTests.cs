using System.Collections.Generic;
using System.Data;
using System.Linq;
using LfrlAnvil.Extensions;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class BaseExpressionsTests : TestsBase
{
    [Fact]
    public void CustomToString_ShouldReturnTypeInfo()
    {
        var sut = new SqlNodeMock();
        var result = sut.ToString();
        result.Should().Be( $"{{{sut.GetType().GetDebugString()}}}" );
    }

    [Fact]
    public void CustomFunctionToString_ShouldReturnTypeInfoAndArguments()
    {
        var sut = new SqlFunctionNodeMock( new[] { SqlNode.Null(), SqlNode.Literal( 5 ) } );
        var result = sut.ToString();
        result.Should().Be( $"{{{sut.GetType().GetDebugString()}}}((NULL), (\"5\" : System.Int32))" );
    }

    [Fact]
    public void CustomAggregateFunctionToString_ShouldReturnTypeInfoAndArgumentsAndTraits()
    {
        var sut = new SqlAggregateFunctionNodeMock( new[] { SqlNode.Null(), SqlNode.Literal( 5 ) }, Chain<SqlTraitNode>.Empty )
            .AddTrait( SqlNode.DistinctTrait() );

        var result = sut.ToString();

        result.Should()
            .Be(
                $@"AGG_{{{sut.GetType().GetDebugString()}}}((NULL), (""5"" : System.Int32))
  DISTINCT" );
    }

    [Fact]
    public void CustomWindowFrameToString_ShouldReturnTypeInfoAndBoundaries()
    {
        var sut = new SqlWindowFrameMock( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.UnboundedFollowing );
        var result = sut.ToString();
        result.Should().Be( $"{{{sut.GetType().GetDebugString()}}} BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING" );
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void TypeCast_ShouldCreateTypeCastExpressionNode(bool isNullable)
    {
        var node = SqlNode.Parameter<int>( "foo", isNullable );
        var sut = node.CastTo<long>();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.TypeCast );
            sut.TargetType.Should().Be( typeof( long ) );
            sut.TargetTypeDefinition.Should().BeNull();
            sut.Value.Should().BeSameAs( node );
            text.Should().Be( $"CAST(({node}) AS System.Int64)" );
        }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void TypeCast_ShouldCreateTypeCastExpressionNode_WithTypeDefinition(bool isNullable)
    {
        var typeDefinitions = new SqlColumnTypeDefinitionProviderMock( new SqlColumnTypeDefinitionProviderBuilderMock() );
        var definition = typeDefinitions.GetByType<long>();
        var node = SqlNode.Parameter<int>( "foo", isNullable );
        var sut = node.CastTo( definition );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.TypeCast );
            sut.TargetType.Should().Be( typeof( long ) );
            sut.TargetTypeDefinition.Should().BeSameAs( definition );
            sut.Value.Should().BeSameAs( node );
            text.Should().Be( $"CAST(({node}) AS System.Int64)" );
        }
    }

    [Fact]
    public void RawExpression_ShouldCreateRawExpressionNode()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawExpression( "foo(@a, @b, 10) + 15", TypeNullability.Create<int>(), parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawExpression );
            sut.Sql.Should().Be( "foo(@a, @b, 10) + 15" );
            sut.Type.Should().Be( TypeNullability.Create<int>() );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            text.Should().Be( "foo(@a, @b, 10) + 15" );
        }
    }

    [Fact]
    public void RawExpression_ShouldCreateRawExpressionNode_WithoutType()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawExpression( "foo(@a, @b, 10) + 15", parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawExpression );
            sut.Sql.Should().Be( "foo(@a, @b, 10) + 15" );
            sut.Type.Should().BeNull();
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            text.Should().Be( "foo(@a, @b, 10) + 15" );
        }
    }

    [Fact]
    public void Query_FromSelectField_ShouldCreateDataSourceQueryExpressionNodeFromDummy()
    {
        var field = SqlNode.Literal( 1 ).As( "foo" );
        var sut = field.ToQuery();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.DataSource.Should().BeSameAs( SqlNode.DummyDataSource() );
            sut.Traits.Should().BeEmpty();
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( field );
            text.Should()
                .Be(
                    $@"FROM <DUMMY>
SELECT
  (""1"" : System.Int32) AS [foo]" );
        }
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithExpressionAndAlias()
    {
        var expression = SqlNode.Parameter<int>( "foo" );
        var sut = expression.As( "bar" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectField );
            sut.Alias.Should().Be( "bar" );
            sut.Expression.Should().BeSameAs( expression );
            sut.FieldName.Should().Be( "bar" );
            text.Should().Be( $"({expression}) AS [bar]" );
        }
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithDataFieldAndAlias()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = dataField.As( "qux" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectField );
            sut.Alias.Should().Be( "qux" );
            sut.Expression.Should().BeSameAs( dataField );
            sut.FieldName.Should().Be( "qux" );
            text.Should().Be( $"({dataField}) AS [qux]" );
        }
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithDataFieldAndWithoutAlias()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = dataField.AsSelf();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectField );
            sut.Alias.Should().BeNull();
            sut.Expression.Should().BeSameAs( dataField );
            sut.FieldName.Should().Be( "bar" );
            text.Should().Be( $"({dataField})" );
        }
    }

    [Fact]
    public void DataField_SelectFieldNodeConversionOperator_ShouldReturnCorrectNode()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = (SqlSelectFieldNode)dataField;
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectField );
            sut.Alias.Should().BeNull();
            sut.Expression.Should().BeSameAs( dataField );
            sut.FieldName.Should().Be( "bar" );
            text.Should().Be( $"({dataField})" );
        }
    }

    [Fact]
    public void SelectAll_ShouldCreateSelectRecordSetNode_WithRecordSet()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.GetAll();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectRecordSet );
            sut.RecordSet.Should().BeSameAs( recordSet );
            text.Should().Be( "[foo].*" );
        }
    }

    [Fact]
    public void SelectAll_ShouldCreateSelectAllNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.GetAll();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectAll );
            sut.DataSource.Should().BeSameAs( dataSource );
            text.Should().Be( "*" );
        }
    }

    [Fact]
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithNonEmptySelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = new[] { dataSource.From["bar"].As( "x" ), dataSource.From["qux"].AsSelf() }.ToList();
        var selector = Substitute.For<Func<SqlSingleDataSourceNode<SqlRawRecordSetNode>, IEnumerable<SqlSelectNode>>>();
        selector.WithAnyArgs( _ => selection );
        var sut = dataSource.Select( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( dataSource );
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Traits.ToArray().Should().BeEmpty();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( selection );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 1 );
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT
  ([foo].[bar] : ?) AS [x],
  ([foo].[qux] : ?)" );
        }
    }

    [Fact]
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithEmptySelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Select();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Traits.ToArray().Should().BeEmpty();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeEmpty();
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 1 );
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT" );
        }
    }

    [Fact]
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithSingleSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From.GetRawField( "bar", TypeNullability.Create<int>() ).AsSelf();
        var sut = dataSource.Select( selection );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSourceQuery );
            sut.Traits.ToArray().Should().BeEmpty();
            sut.DataSource.Should().BeSameAs( dataSource );
            sut.Selection.ToArray().Should().BeSequentiallyEqualTo( selection );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 1 );
            text.Should()
                .Be(
                    @"FROM [foo]
SELECT
  ([foo].[bar] : System.Int32)" );
        }
    }

    [Fact]
    public void RawQuery_ShouldCreateRawQueryExpressionNode()
    {
        var sql = @"SELECT *
FROM foo
WHERE id = @a AND value > @b";

        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawQuery( sql, parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawQuery );
            sut.Sql.Should().Be( sql );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            sut.Selection.ToArray().Should().BeEmpty();
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 1 );
            text.Should().Be( sql );
        }
    }

    [Fact]
    public void CompoundQuery_ShouldCreateCompoundQueryExpressionNode_WithNonEmptyComponents()
    {
        var query1 = SqlNode.RawQuery(
            @"SELECT a, b
FROM foo
WHERE value > 10" );

        var query2 = SqlNode.RawQuery(
            @"SELECT a, c AS b
FROM qux
WHERE value < 10" );

        var union = query2.ToUnion();
        var sut = query1.CompoundWith( new[] { union }.ToList() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQuery );
            sut.FirstQuery.Should().BeSameAs( query1 );
            sut.FollowingQueries.ToArray().Should().BeSequentiallyEqualTo( union );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 1 );
            text.Should()
                .Be(
                    @"SELECT a, b
FROM foo
WHERE value > 10
UNION
SELECT a, c AS b
FROM qux
WHERE value < 10" );
        }
    }

    [Fact]
    public void CompoundQuery_ShouldThrowArgumentException_WhenComponentsAreEmpty()
    {
        var query = SqlNode.RawQuery(
            @"SELECT *
FROM foo
WHERE value > 10" );

        var action = Lambda.Of( () => query.CompoundWith() );

        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void SwitchCase_ShouldCreateSwitchCaseNode()
    {
        var condition = SqlNode.RawCondition( "@a > 10", SqlNode.Parameter( "a" ) );
        var expression = SqlNode.Literal( 42 );
        var sut = SqlNode.SwitchCase( condition, expression );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SwitchCase );
            sut.Condition.Should().BeSameAs( condition );
            sut.Expression.Should().BeSameAs( expression );
            text.Should()
                .Be(
                    $@"WHEN {condition}
  THEN ({expression})" );
        }
    }

    [Fact]
    public void Switch_ShouldCreateSwitchExpressionNode()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var firstCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 10" ), SqlNode.Literal( 10 ) );
        var secondCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar < 5" ), SqlNode.Literal( 15 ) );
        var sut = SqlNode.Switch( new[] { firstCase, secondCase }, defaultNode );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Switch );
            sut.Default.Should().BeSameAs( defaultNode );
            sut.Cases.ToArray().Should().BeSequentiallyEqualTo( firstCase, secondCase );
            text.Should()
                .Be(
                    $@"CASE
  WHEN {firstCase.Condition}
    THEN ({firstCase.Expression})
  WHEN {secondCase.Condition}
    THEN ({secondCase.Expression})
  ELSE ({defaultNode})
END" );
        }
    }

    [Fact]
    public void Switch_ShouldCreateSwitchExpressionNode_WithAnotherNestedSwitch()
    {
        var nestedSwitch = SqlNode.Switch(
            new[]
            {
                SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 20" ), SqlNode.Literal( 20 ) ),
                SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 15" ), SqlNode.Literal( 15 ) )
            },
            SqlNode.Literal( 10 ) );

        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var firstCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 10" ), nestedSwitch );
        var secondCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar < 5" ), SqlNode.Literal( 5 ) );
        var sut = SqlNode.Switch( new[] { firstCase, secondCase }, defaultNode );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Switch );
            sut.Default.Should().BeSameAs( defaultNode );
            sut.Cases.ToArray().Should().BeSequentiallyEqualTo( firstCase, secondCase );
            text.Should()
                .Be(
                    @"CASE
  WHEN bar > 10
    THEN (
      CASE
        WHEN bar > 20
          THEN (""20"" : System.Int32)
        WHEN bar > 15
          THEN (""15"" : System.Int32)
        ELSE (""10"" : System.Int32)
      END
    )
  WHEN bar < 5
    THEN (""5"" : System.Int32)
  ELSE (@foo : System.Int32)
END" );
        }
    }

    [Fact]
    public void Switch_ShouldThrowArgumentException_WhenCasesAreEmpty()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var action = Lambda.Of( () => SqlNode.Switch( Enumerable.Empty<SqlSwitchCaseNode>(), defaultNode ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void Iif_ShouldCreateSwitchExpressionNode()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var whenTrue = SqlNode.Literal( 10 );
        var whenFalse = SqlNode.Literal( 15 );
        var sut = SqlNode.Iif( condition, whenTrue, whenFalse );

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Switch );
            sut.Default.Should().BeSameAs( whenFalse );
            sut.Cases.ToArray().Should().HaveCount( 1 );
            (sut.Cases.ToArray().ElementAtOrDefault( 0 )?.Condition).Should().BeSameAs( condition );
            (sut.Cases.ToArray().ElementAtOrDefault( 0 )?.Expression).Should().BeSameAs( whenTrue );
        }
    }

    [Fact]
    public void UnionWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToUnion();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.Union );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"UNION
SELECT * FROM foo" );
        }
    }

    [Fact]
    public void UnionAllWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToUnionAll();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.UnionAll );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"UNION ALL
SELECT * FROM foo" );
        }
    }

    [Fact]
    public void IntersectWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToIntersect();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.Intersect );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"INTERSECT
SELECT * FROM foo" );
        }
    }

    [Fact]
    public void ExceptWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToExcept();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( SqlCompoundQueryOperator.Except );
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"EXCEPT
SELECT * FROM foo" );
        }
    }

    [Theory]
    [InlineData( SqlCompoundQueryOperator.Union )]
    [InlineData( SqlCompoundQueryOperator.UnionAll )]
    [InlineData( SqlCompoundQueryOperator.Intersect )]
    [InlineData( SqlCompoundQueryOperator.Except )]
    public void CompoundWith_ShouldCreateSqlCompoundQueryComponentNode(SqlCompoundQueryOperator @operator)
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToCompound( @operator );

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CompoundQueryComponent );
            sut.Operator.Should().Be( @operator );
            sut.Query.Should().BeSameAs( query );
        }
    }

    [Fact]
    public void CompoundWith_ShouldThrowArgumentException_WhenOperatorIsUnrecognized()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var action = Lambda.Of( () => query.ToCompound( (SqlCompoundQueryOperator)10 ) );
        action.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void SelectExpression_ShouldCreateSelectExpressionNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From.GetRawField( "bar", TypeNullability.Create<int>() ).AsSelf();
        var sut = selection.ToExpression();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.SelectExpression );
            sut.Selection.Should().BeSameAs( selection );
            text.Should().Be( "[bar]" );
        }
    }

    [Fact]
    public void Values_From1DArray_ShouldCreateValuesNode()
    {
        var expressions = new[]
        {
            SqlNode.Literal( 1 ),
            SqlNode.Literal( 2 ),
            SqlNode.Literal( 3 )
        };

        var sut = SqlNode.Values( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Values );
            sut.RowCount.Should().Be( 1 );
            sut.ColumnCount.Should().Be( 3 );
            sut[0].ToArray().Should().BeSequentiallyEqualTo( expressions );
            text.Should()
                .Be(
                    @"VALUES
((""1"" : System.Int32), (""2"" : System.Int32), (""3"" : System.Int32))" );
        }
    }

    [Fact]
    public void Values_From2DArray_ShouldCreateValuesNode()
    {
        var expressions = new[,]
        {
            {
                SqlNode.Literal( 1 ),
                SqlNode.Literal( 2 ),
                SqlNode.Literal( 3 )
            },
            {
                SqlNode.Literal( 4 ),
                SqlNode.Literal( 5 ),
                SqlNode.Literal( 6 )
            }
        };

        var sut = SqlNode.Values( expressions );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Values );
            sut.RowCount.Should().Be( 2 );
            sut.ColumnCount.Should().Be( 3 );
            sut[0].ToArray().Should().BeSequentiallyEqualTo( expressions[0, 0], expressions[0, 1], expressions[0, 2] );
            sut[1].ToArray().Should().BeSequentiallyEqualTo( expressions[1, 0], expressions[1, 1], expressions[1, 2] );
            text.Should()
                .Be(
                    @"VALUES
((""1"" : System.Int32), (""2"" : System.Int32), (""3"" : System.Int32)),
((""4"" : System.Int32), (""5"" : System.Int32), (""6"" : System.Int32))" );
        }
    }

    [Fact]
    public void RawStatement_ShouldCreateRawStatementNode()
    {
        var sql = @"INSERT INTO foo (x, y)
VALUES
(@a, 10),
(@b, 20)";

        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawStatement( sql, parameters );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawStatement );
            sut.Sql.Should().Be( sql );
            sut.Parameters.ToArray().Should().BeSequentiallyEqualTo( parameters );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( sql );
        }
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode()
    {
        var sut = SqlNode.Column<string>( "foo" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ColumnDefinition );
            sut.Name.Should().Be( "foo" );
            sut.DefaultValue.Should().BeNull();
            sut.Computation.Should().BeNull();
            sut.Type.Should().BeEquivalentTo( TypeNullability.Create<string>( isNullable: false ) );
            sut.TypeDefinition.Should().BeNull();
            text.Should().Be( "[foo] : System.String" );
        }
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithNullableType()
    {
        var sut = SqlNode.Column<string>( "foo", isNullable: true );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ColumnDefinition );
            sut.Name.Should().Be( "foo" );
            sut.DefaultValue.Should().BeNull();
            sut.Computation.Should().BeNull();
            sut.Type.Should().BeEquivalentTo( TypeNullability.Create<string>( isNullable: true ) );
            sut.TypeDefinition.Should().BeNull();
            text.Should().Be( "[foo] : Nullable<System.String>" );
        }
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDefaultValue()
    {
        var defaultValue = SqlNode.Literal( "abc" ).Concat( SqlNode.Literal( "def" ) );
        var sut = SqlNode.Column<string>( "foo", defaultValue: defaultValue );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ColumnDefinition );
            sut.Name.Should().Be( "foo" );
            sut.DefaultValue.Should().BeSameAs( defaultValue );
            sut.Computation.Should().BeNull();
            sut.Type.Should().BeEquivalentTo( TypeNullability.Create<string>() );
            sut.TypeDefinition.Should().BeNull();
            text.Should().Be( "[foo] : System.String DEFAULT ((\"abc\" : System.String) || (\"def\" : System.String))" );
        }
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDbType()
    {
        var typeDef = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = SqlNode.Column( "foo", typeDef );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ColumnDefinition );
            sut.Name.Should().Be( "foo" );
            sut.DefaultValue.Should().BeNull();
            sut.Computation.Should().BeNull();
            sut.Type.Should().BeEquivalentTo( TypeNullability.Create<int>( isNullable: false ) );
            sut.TypeDefinition.Should().BeSameAs( typeDef );
            text.Should().Be( "[foo] : System.Int32" );
        }
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDbTypeAndNullableType()
    {
        var typeDef = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = SqlNode.Column( "foo", typeDef, isNullable: true );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ColumnDefinition );
            sut.Name.Should().Be( "foo" );
            sut.DefaultValue.Should().BeNull();
            sut.Computation.Should().BeNull();
            sut.Type.Should().BeEquivalentTo( TypeNullability.Create<int>( isNullable: true ) );
            sut.TypeDefinition.Should().BeSameAs( typeDef );
            text.Should().Be( "[foo] : Nullable<System.Int32>" );
        }
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDbTypeAndDefaultValue()
    {
        var typeDef = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var defaultValue = SqlNode.Literal( "abc" ).Concat( SqlNode.Literal( "def" ) );
        var sut = SqlNode.Column( "foo", typeDef, defaultValue: defaultValue );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ColumnDefinition );
            sut.Name.Should().Be( "foo" );
            sut.DefaultValue.Should().BeSameAs( defaultValue );
            sut.Computation.Should().BeNull();
            sut.Type.Should().BeEquivalentTo( TypeNullability.Create<int>() );
            sut.TypeDefinition.Should().BeSameAs( typeDef );
            text.Should().Be( "[foo] : System.Int32 DEFAULT ((\"abc\" : System.String) || (\"def\" : System.String))" );
        }
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual )]
    [InlineData( SqlColumnComputationStorage.Stored )]
    public void Column_ShouldCreateColumnDefinitionNode_WithComputation(SqlColumnComputationStorage storage)
    {
        var computation = new SqlColumnComputation( SqlNode.Literal( "abc" ), storage );
        var sut = SqlNode.Column<string>( "foo", computation: computation );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ColumnDefinition );
            sut.Name.Should().Be( "foo" );
            sut.DefaultValue.Should().BeNull();
            sut.Computation.Should().Be( computation );
            sut.Type.Should().BeEquivalentTo( TypeNullability.Create<string>() );
            sut.TypeDefinition.Should().BeNull();
            text.Should().Be( $"[foo] : System.String GENERATED (\"abc\" : System.String) {storage.ToString().ToUpperInvariant()}" );
        }
    }

    [Fact]
    public void PrimaryKey_ShouldCreatePrimaryKeyDefinitionNode()
    {
        var table = SqlNode.RawRecordSet( "foo" );
        var columns = new[] { table["x"].Asc(), table["y"].Desc() };
        var sut = SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "bar", "PK_foo" ), columns );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.PrimaryKeyDefinition );
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "bar", "PK_foo" ) );
            sut.Columns.ToArray().Should().BeSequentiallyEqualTo( columns );
            text.Should().Be( "PRIMARY KEY [bar].[PK_foo] (([foo].[x] : ?) ASC, ([foo].[y] : ?) DESC)" );
        }
    }

    [Fact]
    public void PrimaryKey_ShouldCreatePrimaryKeyDefinitionNode_WithoutColumns()
    {
        var sut = SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK_foo" ), Array.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.PrimaryKeyDefinition );
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "PK_foo" ) );
            sut.Columns.ToArray().Should().BeEmpty();
            text.Should().Be( "PRIMARY KEY [PK_foo] ()" );
        }
    }

    [Fact]
    public void ForeignKey_ShouldCreateForeignKeyDefinitionNode()
    {
        var table = SqlNode.RawRecordSet( "foo" );
        var referencedTable = SqlNode.RawRecordSet( "bar" );
        var columns = new SqlDataFieldNode[] { table["x"], table["y"] };
        var referencedColumns = new SqlDataFieldNode[] { referencedTable["x"], referencedTable["y"] };
        var sut = SqlNode.ForeignKey( SqlSchemaObjectName.Create( "qux", "FK_foo_REF_bar" ), columns, referencedTable, referencedColumns );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ForeignKeyDefinition );
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "qux", "FK_foo_REF_bar" ) );
            sut.Columns.ToArray().Should().BeSequentiallyEqualTo( columns );
            sut.ReferencedTable.Should().BeSameAs( referencedTable );
            sut.ReferencedColumns.ToArray().Should().BeSequentiallyEqualTo( referencedColumns );
            sut.OnDeleteBehavior.Should().BeSameAs( ReferenceBehavior.Restrict );
            sut.OnUpdateBehavior.Should().BeSameAs( ReferenceBehavior.Restrict );
            text.Should()
                .Be(
                    "FOREIGN KEY [qux].[FK_foo_REF_bar] (([foo].[x] : ?), ([foo].[y] : ?)) REFERENCES [bar] (([bar].[x] : ?), ([bar].[y] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT" );
        }
    }

    [Theory]
    [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.Cascade, ReferenceBehavior.Values.Restrict )]
    [InlineData( ReferenceBehavior.Values.SetNull, ReferenceBehavior.Values.NoAction )]
    [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Cascade )]
    [InlineData( ReferenceBehavior.Values.Restrict, ReferenceBehavior.Values.Restrict )]
    [InlineData( ReferenceBehavior.Values.NoAction, ReferenceBehavior.Values.SetNull )]
    public void ForeignKey_ShouldCreateForeignKeyDefinitionNode_WithoutColumns(
        ReferenceBehavior.Values onDelete,
        ReferenceBehavior.Values onUpdate)
    {
        var referencedTable = SqlNode.RawRecordSet( "bar" );
        var onDeleteBehavior = ReferenceBehavior.GetBehavior( onDelete );
        var onUpdateBehavior = ReferenceBehavior.GetBehavior( onUpdate );

        var sut = SqlNode.ForeignKey(
            SqlSchemaObjectName.Create( "FK_foo_REF_bar" ),
            Array.Empty<SqlDataFieldNode>(),
            referencedTable,
            Array.Empty<SqlDataFieldNode>(),
            onDeleteBehavior,
            onUpdateBehavior );

        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.ForeignKeyDefinition );
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "FK_foo_REF_bar" ) );
            sut.Columns.ToArray().Should().BeEmpty();
            sut.ReferencedTable.Should().BeSameAs( referencedTable );
            sut.ReferencedColumns.ToArray().Should().BeEmpty();
            sut.OnDeleteBehavior.Should().BeSameAs( onDeleteBehavior );
            sut.OnUpdateBehavior.Should().BeSameAs( onUpdateBehavior );
            text.Should()
                .Be(
                    $"FOREIGN KEY [FK_foo_REF_bar] () REFERENCES [bar] () ON DELETE {onDeleteBehavior.Name} ON UPDATE {onUpdateBehavior.Name}" );
        }
    }

    [Fact]
    public void Check_ShouldCreateCheckDefinitionNode()
    {
        var table = SqlNode.RawRecordSet( "foo" );
        var predicate = table["x"] > SqlNode.Literal( 10 );
        var sut = SqlNode.Check( SqlSchemaObjectName.Create( "bar", "CHK_foo" ), predicate );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CheckDefinition );
            sut.Name.Should().Be( SqlSchemaObjectName.Create( "bar", "CHK_foo" ) );
            sut.Condition.Should().BeSameAs( predicate );
            text.Should().Be( "CHECK [bar].[CHK_foo] (([foo].[x] : ?) > (\"10\" : System.Int32))" );
        }
    }

    [Fact]
    public void CreateTable_ShouldCreateCreateTableNode()
    {
        var columns = new[]
        {
            SqlNode.Column<int>( "x" ),
            SqlNode.Column<string>( "y", isNullable: true ),
            SqlNode.Column<double>( "z", defaultValue: SqlNode.Literal( 10.5 ) ),
            SqlNode.Column<string>( "a", computation: SqlColumnComputation.Virtual( SqlNode.Literal( "foo" ) ) ),
            SqlNode.Column<string>( "b", isNullable: true, computation: SqlColumnComputation.Stored( SqlNode.Literal( "bar" ) ) )
        };

        SqlPrimaryKeyDefinitionNode? primaryKey = null;
        var foreignKeys = Array.Empty<SqlForeignKeyDefinitionNode>();
        var checks = Array.Empty<SqlCheckDefinitionNode>();
        var info = SqlRecordSetInfo.Create( "foo", "bar" );

        var sut = SqlNode.CreateTable(
            info,
            columns,
            constraintsProvider: t =>
            {
                var qux = SqlNode.RawRecordSet( "qux" );
                primaryKey = SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK_foobar" ), new[] { t["x"].Asc() } );
                foreignKeys = new[]
                {
                    SqlNode.ForeignKey(
                        SqlSchemaObjectName.Create( "FK_foobar_REF_qux" ),
                        new SqlDataFieldNode[] { t["y"] },
                        qux,
                        new SqlDataFieldNode[] { qux["y"] } )
                };

                checks = new[] { SqlNode.Check( SqlSchemaObjectName.Create( "CHK_foobar" ), t["z"] > SqlNode.Literal( 100.0 ) ) };

                return SqlCreateTableConstraints.Empty
                    .WithPrimaryKey( primaryKey )
                    .WithForeignKeys( foreignKeys )
                    .WithChecks( checks );
            } );

        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CreateTable );
            sut.Info.Should().Be( info );
            sut.IfNotExists.Should().BeFalse();
            sut.Columns.ToArray().Should().BeSequentiallyEqualTo( columns );
            sut.PrimaryKey.Should().BeSameAs( primaryKey );
            sut.ForeignKeys.ToArray().Should().BeSequentiallyEqualTo( foreignKeys );
            sut.Checks.ToArray().Should().BeSequentiallyEqualTo( checks );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should()
                .Be(
                    @"CREATE TABLE [foo].[bar] (
  [x] : System.Int32,
  [y] : Nullable<System.String>,
  [z] : System.Double DEFAULT (""10,5"" : System.Double),
  [a] : System.String GENERATED (""foo"" : System.String) VIRTUAL,
  [b] : Nullable<System.String> GENERATED (""bar"" : System.String) STORED,
  PRIMARY KEY [PK_foobar] (([foo].[bar].[x] : System.Int32) ASC),
  FOREIGN KEY [FK_foobar_REF_qux] (([foo].[bar].[y] : Nullable<System.String>)) REFERENCES [qux] (([qux].[y] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT,
  CHECK [CHK_foobar] (([foo].[bar].[z] : System.Double) > (""100"" : System.Double))
)" );
        }
    }

    [Theory]
    [InlineData( false, false, "CREATE TABLE [foo]" )]
    [InlineData( true, false, "CREATE TABLE TEMP.[foo]" )]
    [InlineData( false, true, "CREATE TABLE IF NOT EXISTS [foo]" )]
    [InlineData( true, true, "CREATE TABLE IF NOT EXISTS TEMP.[foo]" )]
    public void CreateTable_ShouldCreateCreateTableNode_WithEmptyColumnsAndNoConstraints(
        bool isTemporary,
        bool ifNotExists,
        string expectedText)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo" );
        var sut = SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>(), ifNotExists );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CreateTable );
            sut.Info.Should().Be( info );
            sut.IfNotExists.Should().Be( ifNotExists );
            sut.Columns.ToArray().Should().BeEmpty();
            sut.PrimaryKey.Should().BeNull();
            sut.ForeignKeys.ToArray().Should().BeEmpty();
            sut.Checks.ToArray().Should().BeEmpty();
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should()
                .Be(
                    $@"{expectedText} (
)" );
        }
    }

    [Theory]
    [InlineData( false, true, "CREATE OR REPLACE VIEW [foo].[bar] AS" )]
    [InlineData( true, true, "CREATE OR REPLACE VIEW TEMP.[foo] AS" )]
    [InlineData( false, false, "CREATE VIEW [foo].[bar] AS" )]
    [InlineData( true, false, "CREATE VIEW TEMP.[foo] AS" )]
    public void CreateView_ShouldCreateCreateViewNode(bool isTemporary, bool replaceIfExists, string expectedHeader)
    {
        var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var source = SqlNode.RawQuery( "SELECT * FROM qux" );
        var sut = source.ToCreateView( info, replaceIfExists );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CreateView );
            sut.Info.Should().Be( info );
            sut.ReplaceIfExists.Should().Be( replaceIfExists );
            sut.Source.Should().BeSameAs( source );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should()
                .Be(
                    $@"{expectedHeader}
SELECT * FROM qux" );
        }
    }

    [Fact]
    public void CreateIndex_ShouldCreateCreateIndexNode()
    {
        var table = SqlNode.RawRecordSet( "qux" );
        var columns = new[]
        {
            table["x"].Asc(),
            table["y"].Desc()
        };

        var filter = table["x"] > table["y"];
        var name = SqlSchemaObjectName.Create( "foo", "bar" );

        var sut = SqlNode.CreateIndex( name, isUnique: false, table, columns, filter: filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CreateIndex );
            sut.Name.Should().Be( name );
            sut.ReplaceIfExists.Should().BeFalse();
            sut.IsUnique.Should().BeFalse();
            sut.Table.Should().BeSameAs( table );
            sut.Columns.ToArray().Should().BeSequentiallyEqualTo( columns );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should()
                .Be(
                    "CREATE INDEX [foo].[bar] ON [qux] (([qux].[x] : ?) ASC, ([qux].[y] : ?) DESC) WHERE (([qux].[x] : ?) > ([qux].[y] : ?))" );
        }
    }

    [Fact]
    public void CreateIndex_ShouldCreateCreateIndexNode_WithTemporaryRecordSet()
    {
        var table = SqlNode.CreateTable(
                SqlRecordSetInfo.CreateTemporary( "qux" ),
                new[] { SqlNode.Column<int>( "x" ), SqlNode.Column<int>( "y" ) } )
            .RecordSet;

        var columns = new[]
        {
            table["x"].Asc(),
            table["y"].Desc()
        };

        var filter = table["x"] > table["y"];
        var name = SqlSchemaObjectName.Create( "foo", "bar" );

        var sut = SqlNode.CreateIndex( name, isUnique: false, table, columns, filter: filter );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CreateIndex );
            sut.Name.Should().Be( name );
            sut.ReplaceIfExists.Should().BeFalse();
            sut.IsUnique.Should().BeFalse();
            sut.Table.Should().BeSameAs( table );
            sut.Columns.ToArray().Should().BeSequentiallyEqualTo( columns );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should()
                .Be(
                    "CREATE INDEX [foo].[bar] ON TEMP.[qux] ((TEMP.[qux].[x] : System.Int32) ASC, (TEMP.[qux].[y] : System.Int32) DESC) WHERE ((TEMP.[qux].[x] : System.Int32) > (TEMP.[qux].[y] : System.Int32))" );
        }
    }

    [Theory]
    [InlineData( false, false, "CREATE INDEX [foo] ON [bar] ()" )]
    [InlineData( false, true, "CREATE UNIQUE INDEX [foo] ON [bar] ()" )]
    [InlineData( true, false, "CREATE OR REPLACE INDEX [foo] ON [bar] ()" )]
    [InlineData( true, true, "CREATE OR REPLACE UNIQUE INDEX [foo] ON [bar] ()" )]
    public void CreateIndex_ShouldCreateCreateIndexNode_WithEmptyColumnsAndNoFilter(
        bool replaceIfExists,
        bool isUnique,
        string expectedText)
    {
        var name = SqlSchemaObjectName.Create( "foo" );
        var table = SqlNode.RawRecordSet( "bar" );
        var sut = SqlNode.CreateIndex( name, isUnique, table, Array.Empty<SqlOrderByNode>(), replaceIfExists );

        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CreateIndex );
            sut.Name.Should().Be( name );
            sut.ReplaceIfExists.Should().Be( replaceIfExists );
            sut.IsUnique.Should().Be( isUnique );
            sut.Table.Should().BeSameAs( table );
            sut.Columns.ToArray().Should().BeEmpty();
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Theory]
    [InlineData( false, "RENAME TABLE [foo].[bar] TO [foo].[qux]" )]
    [InlineData( true, "RENAME TABLE TEMP.[foo] TO [qux]" )]
    public void RenameTable_ShouldCreateRenameTableNode(bool isTemporary, string expectedText)
    {
        var table = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var newName = SqlSchemaObjectName.Create( table.Name.Schema, "qux" );
        var sut = SqlNode.RenameTable( table, newName );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RenameTable );
            sut.Table.Should().Be( table );
            sut.NewName.Should().Be( newName );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Theory]
    [InlineData( false, "RENAME COLUMN [foo].[bar].[qux] TO [lorem]" )]
    [InlineData( true, "RENAME COLUMN TEMP.[foo].[qux] TO [lorem]" )]
    public void RenameColumn_ShouldCreateRenameColumnNode(bool isTableTemporary, string expectedText)
    {
        var table = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var sut = SqlNode.RenameColumn( table, "qux", "lorem" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RenameColumn );
            sut.Table.Should().Be( table );
            sut.OldName.Should().Be( "qux" );
            sut.NewName.Should().Be( "lorem" );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Theory]
    [InlineData( false, "ADD COLUMN [foo].[bar].[qux] : System.Int32 DEFAULT (\"10\" : System.Int32)" )]
    [InlineData( true, "ADD COLUMN TEMP.[foo].[qux] : System.Int32 DEFAULT (\"10\" : System.Int32)" )]
    public void AddColumn_ShouldCreateAddColumnNode(bool isTableTemporary, string expectedText)
    {
        var table = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var definition = SqlNode.Column<int>( "qux", defaultValue: SqlNode.Literal( 10 ) );
        var sut = SqlNode.AddColumn( table, definition );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.AddColumn );
            sut.Table.Should().Be( table );
            sut.Definition.Should().BeSameAs( definition );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Theory]
    [InlineData( false, "DROP COLUMN [foo].[bar].[qux]" )]
    [InlineData( true, "DROP COLUMN TEMP.[foo].[qux]" )]
    public void DropColumn_ShouldCreateDropColumnNode(bool isTableTemporary, string expectedText)
    {
        var table = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var sut = SqlNode.DropColumn( table, "qux" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DropColumn );
            sut.Table.Should().Be( table );
            sut.Name.Should().Be( "qux" );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Theory]
    [InlineData( false, false, "DROP TABLE [foo].[bar]" )]
    [InlineData( true, false, "DROP TABLE TEMP.[foo]" )]
    [InlineData( false, true, "DROP TABLE IF EXISTS [foo].[bar]" )]
    [InlineData( true, true, "DROP TABLE IF EXISTS TEMP.[foo]" )]
    public void DropTable_ShouldCreateDropTableNode(bool isTemporary, bool ifExists, string expectedText)
    {
        var table = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var sut = SqlNode.CreateTable( table, Array.Empty<SqlColumnDefinitionNode>() ).ToDropTable( ifExists );

        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DropTable );
            sut.Table.Should().Be( table );
            sut.IfExists.Should().Be( ifExists );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Theory]
    [InlineData( false, false, "DROP VIEW [foo].[bar]" )]
    [InlineData( true, false, "DROP VIEW TEMP.[foo]" )]
    [InlineData( false, true, "DROP VIEW IF EXISTS [foo].[bar]" )]
    [InlineData( true, true, "DROP VIEW IF EXISTS TEMP.[foo]" )]
    public void DropView_ShouldCreateDropViewNode(bool isTemporary, bool ifExists, string expectedText)
    {
        var view = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var sut = SqlNode.CreateView( view, SqlNode.RawQuery( "SELECT * FROM qux" ) ).ToDropView( ifExists );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DropView );
            sut.View.Should().Be( view );
            sut.IfExists.Should().Be( ifExists );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Theory]
    [InlineData( false, false, "DROP INDEX [foo].[bar] ON [qux]" )]
    [InlineData( true, false, "DROP INDEX IF EXISTS [foo].[bar] ON [qux]" )]
    [InlineData( false, true, "DROP INDEX [foo].[bar] ON TEMP.[qux]" )]
    [InlineData( true, true, "DROP INDEX IF EXISTS [foo].[bar] ON TEMP.[qux]" )]
    public void DropIndex_ShouldCreateDropIndexNode(bool ifExists, bool isRecordSetTemporary, string expectedText)
    {
        SqlRecordSetNode recordSet = isRecordSetTemporary
            ? SqlNode.CreateTable( SqlRecordSetInfo.CreateTemporary( "qux" ), Array.Empty<SqlColumnDefinitionNode>() ).RecordSet
            : SqlNode.RawRecordSet( "qux" );

        var name = SqlSchemaObjectName.Create( "foo", "bar" );
        var sut = SqlNode.CreateIndex( name, isUnique: Fixture.Create<bool>(), recordSet, Array.Empty<SqlOrderByNode>() )
            .ToDropIndex( ifExists );

        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DropIndex );
            sut.Table.Should().Be( recordSet.Info );
            sut.Name.Should().Be( name );
            sut.IfExists.Should().Be( ifExists );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( expectedText );
        }
    }

    [Fact]
    public void Batch_ShouldCreateStatementBatchNode()
    {
        var statements = new ISqlStatementNode[]
        {
            SqlNode.RawQuery( "SELECT a, b FROM foo" ),
            SqlNode.RawQuery( "SELECT b, c FROM bar" ),
            SqlNode.RawStatement( "INSERT INTO qux (x, y) VALUES (1, 'foo')" )
        };

        var sut = SqlNode.Batch( statements );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.StatementBatch );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            sut.QueryCount.Should().Be( 2 );
            sut.Statements.ToArray().Should().BeSequentiallyEqualTo( statements );
            text.Should()
                .Be(
                    @"BATCH
(
  SELECT a, b FROM foo;

  SELECT b, c FROM bar;

  INSERT INTO qux (x, y) VALUES (1, 'foo');
)" );
        }
    }

    [Fact]
    public void Batch_ShouldCreateStatementBatchNode_WithEmptyStatements()
    {
        var sut = SqlNode.Batch();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.StatementBatch );
            sut.Statements.ToArray().Should().BeEmpty();
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            sut.QueryCount.Should().Be( 0 );
            text.Should()
                .Be(
                    @"BATCH
(
  
)" );
        }
    }

    [Theory]
    [InlineData( IsolationLevel.Serializable )]
    [InlineData( IsolationLevel.ReadUncommitted )]
    [InlineData( IsolationLevel.Unspecified )]
    public void BeginTransaction_ShouldCreateBeginTransactionNode(IsolationLevel isolationLevel)
    {
        var sut = SqlNode.BeginTransaction( isolationLevel );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.BeginTransaction );
            sut.IsolationLevel.Should().Be( isolationLevel );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( $"BEGIN {isolationLevel.ToString().ToUpperInvariant()} TRANSACTION" );
        }
    }

    [Fact]
    public void CommitTransaction_ShouldCreateCommitTransactionNode()
    {
        var sut = SqlNode.CommitTransaction();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CommitTransaction );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( "COMMIT" );
        }
    }

    [Fact]
    public void RollbackTransaction_ShouldCreateRollbackTransactionNode()
    {
        var sut = SqlNode.RollbackTransaction();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RollbackTransaction );
            ((ISqlStatementNode)sut).Node.Should().BeSameAs( sut );
            ((ISqlStatementNode)sut).QueryCount.Should().Be( 0 );
            text.Should().Be( "ROLLBACK" );
        }
    }
}
