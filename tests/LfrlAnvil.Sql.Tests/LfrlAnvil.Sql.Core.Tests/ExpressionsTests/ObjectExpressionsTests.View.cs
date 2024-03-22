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

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.GetField( "Col0" ), sut.GetField( "Col1" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateViewNode_WithNewAlias()
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view );
            var result = sut.As( "bar" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.View.Should().BeSameAs( sut.View );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "foo" ) );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldCreateViewNode_WithoutAlias()
        {
            var view = SqlViewMock.Create( "bar" );
            var sut = SqlNode.View( view, "qux" );
            var result = sut.AsSelf();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.View.Should().BeSameAs( sut.View );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "bar" ) );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "common.bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeFalse();
            }
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

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ViewDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                var field = result as SqlViewDataFieldNode;
                (field?.Value).Should().BeSameAs( view.DataFields.Get( "Col0" ) );
                text.Should().Be( "[common].[foo].[Col0]" );
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenFieldDoesNotExist()
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view );
            var result = sut.GetUnsafeField( "bar" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( "[common].[foo].[bar] : ?" );
            }
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

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ViewDataField );
                result.Value.Should().BeSameAs( view.DataFields.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                text.Should().Be( "[common].[foo].[Col0]" );
            }
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

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ViewDataField );
                result.Value.Should().BeSameAs( view.DataFields.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                text.Should().Be( "[bar].[Col0]" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenFieldDoesNotExist()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var view = SqlViewMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view, "bar" );

            var result = sut["Col0"];

            result.Should().BeSameAs( sut.GetField( "Col0" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view );
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[common].[foo].[bar] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnViewNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.View.Should().BeSameAs( sut.View );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "foo" ) );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "common.foo" );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnViewNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var view = SqlViewMock.Create( "foo" );
            var sut = SqlNode.View( view, "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.View.Should().BeSameAs( sut.View );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "foo" ) );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
            }
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

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Name.Should().Be( sut.Name );
                result.Value.Should().BeSameAs( sut.Value );
                result.RecordSet.Should().BeSameAs( recordSet );
            }
        }
    }
}
