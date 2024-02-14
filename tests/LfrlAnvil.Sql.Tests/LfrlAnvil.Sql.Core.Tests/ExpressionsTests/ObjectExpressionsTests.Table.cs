using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class Table : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownColumns()
        {
            var table = TableMock.Create( "foo", ColumnMock.CreateMany<int>( areNullable: false, "Col0", "Col1" ) );
            var sut = SqlNode.Table( table );

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.GetField( "Col0" ), sut.GetField( "Col1" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateTableNode_WithNewAlias()
        {
            var table = TableMock.Create( "foo" );
            var sut = SqlNode.Table( table );
            var result = sut.As( "bar" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldCreateTableNode_WithoutAlias()
        {
            var table = TableMock.Create( "bar", SchemaMock.Create( "foo" ) );
            var sut = SqlNode.Table( table, "qux" );
            var result = sut.AsSelf();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo", "bar" ) );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "foo.bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeFalse();
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnColumnNode_WhenColumnExists()
        {
            var table = TableMock.Create( "foo", ColumnMock.Create<int>( "Col0" ) );
            var sut = SqlNode.Table( table );
            var result = sut.GetUnsafeField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.Column );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                var column = result as SqlColumnNode;
                (column?.Value).Should().BeSameAs( table.Columns.Get( "Col0" ) );
                (column?.Type).Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[foo].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnDoesNotExist()
        {
            var table = TableMock.Create( "foo" );
            var sut = SqlNode.Table( table );
            var result = sut.GetUnsafeField( "bar" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( "[foo].[bar] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnNode()
        {
            var table = TableMock.Create( "foo", ColumnMock.Create<int>( "Col0" ) );
            var sut = SqlNode.Table( table );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.Column );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[foo].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnNode_WhenColumnIsNullable()
        {
            var table = TableMock.Create( "foo", ColumnMock.Create<int>( "Col0", isNullable: true ) );
            var sut = SqlNode.Table( table );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.Column );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>( isNullable: true ) );
                text.Should().Be( "[foo].[Col0] : Nullable<System.Int32>" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnNode_WithNullableType_WhenTableIsOptional()
        {
            var table = TableMock.Create( "foo", ColumnMock.Create<int>( "Col0" ) );
            var sut = SqlNode.Table( table ).MarkAsOptional();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.Column );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>( isNullable: true ) );
                text.Should().Be( "[foo].[Col0] : Nullable<System.Int32>" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnNode_WithAlias()
        {
            var table = TableMock.Create( "foo", ColumnMock.Create<int>( "Col0" ) );
            var sut = SqlNode.Table( table, "bar" );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.Column );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[bar].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var table = TableMock.Create( "foo", ColumnMock.Create<int>( "Col0" ) );
            var sut = SqlNode.Table( table );

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var table = TableMock.Create( "foo", ColumnMock.Create<int>( "Col0" ) );
            var sut = SqlNode.Table( table, "bar" );

            var result = sut["Col0"];

            result.Should().BeSameAs( sut.GetField( "Col0" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var table = TableMock.Create( "foo" );
            var sut = SqlNode.Table( table );
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[foo].[bar] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var table = TableMock.Create( "foo" );
            var sut = SqlNode.Table( table ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnTableNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var table = TableMock.Create( "foo" );
            var sut = SqlNode.Table( table ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "foo" );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnTableNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var table = TableMock.Create( "foo" );
            var sut = SqlNode.Table( table, "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
            }
        }
    }
}
