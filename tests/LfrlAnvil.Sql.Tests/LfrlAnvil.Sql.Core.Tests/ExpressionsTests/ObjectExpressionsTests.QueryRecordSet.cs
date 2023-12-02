﻿using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class QueryRecordSet : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownRecordSetsFields_WithSelectAll()
        {
            var t1 = TableMock.Create( "T1", ColumnMock.CreateMany<int>( areNullable: false, "a", "b" ) ).ToRecordSet();
            var t2 = TableMock.Create( "T2", ColumnMock.CreateMany<int>( areNullable: false, "c", "d" ) ).ToRecordSet();
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );

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
            var sut = dataSource.Select( dataSource.From.GetAll() ).AsSet( "foo" );

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
                .AsSet( "foo" );

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 3 );
                result.Should().BeEquivalentTo( sut.GetField( "a" ), sut.GetField( "d" ), sut.GetField( "e" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateQueryRecordSetNode_WithNewAlias()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "bar" );
            var result = sut.As( "qux" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Query.Should().BeSameAs( sut.Query );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "qux" ) );
                result.Alias.Should().Be( "qux" );
                result.Identifier.Should().Be( "qux" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "bar" );

            var result = sut.AsSelf();

            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).AsSet( "foo" );
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
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );
            var result = sut.GetUnsafeField( "b" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "b" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( "[foo].[b] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).AsSet( "foo" );
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
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );

            var action = Lambda.Of( () => sut.GetField( "b" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );

            var result = sut["a"];

            result.Should().BeSameAs( sut.GetField( "a" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[foo].[bar] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" ).MarkAsOptional( optional );

            var result = sut.MarkAsOptional( optional );

            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnQueryRecordSetNode_WhenOptionalityChanges(bool optional)
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.Query.Should().Be( sut.Query );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "foo" ) );
                result.Alias.Should().Be( "foo" );
                result.Identifier.Should().Be( "foo" );
                result.IsAliased.Should().BeTrue();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Fact]
        public void FieldInit_ShouldThrowArgumentException_WhenMultipleKnownFieldsWithSameNameExist()
        {
            var dataSource = TableMock.Create( "t", ColumnMock.Create<int>( "a" ) ).ToRecordSet().ToDataSource();
            var sut = dataSource.Select( dataSource["t"].GetAll(), dataSource["t"]["a"].AsSelf() ).AsSet( "foo" );

            var action = Lambda.Of( () => sut.GetField( "a" ) );

            action.Should().ThrowExactly<ArgumentException>();
        }
    }
}
