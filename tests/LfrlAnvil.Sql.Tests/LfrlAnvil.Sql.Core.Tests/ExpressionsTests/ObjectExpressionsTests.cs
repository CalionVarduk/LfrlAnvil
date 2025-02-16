using System.Collections.Generic;
using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.NSubstitute;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests : TestsBase
{
    [Fact]
    public void Literal_WithRefType_ShouldCreateLiteralNode_WhenValueIsNotNull()
    {
        var sut = SqlNode.Literal( "foo" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Literal ),
                text.TestEquals( "\"foo\" : System.String" ),
                sut.TestType()
                    .AssignableTo<SqlLiteralNode<string>>(
                        literalNode => Assertion.All(
                            literalNode.Value.TestEquals( "foo" ),
                            literalNode.Type.TestEquals( TypeNullability.Create<string>() ) ) ) )
            .Go();
    }

    [Fact]
    public void Literal_WithRefType_ShouldCreateNullNode_WhenValueIsNull()
    {
        var sut = SqlNode.Literal<string>( null );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Null ),
                text.TestEquals( "NULL" ) )
            .Go();
    }

    [Fact]
    public void Literal_WithValueType_ShouldCreateLiteralNode()
    {
        var sut = SqlNode.Literal( 42 );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Literal ),
                text.TestEquals( "\"42\" : System.Int32" ),
                sut.TestType()
                    .AssignableTo<SqlLiteralNode<int>>(
                        literalNode => Assertion.All(
                            literalNode.Value.TestEquals( 42 ),
                            literalNode.Type.TestEquals( TypeNullability.Create<int>() ) ) ) )
            .Go();
    }

    [Fact]
    public void Literal_WithNullableValueType_ShouldCreateLiteralNode_WhenValueIsNotNull()
    {
        var sut = SqlNode.Literal( ( int? )42 );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Literal ),
                text.TestEquals( "\"42\" : System.Int32" ),
                sut.TestType()
                    .AssignableTo<SqlLiteralNode<int>>(
                        literalNode => Assertion.All(
                            literalNode.Value.TestEquals( 42 ),
                            literalNode.Type.TestEquals( TypeNullability.Create<int>() ) ) ) )
            .Go();
    }

    [Fact]
    public void Literal_WithNullableValueType_ShouldCreateNullNode_WhenValueIsNull()
    {
        var sut = SqlNode.Literal<int>( null );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Null ),
                text.TestEquals( "NULL" ) )
            .Go();
    }

    [Fact]
    public void Null_ShouldCreateNullNode()
    {
        var sut = SqlNode.Null();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Null ),
                text.TestEquals( "NULL" ) )
            .Go();
    }

    [Theory]
    [InlineData( false )]
    [InlineData( true )]
    public void Parameter_ShouldCreateParameterNode_WithNonNullableType(bool isNullable)
    {
        var expectedType = TypeNullability.Create<string>( isNullable );
        var sut = SqlNode.Parameter<string>( "foo", isNullable );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Parameter ),
                sut.Name.TestEquals( "foo" ),
                sut.Type.TestEquals( expectedType ),
                sut.Index.TestNull(),
                text.TestEquals( $"@foo : {expectedType}" ) )
            .Go();
    }

    [Fact]
    public void Parameter_ShouldCreateParameterNode_WithNullableValueType()
    {
        var sut = SqlNode.Parameter<int?>( "foo" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Parameter ),
                sut.Name.TestEquals( "foo" ),
                sut.Type.TestEquals( TypeNullability.Create<int>( isNullable: true ) ),
                sut.Index.TestNull(),
                text.TestEquals( "@foo : Nullable<System.Int32>" ) )
            .Go();
    }

    [Fact]
    public void Parameter_ShouldCreateParameterNode_WithNoType()
    {
        var sut = SqlNode.Parameter( "foo" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Parameter ),
                sut.Name.TestEquals( "foo" ),
                sut.Type.TestNull(),
                sut.Index.TestNull(),
                text.TestEquals( "@foo : ?" ) )
            .Go();
    }

    [Fact]
    public void Parameter_ShouldCreateParameterNode_WithIndex()
    {
        var sut = SqlNode.Parameter( "foo", index: 1 );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Parameter ),
                sut.Name.TestEquals( "foo" ),
                sut.Type.TestNull(),
                sut.Index.TestEquals( 1 ),
                text.TestEquals( "@foo (#1) : ?" ) )
            .Go();
    }

    [Fact]
    public void Parameter_ShouldCreateParameterNode_WithTypeAndIndex()
    {
        var sut = SqlNode.Parameter<int>( "foo", index: 2 );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Parameter ),
                sut.Name.TestEquals( "foo" ),
                sut.Type.TestEquals( TypeNullability.Create<int>() ),
                sut.Index.TestEquals( 2 ),
                text.TestEquals( "@foo (#2) : System.Int32" ) )
            .Go();
    }

    [Fact]
    public void Parameter_ShouldThrowArgumentOutOfRangeException_WhenIndexIsLessThanZero()
    {
        var action = Lambda.Of( () => SqlNode.Parameter( "foo", index: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void ParameterRange_ShouldReturnArrayOfParameters_WhenCountIsGreaterThanZero()
    {
        var result = SqlNode.ParameterRange<int>( "foo", count: 3 );

        result.TestCount( count => count.TestEquals( 3 ) )
            .Then(
                r => Assertion.All(
                    r[0]
                        .TestType()
                        .AssignableTo<SqlParameterNode>(
                            n => Assertion.All(
                                n.Name.TestEquals( "foo1" ),
                                n.Type.TestEquals( TypeNullability.Create<int>() ),
                                n.Index.TestNull() ) ),
                    r[1]
                        .TestType()
                        .AssignableTo<SqlParameterNode>(
                            n => Assertion.All(
                                n.Name.TestEquals( "foo2" ),
                                n.Type.TestEquals( TypeNullability.Create<int>() ),
                                n.Index.TestNull() ) ),
                    r[2]
                        .TestType()
                        .AssignableTo<SqlParameterNode>(
                            n => Assertion.All(
                                n.Name.TestEquals( "foo3" ),
                                n.Type.TestEquals( TypeNullability.Create<int>() ),
                                n.Index.TestNull() ) ) ) )
            .Go();
    }

    [Fact]
    public void ParameterRange_ShouldReturnArrayOfParameters_WhenCountIsGreaterThanZero_WithFirstIndex()
    {
        var result = SqlNode.ParameterRange<int>( "foo", count: 3, firstIndex: 5 );

        result.TestCount( count => count.TestEquals( 3 ) )
            .Then(
                r => Assertion.All(
                    r[0]
                        .TestType()
                        .AssignableTo<SqlParameterNode>(
                            n => Assertion.All(
                                n.Name.TestEquals( "foo1" ),
                                n.Type.TestEquals( TypeNullability.Create<int>() ),
                                n.Index.TestEquals( 5 ) ) ),
                    r[1]
                        .TestType()
                        .AssignableTo<SqlParameterNode>(
                            n => Assertion.All(
                                n.Name.TestEquals( "foo2" ),
                                n.Type.TestEquals( TypeNullability.Create<int>() ),
                                n.Index.TestEquals( 6 ) ) ),
                    r[2]
                        .TestType()
                        .AssignableTo<SqlParameterNode>(
                            n => Assertion.All(
                                n.Name.TestEquals( "foo3" ),
                                n.Type.TestEquals( TypeNullability.Create<int>() ),
                                n.Index.TestEquals( 7 ) ) ) ) )
            .Go();
    }

    [Fact]
    public void ParameterRange_ShouldReturnEmptyArray_WhenCountEqualsZero()
    {
        var result = SqlNode.ParameterRange<int>( "foo", count: 0 );
        result.TestEmpty().Go();
    }

    [Fact]
    public void ParameterRange_ShouldThrowArgumentOutOfRangeException_WhenCountIsLessThanZero()
    {
        var action = Lambda.Of( () => SqlNode.ParameterRange<int>( "foo", count: -1 ) );
        action.Test( exc => exc.TestType().Exact<ArgumentOutOfRangeException>() ).Go();
    }

    [Fact]
    public void Table_ShouldCreateTableNode()
    {
        var table = SqlTableMock.Create<int>( "foo", new[] { "a" } );
        var sut = table.ToRecordSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Table ),
                sut.Info.TestEquals( table.Info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "common.foo" ),
                sut.Table.TestRefEquals( table ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo]" ) )
            .Go();
    }

    [Fact]
    public void Table_ShouldCreateTableNode_WithAlias()
    {
        var table = SqlTableMock.Create<int>( "foo", new[] { "a" } );
        var sut = table.ToRecordSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.Table ),
                sut.Info.TestEquals( table.Info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.Table.TestRefEquals( table ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void TableBuilder_ShouldCreateTableBuilderNode()
    {
        var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a" } );
        var sut = table.ToRecordSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.TableBuilder ),
                sut.Info.TestEquals( table.Info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "common.foo" ),
                sut.Table.TestRefEquals( table ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo]" ) )
            .Go();
    }

    [Fact]
    public void TableBuilder_ShouldCreateTableBuilderNode_WithAlias()
    {
        var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "a" } );
        var sut = table.ToRecordSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.TableBuilder ),
                sut.Info.TestEquals( table.Info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.Table.TestRefEquals( table ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void View_ShouldCreateViewNode()
    {
        var view = SqlViewMock.Create( "foo" );
        var sut = view.ToRecordSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.View ),
                sut.Info.TestEquals( view.Info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "common.foo" ),
                sut.View.TestRefEquals( view ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo]" ) )
            .Go();
    }

    [Fact]
    public void View_ShouldCreateViewNode_WithAlias()
    {
        var view = SqlViewMock.Create( "foo" );
        var sut = view.ToRecordSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.View ),
                sut.Info.TestEquals( view.Info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.View.TestRefEquals( view ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void ViewBuilder_ShouldCreateViewBuilderNode()
    {
        var view = SqlViewBuilderMock.Create( "foo" );
        var sut = view.ToRecordSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ViewBuilder ),
                sut.Info.TestEquals( view.Info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "common.foo" ),
                sut.View.TestRefEquals( view ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo]" ) )
            .Go();
    }

    [Fact]
    public void ViewBuilder_ShouldCreateViewBuilderNode_WithAlias()
    {
        var view = SqlViewBuilderMock.Create( "foo" );
        var sut = view.ToRecordSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.ViewBuilder ),
                sut.Info.TestEquals( view.Info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.View.TestRefEquals( view ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[common].[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void RawRecordSet_ShouldCreateRawRecordSetNode()
    {
        var sut = SqlNode.RawRecordSet( "foo" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                sut.IsInfoRaw.TestTrue(),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "foo" ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo]" ) )
            .Go();
    }

    [Fact]
    public void RawRecordSet_ShouldCreateRawRecordSetNode_WithAlias()
    {
        var sut = SqlNode.RawRecordSet( "foo", "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                sut.IsInfoRaw.TestTrue(),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void RawRecordSet_ShouldCreateRawRecordSetNode_WithInfo()
    {
        var sut = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "foo", "bar" ) );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "bar" ) ),
                sut.IsInfoRaw.TestFalse(),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "foo.bar" ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo].[bar]" ) )
            .Go();
    }

    [Fact]
    public void RawRecordSet_ShouldCreateRawRecordSetNode_WithInfoAndAlias()
    {
        var sut = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "foo", "bar" ), "qux" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo", "bar" ) ),
                sut.IsInfoRaw.TestFalse(),
                sut.Alias.TestEquals( "qux" ),
                sut.Identifier.TestEquals( "qux" ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo].[bar] AS [qux]" ) )
            .Go();
    }

    [Fact]
    public void NewTable_ShouldCreateNewTableNode()
    {
        var info = SqlRecordSetInfo.Create( "foo" );
        var table = SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() );
        var sut = table.AsSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewTable ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "foo" ),
                sut.CreationNode.TestRefEquals( table ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo]" ) )
            .Go();
    }

    [Fact]
    public void NewTable_ShouldCreateNewTableNode_WithSchemaName()
    {
        var info = SqlRecordSetInfo.Create( "s", "foo" );
        var table = SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() );
        var sut = table.AsSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewTable ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "s.foo" ),
                sut.CreationNode.TestRefEquals( table ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[s].[foo]" ) )
            .Go();
    }

    [Fact]
    public void NewTable_ShouldCreateNewTableNode_WithAlias()
    {
        var info = SqlRecordSetInfo.Create( "foo" );
        var table = SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() );
        var sut = table.AsSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewTable ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.CreationNode.TestRefEquals( table ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void NewTable_ShouldCreateNewTableNode_WithAliasAndSchemaName()
    {
        var info = SqlRecordSetInfo.Create( "s", "foo" );
        var table = SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() );
        var sut = table.AsSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewTable ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.CreationNode.TestRefEquals( table ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[s].[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void NewView_ShouldCreateNewViewNode()
    {
        var info = SqlRecordSetInfo.Create( "foo" );
        var view = SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var sut = view.AsSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewView ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "foo" ),
                sut.CreationNode.TestRefEquals( view ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo]" ) )
            .Go();
    }

    [Fact]
    public void NewView_ShouldCreateNewViewNode_WithSchemaName()
    {
        var info = SqlRecordSetInfo.Create( "s", "foo" );
        var view = SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var sut = view.AsSet();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewView ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestNull(),
                sut.Identifier.TestEquals( "s.foo" ),
                sut.CreationNode.TestRefEquals( view ),
                sut.IsAliased.TestFalse(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[s].[foo]" ) )
            .Go();
    }

    [Fact]
    public void NewView_ShouldCreateNewViewNode_WithAlias()
    {
        var info = SqlRecordSetInfo.Create( "foo" );
        var view = SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var sut = view.AsSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewView ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.CreationNode.TestRefEquals( view ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void NewView_ShouldCreateNewViewNode_WithAliasAndSchemaName()
    {
        var info = SqlRecordSetInfo.Create( "s", "foo" );
        var view = SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM qux" ) );
        var sut = view.AsSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NewView ),
                sut.Info.TestEquals( info ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.CreationNode.TestRefEquals( view ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                text.TestEquals( "[s].[foo] AS [bar]" ) )
            .Go();
    }

    [Fact]
    public void RawDataField_ShouldCreateRawDataFieldNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = SqlNode.RawDataField( recordSet, "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawDataField ),
                sut.RecordSet.TestRefEquals( recordSet ),
                sut.Name.TestEquals( "bar" ),
                sut.Type.TestNull(),
                text.TestEquals( "[foo].[bar] : ?" ) )
            .Go();
    }

    [Fact]
    public void RawDataField_ShouldCreateRawDataFieldNode_WithType()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = SqlNode.RawDataField( recordSet, "bar", TypeNullability.Create<int>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.RawDataField ),
                sut.RecordSet.TestRefEquals( recordSet ),
                sut.Name.TestEquals( "bar" ),
                sut.Type.TestEquals( TypeNullability.Create<int>() ),
                text.TestEquals( "[foo].[bar] : System.Int32" ) )
            .Go();
    }

    [Fact]
    public void NamedFunctionRecordSet_ShouldCreateNamedFunctionRecordSetNode()
    {
        var function = SqlNode.Functions.Named(
            SqlSchemaObjectName.Create( "foo", "bar" ),
            SqlNode.Literal( 1 ),
            SqlNode.Parameter( "a" ) );

        var sut = function.AsSet( "qux" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.NamedFunctionRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "qux" ) ),
                sut.Alias.TestEquals( "qux" ),
                sut.Identifier.TestEquals( "qux" ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                sut.Function.TestRefEquals( function ),
                text.TestEquals( "[foo].[bar]((\"1\" : System.Int32), (@a : ?)) AS [qux]" ) )
            .Go();
    }

    [Fact]
    public void QueryRecordSet_FromDataSourceQuery_ShouldCreateQueryRecordSetNode()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var query = dataSource.Select( dataSource.From["bar"].As( "x" ), dataSource.From["qux"].AsSelf() );
        var sut = query.AsSet( "lorem" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.QueryRecordSet ),
                sut.Query.TestRefEquals( query ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "lorem" ) ),
                sut.Alias.TestEquals( "lorem" ),
                sut.Identifier.TestEquals( "lorem" ),
                sut.IsOptional.TestFalse(),
                sut.IsAliased.TestTrue(),
                text.TestEquals(
                    """
                    (
                      FROM [foo]
                      SELECT
                        ([foo].[bar] : ?) AS [x],
                        ([foo].[qux] : ?)
                    ) AS [lorem]
                    """ ) )
            .Go();
    }

    [Fact]
    public void QueryRecordSet_FromRawQuery_ShouldCreateQueryRecordSetNode()
    {
        var query = SqlNode.RawQuery(
            """
            SELECT *
            FROM foo
            WHERE value > 10
            """ );

        var sut = query.AsSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.QueryRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "bar" ) ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                sut.Query.TestRefEquals( query ),
                text.TestEquals(
                    """
                    (
                      SELECT *
                      FROM foo
                      WHERE value > 10
                    ) AS [bar]
                    """ ) )
            .Go();
    }

    [Fact]
    public void QueryRecordSet_FromCompoundQuery_ShouldCreateQueryRecordSetNode()
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

        var query = query1.CompoundWith( query2.ToUnion() );
        var sut = query.AsSet( "bar" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.QueryRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "bar" ) ),
                sut.Alias.TestEquals( "bar" ),
                sut.Identifier.TestEquals( "bar" ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                sut.Query.TestRefEquals( query ),
                text.TestEquals(
                    """
                    (
                      
                      SELECT a, b
                      FROM foo
                      WHERE value > 10
                    
                      UNION
                      
                      SELECT a, c AS b
                      FROM qux
                      WHERE value < 10

                    ) AS [bar]
                    """ ) )
            .Go();
    }

    [Fact]
    public void QueryRecordSet_FromCompoundQuery_ShouldCreateQueryRecordSetNode_WithKnownFields()
    {
        var dataSource1 = SqlNode.RawRecordSet( "T1" ).ToDataSource();
        var a1 = dataSource1["T1"]["a"];
        var a1Select = a1.AsSelf();
        var query1 = dataSource1.Select( a1Select );

        var dataSource2 = SqlNode.RawRecordSet( "T2" ).ToDataSource();
        var a2 = dataSource2["T2"]["a"];
        var a2Select = a2.AsSelf();
        var query2 = dataSource2.Select( a2Select );

        var query = query1.CompoundWith( query2.ToUnion() );
        var sut = query.AsSet( "foo" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.QueryRecordSet ),
                sut.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                sut.Alias.TestEquals( "foo" ),
                sut.Identifier.TestEquals( "foo" ),
                sut.IsAliased.TestTrue(),
                sut.IsOptional.TestFalse(),
                sut.Query.TestRefEquals( query ),
                text.TestEquals(
                    """
                    (
                      
                      FROM [T1]
                      SELECT
                        ([T1].[a] : ?)
                    
                      UNION
                      
                      FROM [T2]
                      SELECT
                        ([T2].[a] : ?)

                    ) AS [foo]
                    """ ),
                sut.GetField( "a" ).Selection.TestRefEquals( query.Selection[0] ),
                sut.GetField( "a" ).Expression.TestNull() )
            .Go();
    }

    [Fact]
    public void SingleDataSource_ShouldCreateSingleDataSourceNode()
    {
        var from = SqlNode.RawRecordSet( "foo" );
        var sut = from.ToDataSource();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( from ),
                sut.Joins.TestEmpty(),
                sut.RecordSets.TestSequence( [ from ] ),
                text.TestEquals( "FROM [foo]" ) )
            .Go();
    }

    [Fact]
    public void DummyDataSource_ShouldCreateDummyDataSourceNode()
    {
        var sut = SqlNode.DummyDataSource();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.Joins.TestEmpty(),
                sut.RecordSets.TestEmpty(),
                text.TestEquals( "FROM <DUMMY>" ) )
            .Go();
    }

    [Fact]
    public void InnerJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.InnerOn( condition );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.JoinOn ),
                sut.JoinType.TestEquals( SqlJoinType.Inner ),
                sut.InnerRecordSet.TestRefEquals( recordSet ),
                sut.OnExpression.TestRefEquals( condition ),
                text.TestEquals( "INNER JOIN [foo] ON TRUE" ) )
            .Go();
    }

    [Fact]
    public void LeftJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.LeftOn( condition );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.JoinOn ),
                sut.JoinType.TestEquals( SqlJoinType.Left ),
                sut.InnerRecordSet.TestRefEquals( recordSet ),
                sut.OnExpression.TestRefEquals( condition ),
                text.TestEquals( "LEFT JOIN [foo] ON TRUE" ) )
            .Go();
    }

    [Fact]
    public void RightJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.RightOn( condition );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.JoinOn ),
                sut.JoinType.TestEquals( SqlJoinType.Right ),
                sut.InnerRecordSet.TestRefEquals( recordSet ),
                sut.OnExpression.TestRefEquals( condition ),
                text.TestEquals( "RIGHT JOIN [foo] ON TRUE" ) )
            .Go();
    }

    [Fact]
    public void FullJoinOn_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var condition = SqlNode.True();
        var sut = recordSet.FullOn( condition );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.JoinOn ),
                sut.JoinType.TestEquals( SqlJoinType.Full ),
                sut.InnerRecordSet.TestRefEquals( recordSet ),
                sut.OnExpression.TestRefEquals( condition ),
                text.TestEquals( "FULL JOIN [foo] ON TRUE" ) )
            .Go();
    }

    [Fact]
    public void CrossJoin_ShouldCreateDataSourceJoinOnNode()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.Cross();
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.JoinOn ),
                sut.JoinType.TestEquals( SqlJoinType.Cross ),
                sut.InnerRecordSet.TestRefEquals( recordSet ),
                sut.OnExpression.TestRefEquals( SqlNode.True() ),
                text.TestEquals( "CROSS JOIN [foo]" ) )
            .Go();
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithRecordSetAndNodes()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var joinOn = inner.InnerOn( SqlNode.True() );
        var sut = recordSet.Join( new[] { joinOn }.ToList() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( recordSet ),
                sut.RecordSets.Count.TestEquals( 2 ),
                sut.RecordSets.TestSetEqual( [ recordSet, inner ] ),
                sut.Joins.TestSequence( [ joinOn ] ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] AS [qux] ON TRUE
                    """ ) )
            .Go();
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithRecordSetAndDefinitions()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var sut = recordSet.Join( new[] { SqlJoinDefinition.Inner( inner, _ => SqlNode.True() ) }.ToList() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( recordSet ),
                sut.RecordSets.Count.TestEquals( 2 ),
                sut.RecordSets.TestSetEqual( [ recordSet, inner ] ),
                sut.Joins.Count.TestEquals( 1 ),
                sut.Joins.TestAll(
                    (n, _) => Assertion.All(
                        n.JoinType.TestEquals( SqlJoinType.Inner ),
                        n.InnerRecordSet.TestRefEquals( inner ),
                        n.OnExpression.TestType().Exact<SqlTrueNode>() ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] AS [qux] ON TRUE
                    """ ) )
            .Go();
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithRecordSetAndEmptyDefinitions()
    {
        var recordSet = SqlNode.RawRecordSet( "foo" );
        var sut = recordSet.Join( Array.Empty<SqlJoinDefinition>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( recordSet ),
                sut.RecordSets.TestSequence( [ recordSet ] ),
                sut.Joins.TestEmpty(),
                text.TestEquals( "FROM [foo]" ) )
            .Go();
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithDataSourceAndNodes()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var joinOn = inner.InnerOn( SqlNode.True() );
        var sut = dataSource.Join( new[] { joinOn }.ToList() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.RecordSets.Count.TestEquals( 2 ),
                sut.RecordSets.TestSetEqual( [ dataSource.From, inner ] ),
                sut.Joins.TestSequence( [ joinOn ] ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] AS [qux] ON TRUE
                    """ ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( from ),
                sut.RecordSets.Count.TestEquals( 3 ),
                sut.RecordSets.TestSetEqual( [ from, firstJointed, inner ] ),
                sut.Joins.TestSequence( [ firstJoinOn, joinOn ] ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    INNER JOIN [qux] ON FALSE
                    """ ) )
            .Go();
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithDataSourceAndDefinitions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var inner = SqlNode.RawRecordSet( "bar" ).As( "qux" );
        var sut = dataSource.Join( new[] { SqlJoinDefinition.Inner( inner, _ => SqlNode.True() ) }.ToList() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.RecordSets.Count.TestEquals( 2 ),
                sut.RecordSets.TestSetEqual( [ dataSource.From, inner ] ),
                sut.Joins.Count.TestEquals( 1 ),
                sut.Joins.TestAll(
                    (n, _) => Assertion.All(
                        n.JoinType.TestEquals( SqlJoinType.Inner ),
                        n.InnerRecordSet.TestRefEquals( inner ),
                        n.OnExpression.TestType().Exact<SqlTrueNode>() ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] AS [qux] ON TRUE
                    """ ) )
            .Go();
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

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( from ),
                sut.RecordSets.Count.TestEquals( 3 ),
                sut.RecordSets.TestSetEqual( [ from, firstJointed, inner ] ),
                sut.Joins.TestCount( count => count.TestEquals( 2 ) )
                    .Then(
                        joins => Assertion.All(
                            joins[0].TestRefEquals( firstJoinOn ),
                            joins[1].JoinType.TestEquals( SqlJoinType.Inner ),
                            joins[1].InnerRecordSet.TestRefEquals( inner ),
                            joins[1].OnExpression.TestType().Exact<SqlFalseNode>() ) ),
                text.TestEquals(
                    """
                    FROM [foo]
                    INNER JOIN [bar] ON TRUE
                    INNER JOIN [qux] ON FALSE
                    """ ) )
            .Go();
    }

    [Fact]
    public void Join_ShouldCreateMultiSetDataSourceNode_WithDataSourceAndEmptyDefinitions()
    {
        var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
        var sut = dataSource.Join( Array.Empty<SqlJoinDefinition>() );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.From.TestRefEquals( dataSource.From ),
                sut.RecordSets.TestSequence( [ dataSource.From ] ),
                sut.Joins.TestEmpty(),
                text.TestEquals( "FROM [foo]" ) )
            .Go();
    }

    [Fact]
    public void OrdinalCommonTableExpression_ShouldCreateOrdinalCommonTableExpressionNode()
    {
        var query = SqlNode.RawQuery( "SELECT * FROM foo" );
        var sut = query.ToCte( "A" );
        var text = sut.ToString();

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.CommonTableExpression ),
                sut.Query.TestRefEquals( query ),
                sut.Name.TestEquals( "A" ),
                sut.IsRecursive.TestFalse(),
                sut.RecordSet.NodeType.TestEquals( SqlNodeType.CommonTableExpressionRecordSet ),
                sut.RecordSet.CommonTableExpression.TestRefEquals( sut ),
                sut.RecordSet.Info.TestEquals( SqlRecordSetInfo.Create( "A" ) ),
                sut.RecordSet.Alias.TestNull(),
                sut.RecordSet.Identifier.TestEquals( "A" ),
                sut.RecordSet.IsAliased.TestFalse(),
                sut.RecordSet.IsOptional.TestFalse(),
                text.TestEquals(
                    """
                    ORDINAL [A] (
                      SELECT * FROM foo
                    )
                    """ ) )
            .Go();
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

        Assertion.All(
                selector.CallAt( 0 ).Arguments.TestSequence( [ initialQuery.RecordSet ] ),
                sut.NodeType.TestEquals( SqlNodeType.CommonTableExpression ),
                sut.Query.FirstQuery.TestRefEquals( initialQuery.Query ),
                sut.Query.FollowingQueries.TestSequence( components ),
                sut.Query.Traits.TestEmpty(),
                sut.Name.TestEquals( "A" ),
                sut.IsRecursive.TestTrue(),
                sut.RecordSet.TestNotRefEquals( initialQuery.RecordSet ),
                sut.RecordSet.NodeType.TestEquals( SqlNodeType.CommonTableExpressionRecordSet ),
                sut.RecordSet.CommonTableExpression.TestRefEquals( sut ),
                sut.RecordSet.Info.TestEquals( SqlRecordSetInfo.Create( "A" ) ),
                sut.RecordSet.Alias.TestNull(),
                sut.RecordSet.Identifier.TestEquals( "A" ),
                sut.RecordSet.IsAliased.TestFalse(),
                sut.RecordSet.IsOptional.TestFalse(),
                text.TestEquals(
                    """
                    RECURSIVE [A] (
                      
                      SELECT * FROM foo
                    
                      UNION
                      
                      FROM [A]
                      SELECT
                        [A].*

                    )
                    """ ) )
            .Go();
    }

    [Fact]
    public void BaseDataSourceNodeAddTrait_ShouldCallSetTraits()
    {
        var sut = new SqlDataSourceNodeMock().AddTrait( SqlNode.DistinctTrait() );

        Assertion.All(
                sut.NodeType.TestEquals( SqlNodeType.DataSource ),
                sut.Traits.Count.TestEquals( 1 ),
                (sut.Traits.ElementAtOrDefault( 0 )?.NodeType).TestEquals( SqlNodeType.DistinctTrait ) )
            .Go();
    }
}
