using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class NamedFunctionRecordSet : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnEmpty()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.GetKnownFields();
            result.Should().BeEmpty();
        }

        [Fact]
        public void As_ShouldCreateNamedFunctionRecordSetNode_WithNewAlias()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.As( "qux" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "qux" ) );
                result.Alias.Should().Be( "qux" );
                result.Identifier.Should().Be( "qux" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
                result.Function.Should().BeSameAs( sut.Function );
            }
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.AsSelf();
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.GetUnsafeField( "qux" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "qux" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().BeNull();
                text.Should().Be( "[bar].[qux] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.GetField( "qux" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "qux" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().BeNull();
                text.Should().Be( "[bar].[qux] : ?" );
            }
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut["qux"];
            result.Should().BeEquivalentTo( sut.GetField( "qux" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.GetRawField( "qux", SqlExpressionType.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "qux" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "[bar].[qux] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNamedFunctionRecordSetNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Info.Should().Be( SqlRecordSetInfo.Create( "bar" ) );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
                result.Function.Should().BeSameAs( sut.Function );
            }
        }
    }
}
