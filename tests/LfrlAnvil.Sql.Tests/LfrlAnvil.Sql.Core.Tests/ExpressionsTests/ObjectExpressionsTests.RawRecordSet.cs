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
            result.TestEmpty().Go();
        }

        [Fact]
        public void As_ShouldCreateRawRecordSetNode_WithNewAlias()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.As( "bar" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                    result.IsInfoRaw.TestEquals( sut.IsInfoRaw ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldCreateRawRecordSetNode_WithoutAlias()
        {
            var sut = SqlNode.RawRecordSet( "foo", "bar" );
            var result = sut.AsSelf();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                    result.IsInfoRaw.TestEquals( sut.IsInfoRaw ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "foo" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestFalse() )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.GetUnsafeField( "bar" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestNull(),
                    text.TestEquals( "[foo].[bar] : ?" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.GetField( "bar" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestNull(),
                    text.TestEquals( "[foo].[bar] : ?" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnRawDataFieldNode_WithAlias()
        {
            var sut = SqlNode.RawRecordSet( "foo", "bar" );
            var result = sut.GetField( "qux" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "qux" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestNull(),
                    text.TestEquals( "[bar].[qux] : ?" ) )
                .Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var sut = SqlNode.RawRecordSet( "foo" );

            var result = sut["bar"];

            Assertion.All(
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ) )
                .Go();
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.RawRecordSet( "foo" );
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[foo].[bar] : System.Int32" ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var sut = SqlNode.RawRecordSet( "foo" ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnRawRecordSetNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var sut = SqlNode.RawRecordSet( "foo" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                    result.IsInfoRaw.TestEquals( sut.IsInfoRaw ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "foo" ),
                    result.IsAliased.TestFalse(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnRawRecordSetNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var sut = SqlNode.RawRecordSet( "foo", "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                    result.IsInfoRaw.TestEquals( sut.IsInfoRaw ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsAliased.TestTrue(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Fact]
        public void DataField_ReplaceRecordSet_ShouldReturnRawDataFieldNodeWithReplacedRecordSet()
        {
            var source = SqlNode.RawRecordSet( "foo" );
            var recordSet = SqlNode.RawRecordSet( "bar" );
            var sut = source["Col0"];
            var result = sut.ReplaceRecordSet( recordSet );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Name.TestEquals( sut.Name ),
                    result.Type.TestEquals( sut.Type ),
                    result.RecordSet.TestRefEquals( recordSet ) )
                .Go();
        }
    }
}
