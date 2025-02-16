using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class NewTable : TestsBase
    {
        [Theory]
        [InlineData( false, "[foo].[bar] AS [qux]" )]
        [InlineData( true, "TEMP.[foo] AS [qux]" )]
        public void ToString_ShouldReturnCorrectRepresentation(bool isTemporary, string expected)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet( "qux" );

            var result = sut.ToString();

            result.TestEquals( expected ).Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownColumns()
        {
            var table = SqlNode.CreateTable(
                SqlRecordSetInfo.Create( "foo" ),
                new[] { SqlNode.Column<int>( "Col0" ), SqlNode.Column<string>( "Col1" ) } );

            var sut = table.AsSet();

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestSetEqual( [ sut.GetField( "Col0" ), sut.GetField( "Col1" ) ] ) )
                .Go();
        }

        [Fact]
        public void As_ShouldCreateNewTableNode_WithNewAlias()
        {
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet();
            var result = sut.As( "bar" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldCreateNewTableNode_WithoutAlias()
        {
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo", "bar" ), Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet( "qux" );
            var result = sut.AsSelf();

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "foo.bar" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestFalse() )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : System.Int32" )]
        [InlineData( true, "TEMP.[foo].[Col0] : System.Int32" )]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnExists(bool isTemporary, string expectedText)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet();
            var result = sut.GetUnsafeField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType()
                        .AssignableTo<SqlRawDataFieldNode>( dataField => dataField.Type.TestEquals( TypeNullability.Create<int>() ) ),
                    text.TestEquals( expectedText ) )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[x] : ?" )]
        [InlineData( true, "TEMP.[foo].[x] : ?" )]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnDoesNotExist(bool isTemporary, string expected)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet();
            var result = sut.GetUnsafeField( "x" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "x" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType().AssignableTo<SqlRawDataFieldNode>( dataField => dataField.Type.TestNull() ),
                    text.TestEquals( expected ) )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : System.Int32" )]
        [InlineData( true, "TEMP.[foo].[Col0] : System.Int32" )]
        public void GetField_ShouldReturnRawDataFieldNode(bool isTemporary, string expected)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( expected ) )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : Nullable<System.Int32>" )]
        [InlineData( true, "TEMP.[foo].[Col0] : Nullable<System.Int32>" )]
        public void GetField_ShouldReturnRawDataFieldNode_WhenColumnIsNullable(bool isTemporary, string expected)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "Col0", isNullable: true ) } );

            var sut = table.AsSet();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>( isNullable: true ) ),
                    text.TestEquals( expected ) )
                .Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : Nullable<System.Int32>" )]
        [InlineData( true, "TEMP.[foo].[Col0] : Nullable<System.Int32>" )]
        public void GetField_ShouldReturnRawDataFieldNode_WithNullableType_WhenTableIsOptional(bool isTemporary, string expected)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet().MarkAsOptional();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>( isNullable: true ) ),
                    text.TestEquals( expected ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void GetField_ShouldReturnRawDataFieldNode_WithAlias(bool isTemporary)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet( "qux" );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "Col0" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[qux].[Col0] : System.Int32" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet();

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet( "bar" );

            var result = sut["Col0"];

            result.TestRefEquals( sut.GetField( "Col0" ) ).Go();
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[x] : System.Int32" )]
        [InlineData( true, "TEMP.[foo].[x] : System.Int32" )]
        public void GetRawField_ShouldReturnRawDataFieldNode(bool isTemporary, string expected)
        {
            var info = isTemporary ? SqlRecordSetInfo.CreateTemporary( "foo" ) : SqlRecordSetInfo.Create( "foo", "bar" );
            var table = SqlNode.CreateTable( info, new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet();
            var result = sut.GetRawField( "x", TypeNullability.Create<int>() );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "x" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( expected ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet().MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNewTableNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet().MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "foo" ),
                    result.IsAliased.TestFalse(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNewTableNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var table = SqlNode.CreateTable( SqlRecordSetInfo.Create( "foo" ), Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet( "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CreationNode.TestRefEquals( sut.CreationNode ),
                    result.Info.TestEquals( sut.CreationNode.Info ),
                    result.Alias.TestEquals( "bar" ),
                    result.Identifier.TestEquals( "bar" ),
                    result.IsAliased.TestTrue(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }
    }
}
