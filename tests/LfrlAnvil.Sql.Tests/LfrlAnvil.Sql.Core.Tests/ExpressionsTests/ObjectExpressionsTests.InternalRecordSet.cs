using System.Linq;
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

            using ( new AssertionScope() )
            {
                sut.Info.Should().Be( SqlRecordSetInfo.Create( "<internal>" ) );
                sut.Base.Should().BeSameAs( @base );
                sut.IsAliased.Should().BeFalse();
                sut.IsOptional.Should().Be( @base.IsOptional );
                text.Should().Be( "([<internal>] FROM [foo])" );
            }
        }

        [Fact]
        public void GetKnownFields_ShouldReturnEmptyKnownFields_WhenBaseKnownFieldsAreEmpty()
        {
            var @base = SqlNode.RawRecordSet( "foo" );
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.GetKnownFields();
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnBaseKnownFieldsWithReplacedRecordSet()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.ElementAtOrDefault( 0 ).Should().BeEquivalentTo( @base["a"].ReplaceRecordSet( sut ) );
                result.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( @base["b"].ReplaceRecordSet( sut ) );
            }
        }

        [Fact]
        public void As_ShouldThrowNotSupportedException()
        {
            var sut = new SqlInternalRecordSetNode( SqlNode.RawRecordSet( "foo" ) );
            var action = Lambda.Of( () => sut.As( "bar" ) );
            action.Should().ThrowExactly<NotSupportedException>();
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf()
        {
            var sut = new SqlInternalRecordSetNode( SqlNode.RawRecordSet( "foo" ) );
            var result = sut.AsSelf();
            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnBaseUnsafeFieldWithReplacedRecordSet()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.GetUnsafeField( "bar" );
            result.Should().BeEquivalentTo( sut.GetUnsafeField( "bar" ).ReplaceRecordSet( sut ) );
        }

        [Fact]
        public void GetField_ShouldReturnBaseDataFieldWithReplacedRecordSet()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.GetField( "a" );
            result.Should().BeEquivalentTo( sut.GetField( "a" ).ReplaceRecordSet( sut ) );
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node;
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut["a"];
            result.Should().BeEquivalentTo( sut.GetField( "a" ) );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node.MarkAsOptional( optional );
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnRawRecordSetNode_WhenOptionalityChanges(bool optional)
        {
            var @base = SqlTableMock.Create<int>( "foo", new[] { "a", "b" } ).Node.MarkAsOptional( ! optional );
            var sut = new SqlInternalRecordSetNode( @base );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Base.IsOptional.Should().Be( optional );
                result.IsOptional.Should().Be( optional );
            }
        }
    }
}
