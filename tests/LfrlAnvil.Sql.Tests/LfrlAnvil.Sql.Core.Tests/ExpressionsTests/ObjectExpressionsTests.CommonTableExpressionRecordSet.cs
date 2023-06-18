using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class CommonTableExpressionRecordSet : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownQueryFields()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a", "b" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select(
                dataSource.From["a"].AsSelf(),
                dataSource.From.GetUnsafeField( "c" ).AsSelf(),
                SqlNode.RawSelect( "T1", "f", alias: "g" ) );

            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 3 );
                result.Should().BeEquivalentTo( sut.GetField( "a" ), sut.GetField( "c" ), sut.GetField( "g" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateCommonTableExpressionRecordSetNode_WithNewAlias()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a", "b" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.As( "qux" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CommonTableExpression.Should().BeSameAs( sut.CommonTableExpression );
                result.Name.Should().Be( "qux" );
                result.Alias.Should().Be( "qux" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf_WhenNotAliased()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a", "b" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut.AsSelf();

            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void AsSelf_ShouldCreateCommonTableExpressionRecordSetNode_WhenAliased()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a", "b" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet.As( "B" );
            var result = sut.As( "qux" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CommonTableExpression.Should().BeSameAs( sut.CommonTableExpression );
                result.Name.Should().Be( "qux" );
                result.Alias.Should().Be( "qux" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnKnownQueryDataFieldNode()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetUnsafeField( "a" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "a" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "[A].[a] : System.Int32" );
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawQueryDataFieldNode_WhenNameIsNotKnown()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetUnsafeField( "b" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "b" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().BeNull();
                text.Should().Be( "[A].[b] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetField( "a" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "a" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "[A].[a] : System.Int32" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenNameIsNotKnown()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var action = Lambda.Of( () => sut.GetField( "b" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut["a"];

            result.Should().BeSameAs( sut.GetField( "a" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetRawField( "bar", SqlExpressionType.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( SqlExpressionType.Create<int>() );
                text.Should().Be( "[A].[bar] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet.MarkAsOptional( optional );

            var result = sut.MarkAsOptional( optional );

            result.Should().BeSameAs( sut );
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnCommonTableExpressionRecordSetNode_WhenOptionalityChanges(bool optional)
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet.MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CommonTableExpression.Should().BeSameAs( sut.CommonTableExpression );
                result.Name.Should().Be( sut.Name );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Fact]
        public void OptionalCommonTableExpression_ShouldChangeAllFieldTypesToOptional()
        {
            var dataSource = TableMock.Create( "T1", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet.MarkAsOptional();

            var field = sut.GetField( "a" );

            field.Type.Should().Be( SqlExpressionType.Create<int>( isNullable: true ) );
        }

        [Fact]
        public void FieldInit_ShouldThrowArgumentException_WhenMultipleKnownFieldsWithSameNameExist()
        {
            var dataSource = TableMock.Create( "t", areColumnsNullable: false, "a" ).ToRecordSet().ToDataSource();
            var query = dataSource.Select( dataSource["t"].GetAll(), dataSource["t"]["a"].AsSelf() );
            var sut = query.ToCte( "A" ).RecordSet;

            var action = Lambda.Of( () => sut.GetField( "a" ) );

            action.Should().ThrowExactly<ArgumentException>();
        }
    }
}
