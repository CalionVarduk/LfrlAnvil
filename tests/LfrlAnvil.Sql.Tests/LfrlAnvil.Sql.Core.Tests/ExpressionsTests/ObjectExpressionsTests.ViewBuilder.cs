using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class ViewBuilder : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownFields()
        {
            var view = SqlViewBuilderMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf(), x.From["Col1"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestAll(
                        (f, _) => Assertion.All(
                            f.RecordSet.TestRefEquals( sut ),
                            Assertion.Any( f.Name.TestEquals( "Col0" ), f.Name.TestEquals( "Col1" ) ) ) ) )
                .Go();
        }

        [Fact]
        public void As_ShouldCreateViewBuilderNode_WithNewAlias()
        {
            var view = SqlViewBuilderMock.Create( "foo" );
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
        public void AsSelf_ShouldCreateViewBuilderNode_WithoutAlias()
        {
            var view = SqlViewBuilderMock.Create( "bar" );
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
        public void GetUnsafeField_ShouldReturnQueryDataFieldNode_WhenFieldExists()
        {
            var view = SqlViewBuilderMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var result = sut.GetUnsafeField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    text.TestEquals( "[common].[foo].[Col0]" ) )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenFieldDoesNotExist()
        {
            var view = SqlViewBuilderMock.Create( "foo" );
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
        public void GetField_ShouldReturnQueryDataFieldNode()
        {
            var view = SqlViewBuilderMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    text.TestEquals( "[common].[foo].[Col0]" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnQueryDataFieldNode_WithAlias()
        {
            var view = SqlViewBuilderMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view, "bar" );

            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    text.TestEquals( "[bar].[Col0]" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenFieldDoesNotExist()
        {
            var view = SqlViewBuilderMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void GetField_ShouldThrowArgumentException_WhenFieldNameExistsMoreThanOnce()
        {
            var view = SqlViewBuilderMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf(), x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view );

            var action = Lambda.Of( () => sut.GetField( "Col0" ) );

            action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var view = SqlViewBuilderMock.Create(
                "foo",
                SqlNode.RawRecordSet( "x" ).ToDataSource().Select( x => new[] { x.From["Col0"].AsSelf() } ) );

            var sut = SqlNode.View( view, "bar" );

            var result = sut["Col0"];

            Assertion.All(
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ) )
                .Go();
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var view = SqlViewBuilderMock.Create( "foo" );
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
            var view = SqlViewBuilderMock.Create( "foo" );
            var sut = SqlNode.View( view ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnViewBuilderNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var view = SqlViewBuilderMock.Create( "foo" );
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
        public void MarkAsOptional_ShouldReturnViewBuilderNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var view = SqlViewBuilderMock.Create( "foo" );
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
    }
}
