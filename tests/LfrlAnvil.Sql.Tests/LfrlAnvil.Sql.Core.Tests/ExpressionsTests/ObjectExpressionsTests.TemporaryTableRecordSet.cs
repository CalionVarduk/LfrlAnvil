using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class TemporaryTableRecordSet : TestsBase
    {
        [Fact]
        public void ToString_ShouldReturnCorrectRepresentation()
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet( "bar" );

            var result = sut.ToString();

            result.Should().Be( "TEMP.[foo] AS [bar]" );
        }

        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownColumns()
        {
            var table = SqlNode.CreateTempTable(
                "foo",
                SqlNode.ColumnDefinition<int>( "Col0" ),
                SqlNode.ColumnDefinition<string>( "Col1" ) );

            var sut = table.AsSet();

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.GetField( "Col0" ), sut.GetField( "Col1" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateTemporaryTableRecordSetNode_WithNewAlias()
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet();
            var result = sut.As( "bar" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.Name.Should().Be( "bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldCreateTemporaryTableRecordSetNode_WithoutAlias()
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet( "bar" );
            var result = sut.AsSelf();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.Name.Should().Be( "foo" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeFalse();
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnExists()
        {
            var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "Col0" ) );
            var sut = table.AsSet();
            var result = sut.GetUnsafeField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "TEMP.[foo].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnDoesNotExist()
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet();
            var result = sut.GetUnsafeField( "bar" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( "TEMP.[foo].[bar] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode()
        {
            var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "Col0" ) );
            var sut = table.AsSet();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "TEMP.[foo].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode_WhenColumnIsNullable()
        {
            var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "Col0", isNullable: true ) );
            var sut = table.AsSet( "bar" );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
                text.Should().Be( "[bar].[Col0] : Nullable<System.Int32>" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode_WithNullableType_WhenTableIsOptional()
        {
            var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "Col0" ) );
            var sut = table.AsSet().MarkAsOptional();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
                text.Should().Be( "TEMP.[foo].[Col0] : Nullable<System.Int32>" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode_WithAlias()
        {
            var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "Col0" ) );
            var sut = table.AsSet( "bar" );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "[bar].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "Col0" ) );
            var sut = table.AsSet();

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var table = SqlNode.CreateTempTable( "foo", SqlNode.ColumnDefinition<int>( "Col0" ) );
            var sut = table.AsSet( "bar" );

            var result = sut["Col0"];

            result.Should().BeSameAs( sut.GetField( "Col0" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet();
            var result = sut.GetRawField( "bar", SqlExpressionType.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "TEMP.[foo].[bar] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet().MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnTemporaryTableRecordSetNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet().MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.Name.Should().Be( sut.Name );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnTemporaryTableRecordSetNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var table = SqlNode.CreateTempTable( "foo" );
            var sut = table.AsSet( "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.Name.Should().Be( sut.Name );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
            }
        }
    }
}
