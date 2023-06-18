using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.NSubstitute;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests : TestsBase
{
    [Fact]
    public void Literal_WithRefType_ShouldCreateLiteralNode_WhenValueIsNotNull()
    {
        var sut = SqlNode.Literal( "foo" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Literal );
            sut.Type.Should().Be( SqlExpressionType.Create<string>() );
            text.Should().Be( "\"foo\" : System.String" );
            var literalNode = sut as SqlLiteralNode<string>;
            (literalNode?.Value).Should().Be( "foo" );
        }
    }

    [Fact]
    public void Literal_WithRefType_ShouldCreateNullNode_WhenValueIsNull()
    {
        var sut = SqlNode.Literal<string>( null );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Null );
            sut.Type.Should().Be( SqlExpressionType.Create<DBNull>() );
            text.Should().Be( "NULL" );
        }
    }

    [Fact]
    public void Literal_WithValueType_ShouldCreateLiteralNode()
    {
        var sut = SqlNode.Literal( 42 );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Literal );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            text.Should().Be( "\"42\" : System.Int32" );
            var literalNode = sut as SqlLiteralNode<int>;
            (literalNode?.Value).Should().Be( 42 );
        }
    }

    [Fact]
    public void Literal_WithNullableValueType_ShouldCreateLiteralNode_WhenValueIsNotNull()
    {
        var sut = SqlNode.Literal( (int?)42 );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Literal );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            text.Should().Be( "\"42\" : System.Int32" );
            var literalNode = sut as SqlLiteralNode<int>;
            (literalNode?.Value).Should().Be( 42 );
        }
    }

    [Fact]
    public void Literal_WithNullableValueType_ShouldCreateNullNode_WhenValueIsNull()
    {
        var sut = SqlNode.Literal<int>( null );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Null );
            sut.Type.Should().Be( SqlExpressionType.Create<DBNull>() );
            text.Should().Be( "NULL" );
        }
    }

    [Fact]
    public void Null_ShouldCreateNullNode()
    {
        var sut = SqlNode.Null();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Null );
            sut.Type.Should().Be( SqlExpressionType.Create<DBNull>() );
            text.Should().Be( "NULL" );
        }
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Parameter_ShouldCreateParameterNode_WithNonNullableType(bool isNullable)
    {
        var expectedType = SqlExpressionType.Create<string>( isNullable );
        var sut = SqlNode.Parameter<string>( "foo", isNullable );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Parameter );
            sut.Name.Should().Be( "foo" );
            sut.Type.Should().Be( expectedType );
            text.Should().Be( $"@foo : {expectedType}" );
        }
    }

    [Fact]
    public void Parameter_ShouldCreateParameterNode_WithNullableValueType()
    {
        var sut = SqlNode.Parameter<int?>( "foo" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Parameter );
            sut.Name.Should().Be( "foo" );
            sut.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
            text.Should().Be( "@foo : Nullable<System.Int32>" );
        }
    }

    [Fact]
    public void Parameter_ShouldCreateParameterNode_WithNoType()
    {
        var sut = SqlNode.Parameter( "foo" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.Parameter );
            sut.Name.Should().Be( "foo" );
            sut.Type.Should().BeNull();
            text.Should().Be( "@foo : ?" );
        }
    }

    [Fact]
    public void Table_ShouldCreateTableRecordSetNode()
    {
        var table = Substitute.For<ISqlTable>();
        table.FullName.Returns( "foo" );
        var sut = table.ToRecordSet();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.Name.Should().Be( "foo" );
            sut.Table.Should().BeSameAs( table );
            sut.IsAliased.Should().BeFalse();
            sut.IsOptional.Should().BeFalse();
            text.Should().Be( "[foo]" );
        }
    }

    [Fact]
    public void Table_ShouldCreateTableRecordSetNode_WithAlias()
    {
        var table = Substitute.For<ISqlTable>();
        table.FullName.Returns( "foo" );
        var sut = table.ToRecordSet( "bar" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.Name.Should().Be( "bar" );
            sut.Table.Should().BeSameAs( table );
            sut.IsAliased.Should().BeTrue();
            sut.IsOptional.Should().BeFalse();
            text.Should().Be( "[foo] AS [bar]" );
        }
    }

    [Fact]
    public void RawRecordSet_ShouldCreateRawRecordSetNode()
    {
        var sut = SqlNode.RawRecordSet( "foo" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.BaseName.Should().Be( "foo" );
            sut.Name.Should().Be( "foo" );
            sut.IsAliased.Should().BeFalse();
            sut.IsOptional.Should().BeFalse();
            text.Should().Be( "[foo]" );
        }
    }

    [Fact]
    public void RawRecordSet_ShouldCreateRawRecordSetNode_WithAlias()
    {
        var sut = SqlNode.RawRecordSet( "foo", "bar" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.BaseName.Should().Be( "foo" );
            sut.Name.Should().Be( "bar" );
            sut.IsAliased.Should().BeTrue();
            sut.IsOptional.Should().BeFalse();
            text.Should().Be( "[foo] AS [bar]" );
        }
    }

    [Fact]
    public void RawDataField_ShouldCreateRawDataFieldNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = SqlNode.RawDataField( recordSet, "bar" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawDataField );
            sut.RecordSet.Should().BeSameAs( recordSet );
            sut.Name.Should().Be( "bar" );
            sut.Type.Should().BeNull();
            text.Should().Be( "[foo].[bar] : ?" );
        }
    }

    [Fact]
    public void RawDataField_ShouldCreateRawDataFieldNode_WithType()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = SqlNode.RawDataField( recordSet, "bar", SqlExpressionType.Create<int>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RawDataField );
            sut.RecordSet.Should().BeSameAs( recordSet );
            sut.Name.Should().Be( "bar" );
            sut.Type.Should().Be( SqlExpressionType.Create<int>() );
            text.Should().Be( "[foo].[bar] : System.Int32" );
        }
    }

    [Fact]
    public void QueryRecordSet_FromDataSourceQuery_ShouldCreateQueryRecordSetNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select( dataSource.From["bar"].As( "x" ), dataSource.From["qux"].AsSelf() );
        var sut = query.AsSet( "lorem" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.Query.Should().BeSameAs( query );
            sut.Name.Should().Be( "lorem" );
            sut.IsOptional.Should().BeFalse();
            sut.IsAliased.Should().BeTrue();
            text.Should()
                .Be(
                    @"(
    FROM [foo]
    SELECT
        ([foo].[bar] : ?) AS [x],
        ([foo].[qux] : ?)
) AS [lorem]" );
        }
    }

    [Fact]
    public void QueryRecordSet_FromRawQuery_ShouldCreateQueryRecordSetNode()
    {
        var query = SqlNode.RawQuery(
            @"SELECT *
FROM foo
WHERE value > 10" );

        var sut = query.AsSet( "bar" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.Name.Should().Be( "bar" );
            sut.IsAliased.Should().BeTrue();
            sut.IsOptional.Should().BeFalse();
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"(
    SELECT *
    FROM foo
    WHERE value > 10
) AS [bar]" );
        }
    }

    [Fact]
    public void QueryRecordSet_FromCompoundQuery_ShouldCreateQueryRecordSetNode()
    {
        var query1 = SqlNode.RawQuery(
            @"SELECT a, b
FROM foo
WHERE value > 10" );

        var query2 = SqlNode.RawQuery(
            @"SELECT a, c AS b
FROM qux
WHERE value < 10" );

        var query = query1.CompoundWith( query2.ToUnion() );
        var sut = query.AsSet( "bar" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.Name.Should().Be( "bar" );
            sut.IsAliased.Should().BeTrue();
            sut.IsOptional.Should().BeFalse();
            sut.Query.Should().BeSameAs( query );
            text.Should()
                .Be(
                    @"(
    (
        SELECT a, b
        FROM foo
        WHERE value > 10
    )
    UNION
    (
        SELECT a, c AS b
        FROM qux
        WHERE value < 10
    )
) AS [bar]" );
        }
    }

    [Fact]
    public void SingleDataSource_ShouldCreateSingleDataSourceNode()
    {
        var from = SqlNode.RawRecordSet( "foo" );
        var sut = from.ToDataSource();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( from );
            sut.Joins.ToArray().Should().BeEmpty();
            sut.RecordSets.Should().BeSequentiallyEqualTo( from );
            text.Should().Be( "FROM [foo]" );
        }
    }

    [Fact]
    public void InnerJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.InnerOn( condition );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.JoinOn );
            sut.JoinType.Should().Be( SqlJoinType.Inner );
            sut.InnerRecordSet.Should().BeSameAs( recordSet );
            sut.OnExpression.Should().BeSameAs( condition );
            text.Should()
                .Be(
                    @"INNER JOIN [foo] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void LeftJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.LeftOn( condition );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.JoinOn );
            sut.JoinType.Should().Be( SqlJoinType.Left );
            sut.InnerRecordSet.Should().BeSameAs( recordSet );
            sut.OnExpression.Should().BeSameAs( condition );
            text.Should()
                .Be(
                    @"LEFT JOIN [foo] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void RightJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.RightOn( condition );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.JoinOn );
            sut.JoinType.Should().Be( SqlJoinType.Right );
            sut.InnerRecordSet.Should().BeSameAs( recordSet );
            sut.OnExpression.Should().BeSameAs( condition );
            text.Should()
                .Be(
                    @"RIGHT JOIN [foo] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void FullJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.FullOn( condition );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.JoinOn );
            sut.JoinType.Should().Be( SqlJoinType.Full );
            sut.InnerRecordSet.Should().BeSameAs( recordSet );
            sut.OnExpression.Should().BeSameAs( condition );
            text.Should()
                .Be(
                    @"FULL JOIN [foo] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void CrossJoin_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.Cross();
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.JoinOn );
            sut.JoinType.Should().Be( SqlJoinType.Cross );
            sut.InnerRecordSet.Should().BeSameAs( recordSet );
            sut.OnExpression.Should().BeSameAs( SqlNode.True() );
            text.Should().Be( "CROSS JOIN [foo]" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithRecordSetAndNodes()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var joinOn = inner.InnerOn( SqlNode.True() );
        var sut = recordSet.Join( new[] { joinOn }.ToList() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( recordSet );
            sut.RecordSets.Should().HaveCount( 2 );
            sut.RecordSets.Should().BeEquivalentTo( recordSet, inner );
            sut.Joins.ToArray().Should().BeSequentiallyEqualTo( joinOn );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] AS [qux] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithRecordSetAndDefinitions()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var sut = recordSet.Join( new[] { SqlJoinDefinition.Inner( inner, _ => SqlNode.True() ) }.ToList() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( recordSet );
            sut.RecordSets.Should().HaveCount( 2 );
            sut.RecordSets.Should().BeEquivalentTo( recordSet, inner );
            sut.Joins.ToArray().Should().HaveCount( 1 );
            sut.Joins.ToArray().ElementAtOrDefault( 0 ).Should().BeEquivalentTo( SqlNode.InnerJoinOn( inner, SqlNode.True() ) );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] AS [qux] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithRecordSetAndEmptyDefinitions()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.Join( Array.Empty<SqlJoinDefinition>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( recordSet );
            sut.RecordSets.Should().BeSequentiallyEqualTo( recordSet );
            sut.Joins.ToArray().Should().BeEmpty();
            text.Should().Be( "FROM [foo]" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithDataSourceAndNodes()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var joinOn = inner.InnerOn( SqlNode.True() );
        var sut = dataSource.Join( new[] { joinOn }.ToList() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.RecordSets.Should().HaveCount( 2 );
            sut.RecordSets.Should().BeEquivalentTo( dataSource.From, inner );
            sut.Joins.ToArray().Should().BeSequentiallyEqualTo( joinOn );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] AS [qux] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithMultiDataSourceAndNodes()
    {
        var from = SqlNode.RawRecordSet( "foo" );
        var firstJointed = SqlNode.RawRecordSet( "bar" );
        var inner = SqlNode.RawRecordSet( "qux" );
        var firstJoinOn = firstJointed.InnerOn( SqlNode.True() );
        var dataSource = from.Join( firstJoinOn );
        var joinOn = inner.InnerOn( SqlNode.False() );
        var sut = dataSource.Join( joinOn );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( from );
            sut.RecordSets.Should().HaveCount( 3 );
            sut.RecordSets.Should().BeEquivalentTo( from, firstJointed, inner );
            sut.Joins.ToArray().Should().BeSequentiallyEqualTo( firstJoinOn, joinOn );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
INNER JOIN [qux] ON
    (FALSE)" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithDataSourceAndDefinitions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var sut = dataSource.Join( new[] { SqlJoinDefinition.Inner( inner, _ => SqlNode.True() ) }.ToList() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.RecordSets.Should().HaveCount( 2 );
            sut.RecordSets.Should().BeEquivalentTo( dataSource.From, inner );
            sut.Joins.ToArray().Should().HaveCount( 1 );
            sut.Joins.ToArray().ElementAtOrDefault( 0 ).Should().BeEquivalentTo( SqlNode.InnerJoinOn( inner, SqlNode.True() ) );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] AS [qux] ON
    (TRUE)" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithMultiDataSourceAndDefinitions()
    {
        var from = SqlNode.RawRecordSet( "foo" );
        var firstJointed = SqlNode.RawRecordSet( "bar" );
        var inner = SqlNode.RawRecordSet( "qux" );
        var firstJoinOn = firstJointed.InnerOn( SqlNode.True() );
        var dataSource = from.Join( firstJoinOn );
        var sut = dataSource.Join( SqlJoinDefinition.Inner( inner, _ => SqlNode.False() ) );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( from );
            sut.RecordSets.Should().HaveCount( 3 );
            sut.RecordSets.Should().BeEquivalentTo( from, firstJointed, inner );
            sut.Joins.ToArray().Should().HaveCount( 2 );
            sut.Joins.ToArray().ElementAtOrDefault( 0 ).Should().BeSameAs( firstJoinOn );
            sut.Joins.ToArray().ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.InnerJoinOn( inner, SqlNode.False() ) );
            text.Should()
                .Be(
                    @"FROM [foo]
INNER JOIN [bar] ON
    (TRUE)
INNER JOIN [qux] ON
    (FALSE)" );
        }
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithDataSourceAndEmptyDefinitions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Join( Array.Empty<SqlJoinDefinition>() );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.DataSource );
            sut.From.Should().BeSameAs( dataSource.From );
            sut.RecordSets.Should().BeSequentiallyEqualTo( dataSource.From );
            sut.Joins.ToArray().Should().BeEmpty();
            text.Should().Be( "FROM [foo]" );
        }
    }

    [Fact]
    public void OrdinalCommonTableExpression_ShouldCreateOrdinalCommonTableExpressionNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToCte( "A" );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            sut.NodeType.Should().Be( SqlNodeType.CommonTableExpression );
            sut.Query.Should().BeSameAs( query );
            sut.Name.Should().Be( "A" );
            sut.IsRecursive.Should().BeFalse();
            sut.RecordSet.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.RecordSet.CommonTableExpression.Should().BeSameAs( sut );
            sut.RecordSet.Name.Should().Be( sut.Name );
            sut.RecordSet.Alias.Should().BeNull();
            sut.RecordSet.IsAliased.Should().BeFalse();
            text.Should()
                .Be(
                    @"ORDINAL [A] (
    SELECT * FROM foo
)" );
        }
    }

    [Fact]
    public void RecursiveCommonTableExpression_ShouldCreateRecursiveCommonTableExpressionNode()
    {
        var initialQuery = SqlNode.RawQuery( "SELECT * FROM foo" ).ToCte( "A" );
        var components = new[] { initialQuery.RecordSet.ToDataSource().Select( initialQuery.RecordSet.GetAll() ).ToUnion() };
        var selector = Substitute.For<Func<SqlCommonTableExpressionRecordSetNode, IEnumerable<SqlCompoundQueryComponentNode>>>();
        selector.WithAnyArgs( _ => components );
        var sut = initialQuery.ToRecursive( selector );
        var text = sut.ToString();

        using ( new AssertionScope() )
        {
            selector.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( initialQuery.RecordSet );
            sut.NodeType.Should().Be( SqlNodeType.CommonTableExpression );
            sut.Query.FirstQuery.Should().BeSameAs( initialQuery.Query );
            sut.Query.FollowingQueries.ToArray().Should().BeSequentiallyEqualTo( components );
            sut.Query.Decorators.Should().BeEmpty();
            sut.Name.Should().Be( "A" );
            sut.IsRecursive.Should().BeTrue();
            sut.RecordSet.Should().NotBeSameAs( initialQuery.RecordSet );
            sut.RecordSet.NodeType.Should().Be( SqlNodeType.RecordSet );
            sut.RecordSet.CommonTableExpression.Should().BeSameAs( sut );
            sut.RecordSet.Name.Should().Be( sut.Name );
            sut.RecordSet.Alias.Should().BeNull();
            sut.RecordSet.IsAliased.Should().BeFalse();
            text.Should()
                .Be(
                    @"RECURSIVE [A] (
    (
        SELECT * FROM foo
    )
    UNION
    (
        FROM [A]
        SELECT
            [A].*
    )
)" );
        }
    }
}
