using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class View : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownFields()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf(), x.From["Col1"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestSetEqual( [ sut.GetField( "Col0" ), sut.GetField( "Col1" ) ] ) )
                .Go();
        }

        [Fact]
        public void As_ShouldCreateViewNode_WithNewAlias()
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view );
            var result = sut.As( "bar" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.View.TestRefEquals( sut.View ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "common", "foo" ) ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldCreateViewNode_WithoutAlias()
        {
            var view = SqlViewMock.Create( "bar" );
            var sut = SqlNode.View( view, "qux" );
            var result = sut.AsSelf();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.View.TestRefEquals( sut.View ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "common", "bar" ) ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "common.bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestFalse() )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnViewDataFieldNode_WhenFieldExists()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var result = sut.GetUnsafeField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.ViewDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType()
                        .AssignableTo<SqlViewDataFieldNode>( field => field.Value.TestRefEquals( view.DataFields.Get( "Col0" ) ) ),
                    text.TestEquals( "[common].[foo].[Col0]" ) )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenFieldDoesNotExist()
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view );
            var result = sut.GetUnsafeField( "bar" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType().AssignableTo<SqlRawDataFieldNode>( dataField => dataField.Type.TestNull() ),
                    text.TestEquals( "[common].[foo].[bar] : ?" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnViewDataFieldNode()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.ViewDataField ),
                    result.Value.TestRefEquals( view.DataFields.Get( "Col0" ) ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    text.TestEquals( "[common].[foo].[Col0]" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnViewDataFieldNode_WithAlias()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view, "bar" );

            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.ViewDataField ),
                    result.Value.TestRefEquals( view.DataFields.Get( "Col0" ) ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    text.TestEquals( "[bar].[Col0]" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenFieldDoesNotExist()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view, "bar" );

            var result = sut["Col0"];

            result.TestRefEquals( sut.GetField( "Col0" ) ).Go();
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view );
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[common].[foo].[bar] : System.Int32" ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnViewNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.View.TestRefEquals( sut.View ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "common", "foo" ) ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "common.foo" ),
                    result.IsAliased.TestFalse(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnViewNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view, "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.View.TestRefEquals( sut.View ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "common", "foo" ) ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsAliased.TestTrue(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Fact]
        public void DataField_ReplaceRecordSet_ShouldReturnViewDataFieldNodeWithReplacedRecordSet()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var recordSet = SqlNode.RawRecordSet( "bar" );
            var sut = SqlNode.View( view )["Col0"];
            var result = sut.ReplaceRecordSet( recordSet );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Name.TestEquals( sut.Name ),
                    result.Value.TestRefEquals( sut.Value ),
                    result.RecordSet.TestRefEquals( recordSet ) )
                .Go();
        }
    }
}
