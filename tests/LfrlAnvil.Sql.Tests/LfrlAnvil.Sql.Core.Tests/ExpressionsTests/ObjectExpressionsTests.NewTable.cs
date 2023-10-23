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
        [InlineData( true, "TEMP.[foo].[bar] AS [qux]" )]
        public void ToString_ShouldReturnCorrectRepresentation(bool isTemporary, string expected)
        {
            var table = SqlNode.CreateTable( "foo", "bar", Array.Empty<SqlColumnDefinitionNode>(), isTemporary: isTemporary );
            var sut = table.AsSet( "qux" );

            var result = sut.ToString();

            result.Should().Be( expected );
        }

        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownColumns()
        {
            var table = SqlNode.CreateTable(
                string.Empty,
                "foo",
                new[]
                {
                    SqlNode.Column<int>( "Col0" ),
                    SqlNode.Column<string>( "Col1" )
                } );

            var sut = table.AsSet();

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.GetField( "Col0" ), sut.GetField( "Col1" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateNewTableNode_WithNewAlias()
        {
            var table = SqlNode.CreateTable( string.Empty, "foo", Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet();
            var result = sut.As( "bar" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.SourceSchemaName.Should().Be( sut.CreationNode.SchemaName );
                result.SourceName.Should().Be( sut.CreationNode.Name );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldCreateNewTableNode_WithoutAlias()
        {
            var table = SqlNode.CreateTable( "foo", "bar", Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet( "qux" );
            var result = sut.AsSelf();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.SourceSchemaName.Should().Be( sut.CreationNode.SchemaName );
                result.SourceName.Should().Be( sut.CreationNode.Name );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "foo.bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeFalse();
            }
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : System.Int32" )]
        [InlineData( true, "TEMP.[foo].[bar].[Col0] : System.Int32" )]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnExists(bool isTemporary, string expectedText)
        {
            var table = SqlNode.CreateTable( "foo", "bar", new[] { SqlNode.Column<int>( "Col0" ) }, isTemporary: isTemporary );
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
                text.Should().Be( expectedText );
            }
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[x] : ?" )]
        [InlineData( true, "TEMP.[foo].[bar].[x] : ?" )]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnDoesNotExist(bool isTemporary, string expected)
        {
            var table = SqlNode.CreateTable( "foo", "bar", Array.Empty<SqlColumnDefinitionNode>(), isTemporary: isTemporary );
            var sut = table.AsSet();
            var result = sut.GetUnsafeField( "x" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "x" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( expected );
            }
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : System.Int32" )]
        [InlineData( true, "TEMP.[foo].[bar].[Col0] : System.Int32" )]
        public void GetField_ShouldReturnRawDataFieldNode(bool isTemporary, string expected)
        {
            var table = SqlNode.CreateTable( "foo", "bar", new[] { SqlNode.Column<int>( "Col0" ) }, isTemporary: isTemporary );
            var sut = table.AsSet();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( expected );
            }
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : Nullable<System.Int32>" )]
        [InlineData( true, "TEMP.[foo].[bar].[Col0] : Nullable<System.Int32>" )]
        public void GetField_ShouldReturnRawDataFieldNode_WhenColumnIsNullable(bool isTemporary, string expected)
        {
            var table = SqlNode.CreateTable(
                "foo",
                "bar",
                new[] { SqlNode.Column<int>( "Col0", isNullable: true ) },
                isTemporary: isTemporary );

            var sut = table.AsSet();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
                text.Should().Be( expected );
            }
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[Col0] : Nullable<System.Int32>" )]
        [InlineData( true, "TEMP.[foo].[bar].[Col0] : Nullable<System.Int32>" )]
        public void GetField_ShouldReturnRawDataFieldNode_WithNullableType_WhenTableIsOptional(bool isTemporary, string expected)
        {
            var table = SqlNode.CreateTable( "foo", "bar", new[] { SqlNode.Column<int>( "Col0" ) }, isTemporary: isTemporary );
            var sut = table.AsSet().MarkAsOptional();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
                text.Should().Be( expected );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void GetField_ShouldReturnRawDataFieldNode_WithAlias(bool isTemporary)
        {
            var table = SqlNode.CreateTable( "foo", "bar", new[] { SqlNode.Column<int>( "Col0" ) }, isTemporary: isTemporary );
            var sut = table.AsSet( "qux" );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "[qux].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenColumnDoesNotExist()
        {
            var table = SqlNode.CreateTable( string.Empty, "foo", new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet();

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var table = SqlNode.CreateTable( string.Empty, "foo", new[] { SqlNode.Column<int>( "Col0" ) } );
            var sut = table.AsSet( "bar" );

            var result = sut["Col0"];

            result.Should().BeSameAs( sut.GetField( "Col0" ) );
        }

        [Theory]
        [InlineData( false, "[foo].[bar].[x] : System.Int32" )]
        [InlineData( true, "TEMP.[foo].[bar].[x] : System.Int32" )]
        public void GetRawField_ShouldReturnRawDataFieldNode(bool isTemporary, string expected)
        {
            var table = SqlNode.CreateTable( "foo", "bar", new[] { SqlNode.Column<int>( "Col0" ) }, isTemporary: isTemporary );
            var sut = table.AsSet();
            var result = sut.GetRawField( "x", SqlExpressionType.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "x" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( expected );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var table = SqlNode.CreateTable( string.Empty, "foo", Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet().MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNewTableNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var table = SqlNode.CreateTable( string.Empty, "foo", Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet().MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.SourceSchemaName.Should().Be( sut.CreationNode.SchemaName );
                result.SourceName.Should().Be( sut.CreationNode.Name );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "foo" );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNewTableNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var table = SqlNode.CreateTable( string.Empty, "foo", Array.Empty<SqlColumnDefinitionNode>() );
            var sut = table.AsSet( "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CreationNode.Should().BeSameAs( sut.CreationNode );
                result.SourceSchemaName.Should().Be( sut.CreationNode.SchemaName );
                result.SourceName.Should().Be( sut.CreationNode.Name );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
            }
        }
    }
}
