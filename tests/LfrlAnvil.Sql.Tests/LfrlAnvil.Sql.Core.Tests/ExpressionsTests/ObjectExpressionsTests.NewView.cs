using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class NewView : TestsBase
    {
        [Fact]
        public void ToString_ShouldReturnCorrectRepresentation()
        {
            var view = SqlNode.CreateView( "foo", "bar", SqlNode.RawQuery( "SELECT * FROM qux" ) );
            var sut = view.AsSet( "qux" );

            var result = sut.ToString();

            result.Should().Be( "[foo].[bar] AS [qux]" );
        }

        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownRecordSetsFields_WithSelectAll()
        {
            var t1 = TableMock.Create( "T1", ColumnMock.CreateMany<int>( areNullable: false, "a", "b" ) ).ToRecordSet();
            var t2 = TableMock.Create( "T2", ColumnMock.CreateMany<int>( areNullable: false, "c", "d" ) ).ToRecordSet();
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( string.Empty, "foo" ).AsSet();

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 4 );
                result.Should().BeEquivalentTo( sut.GetField( "a" ), sut.GetField( "b" ), sut.GetField( "c" ), sut.GetField( "d" ) );
            }
        }

        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownSingleRecordSetFields_WithSelectAllFromRecordSet()
        {
            var t1 = TableMock.Create( "T1", ColumnMock.CreateMany<int>( areNullable: false, "a", "b" ) ).ToRecordSet();
            var t2 = TableMock.Create( "T2", ColumnMock.CreateMany<int>( areNullable: false, "c", "d" ) ).ToRecordSet();
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select( dataSource.From.GetAll() ).ToCreateView( string.Empty, "foo" ).AsSet();

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.GetField( "a" ), sut.GetField( "b" ) );
            }
        }

        [Fact]
        public void GetKnownFields_ShouldReturnExplicitSelections_WithFieldSelectionsOnly()
        {
            var t1 = TableMock.Create( "T1", ColumnMock.CreateMany<int>( areNullable: false, "a", "b" ) ).ToRecordSet();
            var t2 = TableMock.Create( "T2", ColumnMock.CreateMany<int>( areNullable: false, "c", "d" ) ).ToRecordSet();
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select(
                    dataSource["T1"]["a"].AsSelf(),
                    dataSource["T2"]["d"].AsSelf(),
                    dataSource["T1"].GetUnsafeField( "e" ).AsSelf() )
                .ToCreateView( string.Empty, "foo" )
                .AsSet();

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 3 );
                result.Should().BeEquivalentTo( sut.GetField( "a" ), sut.GetField( "d" ), sut.GetField( "e" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateNewViewNode_WithNewAlias()
        {
            var view = SqlNode.CreateView( string.Empty, "foo", SqlNode.RawQuery( "SELECT * FROM qux" ) );
            var sut = view.AsSet();
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
        public void AsSelf_ShouldCreateNewViewNode_WithoutAlias()
        {
            var view = SqlNode.CreateView( "foo", "bar", SqlNode.RawQuery( "SELECT * FROM lorem" ) );
            var sut = view.AsSet( "qux" );
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

        [Fact]
        public void GetUnsafeField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).ToCreateView( string.Empty, "foo" ).AsSet();
            var result = sut.GetUnsafeField( "a" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.QueryDataField );
                result.Name.Should().Be( "a" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlQueryDataFieldNode;
                (dataField?.Selection).Should().BeSameAs( selection );
                (dataField?.Expression).Should().BeSameAs( dataSource["t"]["a"] );
                text.Should().Be( "[foo].[a]" );
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenNameIsNotKnown()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( "foo", "bar" ).AsSet();
            var result = sut.GetUnsafeField( "b" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "b" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( "[foo].[bar].[b] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).ToCreateView( string.Empty, "foo" ).AsSet();
            var result = sut.GetField( "a" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.QueryDataField );
                result.Name.Should().Be( "a" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Selection.Should().BeSameAs( selection );
                result.Expression.Should().BeSameAs( dataSource["t"]["a"] );
                text.Should().Be( "[foo].[a]" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenNameIsNotKnown()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( string.Empty, "foo" ).AsSet();

            var action = Lambda.Of( () => sut.GetField( "b" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( string.Empty, "foo" ).AsSet();

            var result = sut["a"];

            result.Should().BeSameAs( sut.GetField( "a" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).ToCreateView( "foo", "bar" ).AsSet( "qux" );
            var result = sut.GetRawField( "x", SqlExpressionType.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "x" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "[qux].[x] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var view = SqlNode.CreateView( string.Empty, "foo", SqlNode.RawQuery( "SELECT * FROM bar" ) );
            var sut = view.AsSet().MarkAsOptional( optional );
            var result = sut.MarkAsOptional( optional );
            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnNewViewNode_WhenOptionalityChanges_WithoutAlias(bool optional)
        {
            var view = SqlNode.CreateView( string.Empty, "foo", SqlNode.RawQuery( "SELECT * FROM bar" ) );
            var sut = view.AsSet().MarkAsOptional( ! optional );
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
        public void MarkAsOptional_ShouldReturnNewViewNode_WhenOptionalityChanges_WithAlias(bool optional)
        {
            var view = SqlNode.CreateView( string.Empty, "foo", SqlNode.RawQuery( "SELECT * FROM qux" ) );
            var sut = view.AsSet( "bar" ).MarkAsOptional( ! optional );
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
