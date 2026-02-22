using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class Table : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownColumns()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0", "Col1" } );
            var sut = SqlNode.Table( table );

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestSetEqual( [ sut.GetField( "Col0" ), sut.GetField( "Col1" ) ] ) )
                .Go();
        }

        [Fact]
        public void As_ShouldCreateTableNode_WithNewAlias()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.As( "bar" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Table.TestRefEquals( sut.Table ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "common", "foo" ) ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldCreateTableNode_WithoutAlias()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0", "Col1" } );
            var sut = SqlNode.Table( table, "qux" );
            var result = sut.AsSelf();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Table.TestRefEquals( sut.Table ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "common", "foo" ) ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "common.foo" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestFalse() )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnColumnNode_WhenColumnExists()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.GetUnsafeField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.Column ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType()
                        .AssignableTo<SqlColumnNode>( column => Assertion.All(
                            column.Value.TestRefEquals( table.Columns.Get( "Col0" ) ),
                            column.Type.TestEquals( TypeNullability.Create<int>() ) ) ),
                    text.TestEquals( "[common].[foo].[Col0] : System.Int32" ) )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnDoesNotExist()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
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
        public void GetField_ShouldReturnColumnNode()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.Column ),
                    result.Value.TestRefEquals( table.Columns.Get( "Col0" ) ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[common].[foo].[Col0] : System.Int32" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnColumnNode_WhenColumnIsNullable()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0", "Col1" }, pkColumns: new[] { "Col1" }, areColumnsNullable: true );
            var sut = SqlNode.Table( table );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.Column ),
                    result.Value.TestRefEquals( table.Columns.Get( "Col0" ) ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>( isNullable: true ) ),
                    text.TestEquals( "[common].[foo].[Col0] : Nullable<System.Int32>" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnColumnNode_WithNullableType_WhenTableIsOptional()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table ).MarkAsOptional();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.Column ),
                    result.Value.TestRefEquals( table.Columns.Get( "Col0" ) ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>( isNullable: true ) ),
                    text.TestEquals( "[common].[foo].[Col0] : Nullable<System.Int32>" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnColumnNode_WithAlias()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table, "bar" );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.Column ),
                    result.Value.TestRefEquals( table.Columns.Get( "Col0" ) ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[bar].[Col0] : System.Int32" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table, "bar" );

            var result = sut["Col0"];

            result.TestRefEquals( sut.GetField( "Col0" ) ).Go();
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
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
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnTableNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Table.TestRefEquals( sut.Table ),
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
        public void MarkAsOptional_ShouldReturnTableNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table, "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Table.TestRefEquals( sut.Table ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "common", "foo" ) ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsAliased.TestTrue(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Fact]
        public void DataField_ReplaceRecordSet_ShouldReturnColumnNodeWithReplacedRecordSet()
        {
            var table = SqlTableMock.Create<int>( "foo", new[] { "Col0" } );
            var recordSet = SqlNode.RawRecordSet( "bar" );
            var sut = SqlNode.Table( table )["Col0"];
            var result = sut.ReplaceRecordSet( recordSet );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Name.TestEquals( sut.Name ),
                    result.Type.TestEquals( sut.Type ),
                    result.Value.TestRefEquals( sut.Value ),
                    result.RecordSet.TestRefEquals( recordSet ) )
                .Go();
        }
    }
}
