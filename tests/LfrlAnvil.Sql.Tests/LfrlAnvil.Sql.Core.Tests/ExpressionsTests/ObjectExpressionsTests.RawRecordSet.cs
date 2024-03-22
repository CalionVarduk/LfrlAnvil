using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class RawRecordSet : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnEmpty()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.GetKnownFields();
            result.Should().BeEmpty();
        }

        [Fact]
        public void As_ShouldCreateRawRecordSetNode_WithNewAlias()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.As( "bar" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.IsInfoRaw.Should().Be( sut.IsInfoRaw );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldCreateRawRecordSetNode_WithoutAlias()
        {
            var sut = SqlNode.RawRecordSet( "foo", "bar" );
            var result = sut.AsSelf();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.IsInfoRaw.Should().Be( sut.IsInfoRaw );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "foo" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeFalse();
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.GetUnsafeField( "bar" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().BeNull();
                text.Should().Be( "[foo].[bar] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.GetField( "bar" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().BeNull();
                text.Should().Be( "[foo].[bar] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode_WithAlias()
        {
            var sut = SqlNode.RawRecordSet( "foo", "bar" );
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
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut["bar"];
            result.Should().BeEquivalentTo( sut.GetField( "bar" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
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
            var sut = SqlNode.RawRecordSet( "foo" ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnRawRecordSetNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var sut = SqlNode.RawRecordSet( "foo" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.IsInfoRaw.Should().Be( sut.IsInfoRaw );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "foo" );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnRawRecordSetNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var sut = SqlNode.RawRecordSet( "foo", "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.IsInfoRaw.Should().Be( sut.IsInfoRaw );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Fact]
        public void DataField_ReplaceRecordSet_ShouldReturnRawDataFieldNodeWithReplacedRecordSet()
        {
            var source = SqlNode.RawRecordSet( "foo" );
            var recordSet = SqlNode.RawRecordSet( "bar" );
            var sut = source["Col0"];
            var result = sut.ReplaceRecordSet( recordSet );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Name.Should().Be( sut.Name );
                result.Type.Should().Be( sut.Type );
                result.RecordSet.Should().BeSameAs( recordSet );
            }
        }
    }
}
