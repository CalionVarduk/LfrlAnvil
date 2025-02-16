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
            var query = dataSource.Select( dataSource.From["a"].AsSelf(), dataSource.From.GetUnsafeField( "c" ).AsSelf() );

            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestSetEqual( [ sut.GetField( "a" ), sut.GetField( "c" ) ] ) )
                .Go();
        }

        [Fact]
        public void As_ShouldCreateCommonTableExpressionRecordSetNode_WithNewAlias()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.As( "qux" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CommonTableExpression.TestRefEquals( sut.CommonTableExpression ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "A" ) ),
                    result.Alias.TestEquals( "qux" ),
                    result.Identifier.TestEquals( "qux" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf_WhenNotAliased()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut.AsSelf();

            result.TestRefEquals( sut ).Go();
        }

        [Fact]
        public void AsSelf_ShouldCreateCommonTableExpressionRecordSetNode_WhenAliased()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet.As( "B" );
            var result = sut.As( "qux" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CommonTableExpression.TestRefEquals( sut.CommonTableExpression ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "A" ) ),
                    result.Alias.TestEquals( "qux" ),
                    result.Identifier.TestEquals( "qux" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
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

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType()
                        .AssignableTo<SqlQueryDataFieldNode>(
                            dataField => Assertion.All(
                                dataField.Selection.TestRefEquals( selection ),
                                dataField.Expression.TestRefEquals( dataSource["common.T1"]["a"] ) ) ),
                    text.TestEquals( "[A].[a]" ) )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawQueryDataFieldNode_WhenNameIsNotKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetUnsafeField( "b" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "b" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType().AssignableTo<SqlRawDataFieldNode>( dataField => dataField.Type.TestNull() ),
                    text.TestEquals( "[A].[b] : ?" ) )
                .Go();
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

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Selection.TestRefEquals( selection ),
                    result.Expression.TestRefEquals( dataSource["common.T1"]["a"] ),
                    text.TestEquals( "[A].[a]" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenNameIsNotKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var action = Lambda.Of( () => sut.GetField( "b" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;

            var result = sut["a"];

            result.TestRefEquals( sut.GetField( "a" ) ).Go();
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource.GetAll() );
            var sut = query.ToCte( "A" ).RecordSet;
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[A].[bar] : System.Int32" ) )
                .Go();
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

            result.TestRefEquals( sut ).Go();
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

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.CommonTableExpression.TestRefEquals( sut.CommonTableExpression ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "A" ) ),
                    result.Alias.TestNull(),
                    result.Identifier.TestEquals( "A" ),
                    result.IsAliased.TestFalse(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Fact]
        public void FieldInit_ShouldThrowArgumentException_WhenMultipleKnownFieldsWithSameNameExist()
        {
            var dataSource = SqlTableMock.Create<int>( "T1", new[] { "a" } ).Node.ToDataSource();
            var query = dataSource.Select( dataSource["common.T1"].GetAll(), dataSource["common.T1"]["a"].AsSelf() );
            var sut = query.ToCte( "A" ).RecordSet;

            var action = Lambda.Of( () => sut.GetField( "a" ) );

            action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
        }
    }
}
