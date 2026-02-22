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
        result.TestEquals( $"{{{sut.GetType().GetDebugString()}}}" ).Go();
    }

    [Fact]
    public void CustomFunctionToString_ShouldReturnTypeInfoAndArguments()
    {
        var sut = new SqlFunctionNodeMock( new[] { SqlNode.Null(), SqlNode.Literal( 5 ) } );
        var result = sut.ToString();
        result.TestEquals( $"{{{sut.GetType().GetDebugString()}}}((NULL), (\"5\" : System.Int32))" ).Go();
    }

    [Fact]
    public void CustomAggregateFunctionToString_ShouldReturnTypeInfoAndArgumentsAndTraits()
    {
        var sut = new SqlAggregateFunctionNodeMock( new[] { SqlNode.Null(), SqlNode.Literal( 5 ) }, Chain<SqlTraitNode>.Empty )
            .AddTrait( SqlNode.DistinctTrait() );

        var result = sut.ToString();

        result.TestEquals(
                $$"""
                  AGG_{{{sut.GetType().GetDebugString()}}}((NULL), ("5" : System.Int32))
                    DISTINCT
                  """ )
            .Go();
    }

    [Fact]
    public void CustomWindowFrameToString_ShouldReturnTypeInfoAndBoundaries()
    {
        var sut = new SqlWindowFrameMock( SqlWindowFrameBoundary.UnboundedPreceding, SqlWindowFrameBoundary.UnboundedFollowing );
        var result = sut.ToString();
        result.TestEquals( $"{{{sut.GetType().GetDebugString()}}} BETWEEN UNBOUNDED PRECEDING AND UNBOUNDED FOLLOWING" ).Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void TypeCast_ShouldCreateTypeCastExpressionNode(bool isNullable)
    {
        var node = SqlNode.Parameter<int>( "foo", isNullable );
        var sut = node.CastTo<long>();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.TypeCast ),
                sut.TargetType.TestEquals( typeof( long ) ),
                sut.TargetTypeDefinition.TestNull(),
                sut.Value.TestRefEquals( node ),
                text.TestEquals( $"CAST(({node}) AS System.Int64)" ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.TypeCast ),
                sut.TargetType.TestEquals( typeof( long ) ),
                sut.TargetTypeDefinition.TestRefEquals( definition ),
                sut.Value.TestRefEquals( node ),
                text.TestEquals( $"CAST(({node}) AS System.Int64)" ) )
            .Go();
    }

    [Fact]
    public void RawExpression_ShouldCreateRawExpressionNode()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawExpression( "foo(@a, @b, 10) + 15", TypeNullability.Create<int>(), parameters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawExpression ),
                sut.Sql.TestEquals( "foo(@a, @b, 10) + 15" ),
                sut.Type.TestEquals( TypeNullability.Create<int>() ),
                sut.Parameters.ToArray().TestSequence( parameters ),
                text.TestEquals( "foo(@a, @b, 10) + 15" ) )
            .Go();
    }

    [Fact]
    public void RawExpression_ShouldCreateRawExpressionNode_WithoutType()
    {
        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawExpression( "foo(@a, @b, 10) + 15", parameters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawExpression ),
                sut.Sql.TestEquals( "foo(@a, @b, 10) + 15" ),
                sut.Type.TestNull(),
                sut.Parameters.ToArray().TestSequence( parameters ),
                text.TestEquals( "foo(@a, @b, 10) + 15" ) )
            .Go();
    }

    [Fact]
    public void Query_FromSelectField_ShouldCreateDataSourceQueryExpressionNodeFromDummy()
    {
        var field = SqlNode.Literal( 1 ).As( "foo" );
        var sut = field.ToQuery();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.DataSource.TestRefEquals( SqlNode.DummyDataSource() ),
                sut.Traits.TestEmpty(),
                sut.Selection.ToArray().TestSequence( [ field ] ),
                text.TestEquals(
                    $"""
                     FROM <DUMMY>
                     SELECT
                       ("1" : System.Int32) AS [foo]
                     """ ) )
            .Go();
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithExpressionAndAlias()
    {
        var expression = SqlNode.Parameter<int>( "foo" );
        var sut = expression.As( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectField ),
                sut.Alias.TestEquals( "bar" ),
                sut.Expression.TestRefEquals( expression ),
                sut.FieldName.TestEquals( "bar" ),
                text.TestEquals( $"({expression}) AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithDataFieldAndAlias()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = dataField.As( "qux" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectField ),
                sut.Alias.TestEquals( "qux" ),
                sut.Expression.TestRefEquals( dataField ),
                sut.FieldName.TestEquals( "qux" ),
                text.TestEquals( $"({dataField}) AS [qux]" ) )
            .Go();
    }

    [Fact]
    public void Select_ShouldCreateSelectFieldNode_WithDataFieldAndWithoutAlias()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = dataField.AsSelf();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectField ),
                sut.Alias.TestNull(),
                sut.Expression.TestRefEquals( dataField ),
                sut.FieldName.TestEquals( "bar" ),
                text.TestEquals( $"({dataField})" ) )
            .Go();
    }

    [Fact]
    public void DataField_SelectFieldNodeConversionOperator_ShouldReturnCorrectNode()
    {
        var dataField = SqlNode.RawRecordSet( "foo" ).GetField( "bar" );
        var sut = ( SqlSelectFieldNode )dataField;
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectField ),
                sut.Alias.TestNull(),
                sut.Expression.TestRefEquals( dataField ),
                sut.FieldName.TestEquals( "bar" ),
                text.TestEquals( $"({dataField})" ) )
            .Go();
    }

    [Fact]
    public void SelectAll_ShouldCreateSelectRecordSetNode_WithRecordSet()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.GetAll();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectRecordSet ),
                sut.RecordSet.TestRefEquals( recordSet ),
                text.TestEquals( "[foo].*" ) )
            .Go();
    }

    [Fact]
    public void SelectAll_ShouldCreateSelectAllNode_WithDataSource()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.GetAll();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectAll ),
                sut.DataSource.TestRefEquals( dataSource ),
                text.TestEquals( "*" ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ dataSource ] ),
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Traits.ToArray().TestEmpty(),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Selection.ToArray().TestSequence( selection ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 1 ),
                text.TestEquals(
                    """
                    FROM [foo]
                    SELECT
                      ([foo].[bar] : ?) AS [x],
                      ([foo].[qux] : ?)
                    """ ) )
            .Go();
    }

    [Fact]
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithEmptySelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Select();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Traits.ToArray().TestEmpty(),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Selection.ToArray().TestEmpty(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 1 ),
                text.TestEquals(
                    """
                    FROM [foo]
                    SELECT
                    """ ) )
            .Go();
    }

    [Fact]
    public void Query_ShouldCreateDataSourceQueryExpressionNode_FromDataSourceNode_WithSingleSelection()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From.GetRawField( "bar", TypeNullability.Create<int>() ).AsSelf();
        var sut = dataSource.Select( selection );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSourceQuery ),
                sut.Traits.ToArray().TestEmpty(),
                sut.DataSource.TestRefEquals( dataSource ),
                sut.Selection.ToArray().TestSequence( [ selection ] ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 1 ),
                text.TestEquals(
                    """
                    FROM [foo]
                    SELECT
                      ([foo].[bar] : System.Int32)
                    """ ) )
            .Go();
    }

    [Fact]
    public void RawQuery_ShouldCreateRawQueryExpressionNode()
    {
        var sql = """
                  SELECT *
                  FROM foo
                  WHERE id = @a AND value > @b
                  """;

        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawQuery( sql, parameters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawQuery ),
                sut.Sql.TestEquals( sql ),
                sut.Parameters.ToArray().TestSequence( parameters ),
                sut.Selection.ToArray().TestEmpty(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 1 ),
                text.TestEquals( sql ) )
            .Go();
    }

    [Fact]
    public void CompoundQuery_ShouldCreateCompoundQueryExpressionNode_WithNonEmptyComponents()
    {
        var query1 = SqlNode.RawQuery(
            """
            SELECT a, b
            FROM foo
            WHERE value > 10
            """ );

        var query2 = SqlNode.RawQuery(
            """
            SELECT a, c AS b
            FROM qux
            WHERE value < 10
            """ );

        var union = query2.ToUnion();
        var sut = query1.CompoundWith( new[] { union }.ToList() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CompoundQuery ),
                sut.FirstQuery.TestRefEquals( query1 ),
                sut.FollowingQueries.ToArray().TestSequence( [ union ] ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 1 ),
                text.TestEquals(
                    """
                    SELECT a, b
                    FROM foo
                    WHERE value > 10
                    UNION
                    SELECT a, c AS b
                    FROM qux
                    WHERE value < 10
                    """ ) )
            .Go();
    }

    [Fact]
    public void CompoundQuery_ShouldThrowArgumentException_WhenComponentsAreEmpty()
    {
        var query = SqlNode.RawQuery(
            """
            SELECT *
            FROM foo
            WHERE value > 10
            """ );

        var action = Lambda.Of( () => query.CompoundWith() );

        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void SwitchCase_ShouldCreateSwitchCaseNode()
    {
        var condition = SqlNode.RawCondition( "@a > 10", SqlNode.Parameter( "a" ) );
        var expression = SqlNode.Literal( 42 );
        var sut = SqlNode.SwitchCase( condition, expression );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SwitchCase ),
                sut.Condition.TestRefEquals( condition ),
                sut.Expression.TestRefEquals( expression ),
                text.TestEquals(
                    $"""
                     WHEN {condition}
                       THEN ({expression})
                     """ ) )
            .Go();
    }

    [Fact]
    public void Switch_ShouldCreateSwitchExpressionNode()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var firstCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar > 10" ), SqlNode.Literal( 10 ) );
        var secondCase = SqlNode.SwitchCase( SqlNode.RawCondition( "bar < 5" ), SqlNode.Literal( 15 ) );
        var sut = SqlNode.Switch( new[] { firstCase, secondCase }, defaultNode );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Switch ),
                sut.Default.TestRefEquals( defaultNode ),
                sut.Cases.ToArray().TestSequence( [ firstCase, secondCase ] ),
                text.TestEquals(
                    $"""
                     CASE
                       WHEN {firstCase.Condition}
                         THEN ({firstCase.Expression})
                       WHEN {secondCase.Condition}
                         THEN ({secondCase.Expression})
                       ELSE ({defaultNode})
                     END
                     """ ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Switch ),
                sut.Default.TestRefEquals( defaultNode ),
                sut.Cases.ToArray().TestSequence( [ firstCase, secondCase ] ),
                text.TestEquals(
                    """
                    CASE
                      WHEN bar > 10
                        THEN (
                          CASE
                            WHEN bar > 20
                              THEN ("20" : System.Int32)
                            WHEN bar > 15
                              THEN ("15" : System.Int32)
                            ELSE ("10" : System.Int32)
                          END
                        )
                      WHEN bar < 5
                        THEN ("5" : System.Int32)
                      ELSE (@foo : System.Int32)
                    END
                    """ ) )
            .Go();
    }

    [Fact]
    public void Switch_ShouldThrowArgumentException_WhenCasesAreEmpty()
    {
        var defaultNode = SqlNode.Parameter<int>( "foo" );
        var action = Lambda.Of( () => SqlNode.Switch( Enumerable.Empty<SqlSwitchCaseNode>(), defaultNode ) );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void Iif_ShouldCreateSwitchExpressionNode()
    {
        var condition = SqlNode.RawCondition( "bar > 10" );
        var whenTrue = SqlNode.Literal( 10 );
        var whenFalse = SqlNode.Literal( 15 );
        var sut = SqlNode.Iif( condition, whenTrue, whenFalse );

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Switch ),
                sut.Default.TestRefEquals( whenFalse ),
                sut.Cases.TestCount( count => count.TestEquals( 1 ) )
                    .Then( c => Assertion.All(
                        c[0].Condition.TestRefEquals( condition ),
                        c[0].Expression.TestRefEquals( whenTrue ) ) ) )
            .Go();
    }

    [Fact]
    public void UnionWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToUnion();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CompoundQueryComponent ),
                sut.Operator.TestEquals( SqlCompoundQueryOperator.Union ),
                sut.Query.TestRefEquals( query ),
                text.TestEquals(
                    """
                    UNION
                    SELECT * FROM foo
                    """ ) )
            .Go();
    }

    [Fact]
    public void UnionAllWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToUnionAll();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CompoundQueryComponent ),
                sut.Operator.TestEquals( SqlCompoundQueryOperator.UnionAll ),
                sut.Query.TestRefEquals( query ),
                text.TestEquals(
                    """
                    UNION ALL
                    SELECT * FROM foo
                    """ ) )
            .Go();
    }

    [Fact]
    public void IntersectWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToIntersect();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CompoundQueryComponent ),
                sut.Operator.TestEquals( SqlCompoundQueryOperator.Intersect ),
                sut.Query.TestRefEquals( query ),
                text.TestEquals(
                    """
                    INTERSECT
                    SELECT * FROM foo
                    """ ) )
            .Go();
    }

    [Fact]
    public void ExceptWith_ShouldCreateSqlCompoundQueryComponentNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToExcept();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CompoundQueryComponent ),
                sut.Operator.TestEquals( SqlCompoundQueryOperator.Except ),
                sut.Query.TestRefEquals( query ),
                text.TestEquals(
                    """
                    EXCEPT
                    SELECT * FROM foo
                    """ ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CompoundQueryComponent ),
                sut.Operator.TestEquals( @operator ),
                sut.Query.TestRefEquals( query ) )
            .Go();
    }

    [Fact]
    public void CompoundWith_ShouldThrowArgumentException_WhenOperatorIsUnrecognized()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var action = Lambda.Of( () => query.ToCompound( ( SqlCompoundQueryOperator )10 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
    }

    [Fact]
    public void SelectExpression_ShouldCreateSelectExpressionNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var selection = dataSource.From.GetRawField( "bar", TypeNullability.Create<int>() ).AsSelf();
        var sut = selection.ToExpression();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.SelectExpression ),
                sut.Selection.TestRefEquals( selection ),
                text.TestEquals( "[bar]" ) )
            .Go();
    }

    [Fact]
    public void Values_From1DArray_ShouldCreateValuesNode()
    {
        var expressions = new[] { SqlNode.Literal( 1 ), SqlNode.Literal( 2 ), SqlNode.Literal( 3 ) };

        var sut = SqlNode.Values( expressions );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Values ),
                sut.RowCount.TestEquals( 1 ),
                sut.ColumnCount.TestEquals( 3 ),
                sut[0].ToArray().TestSequence( expressions ),
                text.TestEquals(
                    """
                    VALUES
                    (("1" : System.Int32), ("2" : System.Int32), ("3" : System.Int32))
                    """ ) )
            .Go();
    }

    [Fact]
    public void Values_From2DArray_ShouldCreateValuesNode()
    {
        var expressions = new[,]
        {
            { SqlNode.Literal( 1 ), SqlNode.Literal( 2 ), SqlNode.Literal( 3 ) },
            { SqlNode.Literal( 4 ), SqlNode.Literal( 5 ), SqlNode.Literal( 6 ) }
        };

        var sut = SqlNode.Values( expressions );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Values ),
                sut.RowCount.TestEquals( 2 ),
                sut.ColumnCount.TestEquals( 3 ),
                sut[0].ToArray().TestSequence( [ expressions[0, 0], expressions[0, 1], expressions[0, 2] ] ),
                sut[1].ToArray().TestSequence( [ expressions[1, 0], expressions[1, 1], expressions[1, 2] ] ),
                text.TestEquals(
                    """
                    VALUES
                    (("1" : System.Int32), ("2" : System.Int32), ("3" : System.Int32)),
                    (("4" : System.Int32), ("5" : System.Int32), ("6" : System.Int32))
                    """ ) )
            .Go();
    }

    [Fact]
    public void RawStatement_ShouldCreateRawStatementNode()
    {
        var sql = """
                  INSERT INTO foo (x, y)
                  VALUES
                  (@a, 10),
                  (@b, 20)
                  """;

        var parameters = new[] { SqlNode.Parameter( "a" ), SqlNode.Parameter( "b" ) };
        var sut = SqlNode.RawStatement( sql, parameters );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawStatement ),
                sut.Sql.TestEquals( sql ),
                sut.Parameters.ToArray().TestSequence( parameters ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( sql ) )
            .Go();
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode()
    {
        var sut = SqlNode.Column<string>( "foo" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ColumnDefinition ),
                sut.Name.TestEquals( "foo" ),
                sut.DefaultValue.TestNull(),
                sut.Computation.TestNull(),
                sut.Type.TestEquals( TypeNullability.Create<string>( isNullable: false ) ),
                sut.TypeDefinition.TestNull(),
                text.TestEquals( "[foo] : System.String" ) )
            .Go();
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithNullableType()
    {
        var sut = SqlNode.Column<string>( "foo", isNullable: true );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ColumnDefinition ),
                sut.Name.TestEquals( "foo" ),
                sut.DefaultValue.TestNull(),
                sut.Computation.TestNull(),
                sut.Type.TestEquals( TypeNullability.Create<string>( isNullable: true ) ),
                sut.TypeDefinition.TestNull(),
                text.TestEquals( "[foo] : Nullable<System.String>" ) )
            .Go();
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDefaultValue()
    {
        var defaultValue = SqlNode.Literal( "abc" ).Concat( SqlNode.Literal( "def" ) );
        var sut = SqlNode.Column<string>( "foo", defaultValue: defaultValue );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ColumnDefinition ),
                sut.Name.TestEquals( "foo" ),
                sut.DefaultValue.TestRefEquals( defaultValue ),
                sut.Computation.TestNull(),
                sut.Type.TestEquals( TypeNullability.Create<string>() ),
                sut.TypeDefinition.TestNull(),
                text.TestEquals( "[foo] : System.String DEFAULT ((\"abc\" : System.String) || (\"def\" : System.String))" ) )
            .Go();
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDbType()
    {
        var typeDef = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = SqlNode.Column( "foo", typeDef );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ColumnDefinition ),
                sut.Name.TestEquals( "foo" ),
                sut.DefaultValue.TestNull(),
                sut.Computation.TestNull(),
                sut.Type.TestEquals( TypeNullability.Create<int>( isNullable: false ) ),
                sut.TypeDefinition.TestRefEquals( typeDef ),
                text.TestEquals( "[foo] : System.Int32" ) )
            .Go();
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDbTypeAndNullableType()
    {
        var typeDef = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var sut = SqlNode.Column( "foo", typeDef, isNullable: true );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ColumnDefinition ),
                sut.Name.TestEquals( "foo" ),
                sut.DefaultValue.TestNull(),
                sut.Computation.TestNull(),
                sut.Type.TestEquals( TypeNullability.Create<int>( isNullable: true ) ),
                sut.TypeDefinition.TestRefEquals( typeDef ),
                text.TestEquals( "[foo] : Nullable<System.Int32>" ) )
            .Go();
    }

    [Fact]
    public void Column_ShouldCreateColumnDefinitionNode_WithDbTypeAndDefaultValue()
    {
        var typeDef = new SqlColumnTypeDefinitionMock<int>( SqlDataTypeMock.Integer, 0 );
        var defaultValue = SqlNode.Literal( "abc" ).Concat( SqlNode.Literal( "def" ) );
        var sut = SqlNode.Column( "foo", typeDef, defaultValue: defaultValue );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ColumnDefinition ),
                sut.Name.TestEquals( "foo" ),
                sut.DefaultValue.TestRefEquals( defaultValue ),
                sut.Computation.TestNull(),
                sut.Type.TestEquals( TypeNullability.Create<int>() ),
                sut.TypeDefinition.TestRefEquals( typeDef ),
                text.TestEquals( "[foo] : System.Int32 DEFAULT ((\"abc\" : System.String) || (\"def\" : System.String))" ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual )]
    [InlineData( SqlColumnComputationStorage.Stored )]
    public void Column_ShouldCreateColumnDefinitionNode_WithComputation(SqlColumnComputationStorage storage)
    {
        var computation = new SqlColumnComputation( SqlNode.Literal( "abc" ), storage );
        var sut = SqlNode.Column<string>( "foo", computation: computation );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ColumnDefinition ),
                sut.Name.TestEquals( "foo" ),
                sut.DefaultValue.TestNull(),
                sut.Computation.TestEquals( computation ),
                sut.Type.TestEquals( TypeNullability.Create<string>() ),
                sut.TypeDefinition.TestNull(),
                text.TestEquals( $"[foo] : System.String GENERATED (\"abc\" : System.String) {storage.ToString().ToUpperInvariant()}" ) )
            .Go();
    }

    [Fact]
    public void PrimaryKey_ShouldCreatePrimaryKeyDefinitionNode()
    {
        var table = SqlNode.RawRecordSet( "foo" );
        var columns = new[] { table["x"].Asc(), table["y"].Desc() };
        var sut = SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "bar", "PK_foo" ), columns );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.PrimaryKeyDefinition ),
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "bar", "PK_foo" ) ),
                sut.Columns.ToArray().TestSequence( columns ),
                text.TestEquals( "PRIMARY KEY [bar].[PK_foo] (([foo].[x] : ?) ASC, ([foo].[y] : ?) DESC)" ) )
            .Go();
    }

    [Fact]
    public void PrimaryKey_ShouldCreatePrimaryKeyDefinitionNode_WithoutColumns()
    {
        var sut = SqlNode.PrimaryKey( SqlSchemaObjectName.Create( "PK_foo" ), Array.Empty<SqlOrderByNode>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.PrimaryKeyDefinition ),
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "PK_foo" ) ),
                sut.Columns.ToArray().TestEmpty(),
                text.TestEquals( "PRIMARY KEY [PK_foo] ()" ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ForeignKeyDefinition ),
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "qux", "FK_foo_REF_bar" ) ),
                sut.Columns.ToArray().TestSequence( columns ),
                sut.ReferencedTable.TestRefEquals( referencedTable ),
                sut.ReferencedColumns.ToArray().TestSequence( referencedColumns ),
                sut.OnDeleteBehavior.TestRefEquals( ReferenceBehavior.Restrict ),
                sut.OnUpdateBehavior.TestRefEquals( ReferenceBehavior.Restrict ),
                text.TestEquals(
                    "FOREIGN KEY [qux].[FK_foo_REF_bar] (([foo].[x] : ?), ([foo].[y] : ?)) REFERENCES [bar] (([bar].[x] : ?), ([bar].[y] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT" ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ForeignKeyDefinition ),
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "FK_foo_REF_bar" ) ),
                sut.Columns.ToArray().TestEmpty(),
                sut.ReferencedTable.TestRefEquals( referencedTable ),
                sut.ReferencedColumns.ToArray().TestEmpty(),
                sut.OnDeleteBehavior.TestRefEquals( onDeleteBehavior ),
                sut.OnUpdateBehavior.TestRefEquals( onUpdateBehavior ),
                text.TestEquals(
                    $"FOREIGN KEY [FK_foo_REF_bar] () REFERENCES [bar] () ON DELETE {onDeleteBehavior.Name} ON UPDATE {onUpdateBehavior.Name}" ) )
            .Go();
    }

    [Fact]
    public void Check_ShouldCreateCheckDefinitionNode()
    {
        var table = SqlNode.RawRecordSet( "foo" );
        var predicate = table["x"] > SqlNode.Literal( 10 );
        var sut = SqlNode.Check( SqlSchemaObjectName.Create( "bar", "CHK_foo" ), predicate );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CheckDefinition ),
                sut.Name.TestEquals( SqlSchemaObjectName.Create( "bar", "CHK_foo" ) ),
                sut.Condition.TestRefEquals( predicate ),
                text.TestEquals( "CHECK [bar].[CHK_foo] (([foo].[x] : ?) > (\"10\" : System.Int32))" ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CreateTable ),
                sut.Info.TestEquals( info ),
                sut.IfNotExists.TestFalse(),
                sut.Columns.ToArray().TestSequence( columns ),
                sut.PrimaryKey.TestRefEquals( primaryKey ),
                sut.ForeignKeys.ToArray().TestSequence( foreignKeys ),
                sut.Checks.ToArray().TestSequence( checks ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    CREATE TABLE [foo].[bar] (
                      [x] : System.Int32,
                      [y] : Nullable<System.String>,
                      [z] : System.Double DEFAULT ("10.5" : System.Double),
                      [a] : System.String GENERATED ("foo" : System.String) VIRTUAL,
                      [b] : Nullable<System.String> GENERATED ("bar" : System.String) STORED,
                      PRIMARY KEY [PK_foobar] (([foo].[bar].[x] : System.Int32) ASC),
                      FOREIGN KEY [FK_foobar_REF_qux] (([foo].[bar].[y] : Nullable<System.String>)) REFERENCES [qux] (([qux].[y] : ?)) ON DELETE RESTRICT ON UPDATE RESTRICT,
                      CHECK [CHK_foobar] (([foo].[bar].[z] : System.Double) > ("100" : System.Double))
                    )
                    """ ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CreateTable ),
                sut.Info.TestEquals( info ),
                sut.IfNotExists.TestEquals( ifNotExists ),
                sut.Columns.ToArray().TestEmpty(),
                sut.PrimaryKey.TestNull(),
                sut.ForeignKeys.ToArray().TestEmpty(),
                sut.Checks.ToArray().TestEmpty(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    $"""
                     {expectedText} (
                     )
                     """ ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CreateView ),
                sut.Info.TestEquals( info ),
                sut.ReplaceIfExists.TestEquals( replaceIfExists ),
                sut.Source.TestRefEquals( source ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    $"""
                     {expectedHeader}
                     SELECT * FROM qux
                     """ ) )
            .Go();
    }

    [Fact]
    public void CreateIndex_ShouldCreateCreateIndexNode()
    {
        var table = SqlNode.RawRecordSet( "qux" );
        var columns = new[] { table["x"].Asc(), table["y"].Desc() };

        var filter = table["x"] > table["y"];
        var name = SqlSchemaObjectName.Create( "foo", "bar" );

        var sut = SqlNode.CreateIndex( name, isUnique: false, table, columns, filter: filter );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CreateIndex ),
                sut.Name.TestEquals( name ),
                sut.ReplaceIfExists.TestFalse(),
                sut.IsUnique.TestFalse(),
                sut.Table.TestRefEquals( table ),
                sut.Columns.ToArray().TestSequence( columns ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    "CREATE INDEX [foo].[bar] ON [qux] (([qux].[x] : ?) ASC, ([qux].[y] : ?) DESC) WHERE (([qux].[x] : ?) > ([qux].[y] : ?))" ) )
            .Go();
    }

    [Fact]
    public void CreateIndex_ShouldCreateCreateIndexNode_WithTemporaryRecordSet()
    {
        var table = SqlNode.CreateTable(
                SqlRecordSetInfo.CreateTemporary( "qux" ),
                new[] { SqlNode.Column<int>( "x" ), SqlNode.Column<int>( "y" ) } )
            .RecordSet;

        var columns = new[] { table["x"].Asc(), table["y"].Desc() };

        var filter = table["x"] > table["y"];
        var name = SqlSchemaObjectName.Create( "foo", "bar" );

        var sut = SqlNode.CreateIndex( name, isUnique: false, table, columns, filter: filter );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CreateIndex ),
                sut.Name.TestEquals( name ),
                sut.ReplaceIfExists.TestFalse(),
                sut.IsUnique.TestFalse(),
                sut.Table.TestRefEquals( table ),
                sut.Columns.ToArray().TestSequence( columns ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    "CREATE INDEX [foo].[bar] ON TEMP.[qux] ((TEMP.[qux].[x] : System.Int32) ASC, (TEMP.[qux].[y] : System.Int32) DESC) WHERE ((TEMP.[qux].[x] : System.Int32) > (TEMP.[qux].[y] : System.Int32))" ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CreateIndex ),
                sut.Name.TestEquals( name ),
                sut.ReplaceIfExists.TestEquals( replaceIfExists ),
                sut.IsUnique.TestEquals( isUnique ),
                sut.Table.TestRefEquals( table ),
                sut.Columns.ToArray().TestEmpty(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RenameTable ),
                sut.Table.TestEquals( table ),
                sut.NewName.TestEquals( newName ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
    }

    [Theory]
    [InlineData( false, "RENAME COLUMN [foo].[bar].[qux] TO [lorem]" )]
    [InlineData( true, "RENAME COLUMN TEMP.[foo].[qux] TO [lorem]" )]
    public void RenameColumn_ShouldCreateRenameColumnNode(bool isTableTemporary, string expectedText)
    {
        var table = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var sut = SqlNode.RenameColumn( table, "qux", "lorem" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RenameColumn ),
                sut.Table.TestEquals( table ),
                sut.OldName.TestEquals( "qux" ),
                sut.NewName.TestEquals( "lorem" ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.AddColumn ),
                sut.Table.TestEquals( table ),
                sut.Definition.TestRefEquals( definition ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
    }

    [Theory]
    [InlineData( false, "DROP COLUMN [foo].[bar].[qux]" )]
    [InlineData( true, "DROP COLUMN TEMP.[foo].[qux]" )]
    public void DropColumn_ShouldCreateDropColumnNode(bool isTableTemporary, string expectedText)
    {
        var table = isTableTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
        var sut = SqlNode.DropColumn( table, "qux" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DropColumn ),
                sut.Table.TestEquals( table ),
                sut.Name.TestEquals( "qux" ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DropTable ),
                sut.Table.TestEquals( table ),
                sut.IfExists.TestEquals( ifExists ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DropView ),
                sut.View.TestEquals( view ),
                sut.IfExists.TestEquals( ifExists ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DropIndex ),
                sut.Table.TestEquals( recordSet.Info ),
                sut.Name.TestEquals( name ),
                sut.IfExists.TestEquals( ifExists ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( expectedText ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.StatementBatch ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                sut.QueryCount.TestEquals( 2 ),
                sut.Statements.ToArray().TestSequence( statements ),
                text.TestEquals(
                    """
                    BATCH
                    (
                      SELECT a, b FROM foo;

                      SELECT b, c FROM bar;

                      INSERT INTO qux (x, y) VALUES (1, 'foo');
                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void Batch_ShouldCreateStatementBatchNode_WithEmptyStatements()
    {
        var sut = SqlNode.Batch();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.StatementBatch ),
                sut.Statements.ToArray().TestEmpty(),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                sut.QueryCount.TestEquals( 0 ),
                text.TestEquals(
                    """
                    BATCH
                    (
                      
                    )
                    """ ) )
            .Go();
    }

    [Theory]
    [InlineData( IsolationLevel.Serializable )]
    [InlineData( IsolationLevel.ReadUncommitted )]
    [InlineData( IsolationLevel.Unspecified )]
    public void BeginTransaction_ShouldCreateBeginTransactionNode(IsolationLevel isolationLevel)
    {
        var sut = SqlNode.BeginTransaction( isolationLevel );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.BeginTransaction ),
                sut.IsolationLevel.TestEquals( isolationLevel ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( $"BEGIN {isolationLevel.ToString().ToUpperInvariant()} TRANSACTION" ) )
            .Go();
    }

    [Fact]
    public void CommitTransaction_ShouldCreateCommitTransactionNode()
    {
        var sut = SqlNode.CommitTransaction();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CommitTransaction ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( "COMMIT" ) )
            .Go();
    }

    [Fact]
    public void RollbackTransaction_ShouldCreateRollbackTransactionNode()
    {
        var sut = SqlNode.RollbackTransaction();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RollbackTransaction ),
                (( ISqlStatementNode )sut).Node.TestRefEquals( sut ),
                (( ISqlStatementNode )sut).QueryCount.TestEquals( 0 ),
                text.TestEquals( "ROLLBACK" ) )
            .Go();
    }
}
