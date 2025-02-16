using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class NewView : TestsBase
    {
        [Theory]
        [InlineData( false, "[foo].[bar] AS [qux]" )]
        [InlineData( true, "TEMP.[foo] AS [qux]" )]
        public void ToString_ShouldReturnCorrectRepresentation(bool isTemporary, string expected)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var view = SqlNode.CreateView( info, SqlNode.RawQuery( "SELECT * FROM qux" ) );
            var sut = view.AsSet( "qux" );

            var result = sut.ToString();

            result.TestEquals( expected ).Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownRecordSetsFields_WithSelectAll()
        {
            var t1 = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node;
            var t2 = SqlTableMock.Create<int>( "T2", new[] { "c", "d" } ).Node;
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( SqlRecordSetInfo.Create( "foo" ) ).AsSet();

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 4 ),
                    result.TestSetEqual( [ sut.GetField( "a" ), sut.GetField( "b" ), sut.GetField( "c" ), sut.GetField( "d" ) ] ) )
                .Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownSingleRecordSetFields_WithSelectAllFromRecordSet()
        {
            var t1 = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node;
            var t2 = SqlTableMock.Create<int>( "T2", new[] { "c", "d" } ).Node;
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select( dataSource.From.GetAll() ).ToCreateView( SqlRecordSetInfo.Create( "foo" ) ).AsSet();

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestSetEqual( [ sut.GetField( "a" ), sut.GetField( "b" ) ] ) )
                .Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnExplicitSelections_WithFieldSelectionsOnly()
        {
            var t1 = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node;
            var t2 = SqlTableMock.Create<int>( "T2", new[] { "c", "d" } ).Node;
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select(
                    dataSource["common.T1"]["a"].AsSelf(),
                    dataSource["common.T2"]["d"].AsSelf(),
                    dataSource["common.T1"].GetUnsafeField( "e" ).AsSelf() )
                .ToCreateView( SqlRecordSetInfo.Create( "foo" ) )
                .AsSet();

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 3 ),
                    result.TestSetEqual( [ sut.GetField( "a" ), sut.GetField( "d" ), sut.GetField( "e" ) ] ) )
                .Go();
        }

        [Fact]
        public void As_ShouldCreateNewViewNode_WithNewAlias()
        {
            var view = SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM qux" ) );
            var sut = view.AsSet();
            var result = sut.As( "bar" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldCreateNewViewNode_WithoutAlias()
        {
            var view = SqlNode.CreateView( SqlRecordSetInfo.Create( "foo", "bar" ), SqlNode.RawQuery( "SELECT * FROM lorem" ) );
            var sut = view.AsSet( "qux" );
            var result = sut.AsSelf();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "foo.bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestFalse() )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[a]" )]
        [InlineData( true, "TEMP.[foo].[a]" )]
        public void GetUnsafeField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown(bool isTemporary, string expectedText)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).ToCreateView( info ).AsSet();
            var result = sut.GetUnsafeField( "a" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType()
                        .AssignableTo<SqlQueryDataFieldNode>(
                            dataField => Assertion.All(
                                dataField.Selection.TestRefEquals( selection ),
                                dataField.Expression.TestRefEquals( dataSource["common.t"]["a"] ) ) ),
                    text.TestEquals( expectedText ) )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[b] : ?" )]
        [InlineData( true, "TEMP.[foo].[b] : ?" )]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenNameIsNotKnown(bool isTemporary, string expectedText)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( info ).AsSet();
            var result = sut.GetUnsafeField( "b" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "b" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType().AssignableTo<SqlRawDataFieldNode>( dataField => dataField.Type.TestNull() ),
                    text.TestEquals( expectedText ) )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[a]" )]
        [InlineData( true, "TEMP.[foo].[a]" )]
        public void GetField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown(bool isTemporary, string expectedText)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).ToCreateView( info ).AsSet();
            var result = sut.GetField( "a" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Selection.TestRefEquals( selection ),
                    result.Expression.TestRefEquals( dataSource["common.t"]["a"] ),
                    text.TestEquals( expectedText ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenNameIsNotKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( SqlRecordSetInfo.Create( "foo" ) ).AsSet();

            var action = Lambda.Of( () => sut.GetField( "b" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( SqlRecordSetInfo.Create( "foo" ) ).AsSet();

            var result = sut["a"];

            result.TestRefEquals( sut.GetField( "a" ) ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void GetRawField_ShouldReturnRawDataFieldNode(bool isTemporary)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( info ).AsSet( "qux" );
            var result = sut.GetRawField( "x", TypeNullability.Create<int>() );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "x" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[qux].[x] : System.Int32" ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var view = SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM bar" ) );
            var sut = view.AsSet().MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNewViewNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var view = SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM bar" ) );
            var sut = view.AsSet().MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "foo" ),
                    result.IsAliased.TestFalse(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNewViewNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var view = SqlNode.CreateView( SqlRecordSetInfo.Create( "foo" ), SqlNode.RawQuery( "SELECT * FROM qux" ) );
            var sut = view.AsSet( "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsAliased.TestTrue(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }
    }
}
