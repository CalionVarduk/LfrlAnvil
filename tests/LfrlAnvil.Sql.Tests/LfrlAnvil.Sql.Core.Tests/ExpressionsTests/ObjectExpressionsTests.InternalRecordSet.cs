using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class InternalRecordSet : TestsBase
    {
        [Fact]
        public void Ctor_ShouldCreateCorrectRecordSet()
        {
            var @base = SqlNode.RawRecordSet( "foo" );
            var sut = new SqlInternalRecordSetNode( @base );
            var text = sut.ToString();

            Assertion.All(
                    sut.Info.TestEquals( SqlRecordSetInfo.Create( "<internal>" ) ),
                    sut.Base.TestRefEquals( @base ),
                    sut.IsAliased.TestFalse(),
                    sut.IsOptional.TestEquals( @base.IsOptional ),
                    text.TestEquals( "([<internal>] FROM [foo])" ) )
                .Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnEmptyKnownFields_WhenBaseKnownFieldsAreEmpty()
        {
            var @base = SqlNode.RawRecordSet( "foo" );
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.GetKnownFields();
            result.TestEmpty().Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnBaseKnownFieldsWithReplacedRecordSet()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );

            var result = sut.GetKnownFields();

            result.TestCount( count => count.TestEquals( 2 ) )
                .Then( r => Assertion.All(
                    r[0].Name.TestEquals( "a" ),
                    r[0].RecordSet.TestRefEquals( sut ),
                    r[1].Name.TestEquals( "b" ),
                    r[1].RecordSet.TestRefEquals( sut ) ) )
                .Go();
        }

        [Fact]
        public void As_ShouldThrowNotSupportedException()
        {
            var sut = new SqlInternalRecordSetNode( SqlNode.RawRecordSet( "foo" ) );
            var action = Lambda.Of( () => sut.As( "bar" ) );
            action.Test( exc => exc.TestType().Exact<NotSupportedException>() ).Go();
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf()
        {
            var sut = new SqlInternalRecordSetNode( SqlNode.RawRecordSet( "foo" ) );
            var result = sut.AsSelf();
            result.TestRefEquals( sut ).Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnBaseUnsafeFieldWithReplacedRecordSet()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );

            var result = sut.GetUnsafeField( "bar" );

            Assertion.All(
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnBaseDataFieldWithReplacedRecordSet()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );

            var result = sut.GetField( "a" );

            Assertion.All(
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ) )
                .Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );

            var result = sut["a"];

            Assertion.All(
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node.MarkAsOptional( optional );
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnRawRecordSetNode_WhenOptionalityChanges(bool optional)
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node.MarkAsOptional( ! optional );
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.Base.IsOptional.TestEquals( optional ),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }
    }
}
