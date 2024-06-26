﻿using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class TableBuilder : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnCollectionWithKnownColumns()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0", "Col1" } );
            var sut = SqlNode.Table( table );

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Count.Should().Be( 2 );
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.GetField( "Col0" ), sut.GetField( "Col1" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateTableBuilderNode_WithNewAlias()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.As( "bar" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "foo" ) );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldCreateTableBuilderNode_WithoutAlias()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table, "qux" );
            var result = sut.AsSelf();

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "foo" ) );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "common.foo" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeFalse();
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnColumnBuilderNode_WhenColumnExists()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.GetUnsafeField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ColumnBuilder );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                var column = result as SqlColumnBuilderNode;
                (column?.Value).Should().BeSameAs( table.Columns.Get( "Col0" ) );
                (column?.Type).Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[common].[foo].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenColumnDoesNotExist()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.GetUnsafeField( "bar" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( "[common].[foo].[bar] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnBuilderNode()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ColumnBuilder );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[common].[foo].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnBuilderNode_WhenColumnIsNullable()
        {
            var table = SqlTableBuilderMock.Create<int>(
                "foo",
                new[] { "Col0", "Col1" },
                pkColumns: new[] { "Col1" },
                areColumnsNullable: true );

            var sut = SqlNode.Table( table );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ColumnBuilder );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>( isNullable: true ) );
                text.Should().Be( "[common].[foo].[Col0] : Nullable<System.Int32>" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnBuilderNode_WithNullableType_WhenTableIsOptional()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table ).MarkAsOptional();
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ColumnBuilder );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>( isNullable: true ) );
                text.Should().Be( "[common].[foo].[Col0] : Nullable<System.Int32>" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnColumnBuilderNode_WithAlias()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table, "bar" );
            var result = sut.GetField( "Col0" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.ColumnBuilder );
                result.Value.Should().BeSameAs( table.Columns.Get( "Col0" ) );
                result.Name.Should().Be( "Col0" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[bar].[Col0] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowException_WhenColumnDoesNotExist()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );

            var action = Lambda.Of( () => sut.GetField( "Col1" ) );

            action.Should().Throw<Exception>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table, "bar" );

            var result = sut["Col0"];

            result.Should().BeEquivalentTo( sut.GetField( "Col0" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table );
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[common].[foo].[bar] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table ).MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnTableBuilderNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "foo" ) );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "common.foo" );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnTableBuilderNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var sut = SqlNode.Table( table, "bar" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Table.Should().BeSameAs( sut.Table );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "common", "foo" ) );
                result.Alias.Should().Be( "bar" );
                result.Identifier.Should().Be( "bar" );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Fact]
        public void DataField_ReplaceRecordSet_ShouldReturnColumnBuilderNodeWithReplacedRecordSet()
        {
            var table = SqlTableBuilderMock.Create<int>( "foo", new[] { "Col0" } );
            var recordSet = SqlNode.RawRecordSet( "bar" );
            var sut = SqlNode.Table( table )["Col0"];
            var result = sut.ReplaceRecordSet( recordSet );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Name.Should().Be( sut.Name );
                result.Type.Should().Be( sut.Type );
                result.Value.Should().BeSameAs( sut.Value );
                result.RecordSet.Should().BeSameAs( recordSet );
            }
        }
    }
}
