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
            result.TestEmpty().Go();
        }

        [Fact]
        public void As_ShouldCreateNamedFunctionRecordSetNode_WithNewAlias()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.As( "qux" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "qux" ) ),
                    result.Alias.TestEquals( "qux" ),
                    result.Identifier.TestEquals( "qux" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue(),
                    result.Function.TestRefEquals( sut.Function ) )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.AsSelf();
            result.TestRefEquals( sut ).Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.GetUnsafeField( "qux" );
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
        public void GetField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
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
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );

            var result = sut["qux"];

            Assertion.All(
                    result.Name.TestEquals( "qux" ),
                    result.RecordSet.TestRefEquals( sut ) )
                .Go();
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" );
            var result = sut.GetRawField( "qux", TypeNullability.Create<int>() );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "qux" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[bar].[qux] : System.Int32" ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNamedFunctionRecordSetNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var sut = SqlNode.Functions.Named( SqlSchemaObjectName.Create( "foo" ) ).AsSet( "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "bar" ) ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsAliased.TestTrue(),
                    result.IsOptional.TestEquals( optional ),
                    result.Function.TestRefEquals( sut.Function ) )
                .Go();
        }
    }
}
