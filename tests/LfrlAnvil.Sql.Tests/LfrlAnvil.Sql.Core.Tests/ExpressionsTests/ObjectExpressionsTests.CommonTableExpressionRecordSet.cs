using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class CommonTableExpressionRecordSet : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownQueryFields()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var query = dataSource.Select(
                dataSource.From["a"].AsSelf(),
                dataSource.From.GetUnsafeField( "c" ).AsSelf() );

            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut.GetKnownFields();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.Should().BeEquivalentTo( sut.GetField( "a" ), sut.GetField( "c" ) );
            }
        }

        [Fact]
        public void As_ShouldCreateCommonTableExpressionRecordSetNode_WithNewAlias()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.As( "qux" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CommonTableExpression.Should().BeSameAs( sut.CommonTableExpression );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "A" ) );
                result.Alias.Should().Be( "qux" );
                result.Identifier.Should().Be( "qux" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf_WhenNotAliased()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut.AsSelf();

            result.Should().BeSameAs( sut );
        }

        [Fact]
        public void AsSelf_ShouldCreateCommonTableExpressionRecordSetNode_WhenAliased()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet.As( "B" );
            var result = sut.As( "qux" );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CommonTableExpression.Should().BeSameAs( sut.CommonTableExpression );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "A" ) );
                result.Alias.Should().Be( "qux" );
                result.Identifier.Should().Be( "qux" );
                result.IsOptional.Should().Be( sut.IsOptional );
                result.IsAliased.Should().BeTrue();
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnKnownQueryDataFieldNode()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var selection = dataSource.GetAll();
            var query = dataSource.Select( selection );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetUnsafeField( "a" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.QueryDataField );
                result.Name.Should().Be( "a" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlQueryDataFieldNode;
                (dataField?.Selection).Should().BeSameAs( selection );
                (dataField?.Expression).Should().BeSameAs( dataSource["common.T1"]["a"] );
                text.Should().Be( "[A].[a]" );
            }
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawQueryDataFieldNode_WhenNameIsNotKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetUnsafeField( "b" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "b" );
                result.RecordSet.Should().BeSameAs( sut );
                var dataField = result as SqlRawDataFieldNode;
                (dataField?.Type).Should().BeNull();
                text.Should().Be( "[A].[b] : ?" );
            }
        }

        [Fact]
        public void GetField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var selection = dataSource.GetAll();
            var query = dataSource.Select( selection );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetField( "a" );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.QueryDataField );
                result.Name.Should().Be( "a" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Selection.Should().BeSameAs( selection );
                result.Expression.Should().BeSameAs( dataSource["common.T1"]["a"] );
                text.Should().Be( "[A].[a]" );
            }
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenNameIsNotKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var action = Lambda.Of( () => sut.GetField( "b" ) );

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut["a"];

            result.Should().BeSameAs( sut.GetField( "a" ) );
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            using ( new AssertionScope() )
            {
                result.NodeType.Should().Be( SqlNodeType.RawDataField );
                result.Name.Should().Be( "bar" );
                result.RecordSet.Should().BeSameAs( sut );
                result.Type.Should().Be( TypeNullability.Create<int>() );
                text.Should().Be( "[A].[bar] : System.Int32" );
            }
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
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
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet.MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            using ( new AssertionScope() )
            {
                result.Should().NotBeSameAs( sut );
                result.CommonTableExpression.Should().BeSameAs( sut.CommonTableExpression );
                result.Info.Should().Be( SqlRecordSetInfo.Create( "A" ) );
                result.Alias.Should().BeNull();
                result.Identifier.Should().Be( "A" );
                result.IsAliased.Should().BeFalse();
                result.IsOptional.Should().Be( optional );
            }
        }

        [Fact]
        public void FieldInit_ShouldThrowArgumentException_WhenMultipleKnownFieldsWithSameNameExist()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource["common.T1"].GetAll(), dataSource["common.T1"]["a"].AsSelf() );
            var sut = query.ToCte( "A" ).RecordSet;

            var action = Lambda.Of( () => sut.GetField( "a" ) );

            action.Should().ThrowExactly<ArgumentException>();
        }
    }
}
