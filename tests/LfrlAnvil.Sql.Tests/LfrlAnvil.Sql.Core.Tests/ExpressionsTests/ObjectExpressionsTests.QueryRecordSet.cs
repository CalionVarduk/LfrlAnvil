using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class ObjectExpressionsTests
{
    public class QueryRecordSet : TestsBase
    {
        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownRecordSetsFields_WithSelectAll()
        {
            var t1 = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node;
            var t2 = SqlTableMock.Create<int>( "T2", new[] { "c", "d" } ).Node;
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 4 ),
                    result.TestSetEqual( [ sut.GetField( "a" ), sut.GetField( "b" ), sut.GetField( "c" ), sut.GetField( "d" ) ] ) )
                .Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnAllKnownSingleRecordSetFields_WithSelectAllFromRecordSet()
        {
            var t1 = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node;
            var t2 = SqlTableMock.Create<int>( "T2", new[] { "c", "d" } ).Node;
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select( dataSource.From.GetAll() ).AsSet( "foo" );

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 2 ),
                    result.TestSetEqual( [ sut.GetField( "a" ), sut.GetField( "b" ) ] ) )
                .Go();
        }

        [Fact]
        public void GetKnownFields_ShouldReturnExplicitSelections_WithFieldSelectionsOnly()
        {
            var t1 = SqlTableMock.Create<int>( "T1", new[] { "a", "b" } ).Node;
            var t2 = SqlTableMock.Create<int>( "T2", new[] { "c", "d" } ).Node;
            var dataSource = t1.Join( SqlNode.InnerJoinOn( t2, SqlNode.True() ) );
            var sut = dataSource.Select(
                    dataSource["common.T1"]["a"].AsSelf(),
                    dataSource["common.T2"]["d"].AsSelf(),
                    dataSource["common.T1"].GetUnsafeField( "e" ).AsSelf() )
                .AsSet( "foo" );

            var result = sut.GetKnownFields();

            Assertion.All(
                    result.Count.TestEquals( 3 ),
                    result.TestSetEqual( [ sut.GetField( "a" ), sut.GetField( "d" ), sut.GetField( "e" ) ] ) )
                .Go();
        }

        [Fact]
        public void As_ShouldCreateQueryRecordSetNode_WithNewAlias()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "bar" );
            var result = sut.As( "qux" );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Query.TestRefEquals( sut.Query ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "qux" ) ),
                    result.Alias.TestEquals( "qux" ),
                    result.Identifier.TestEquals( "qux" ),
                    result.IsOptional.TestEquals( sut.IsOptional ),
                    result.IsAliased.TestTrue() )
                .Go();
        }

        [Fact]
        public void AsSelf_ShouldReturnSelf()
        {
            var dataSource = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "bar" );

            var result = sut.AsSelf();

            result.TestRefEquals( sut ).Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).AsSet( "foo" );
            var result = sut.GetUnsafeField( "a" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType()
                        .AssignableTo<SqlQueryDataFieldNode>( dataField => Assertion.All(
                            dataField.Selection.TestRefEquals( selection ),
                            dataField.Expression.TestRefEquals( dataSource["common.t"]["a"] ) ) ),
                    text.TestEquals( "[foo].[a]" ) )
                .Go();
        }

        [Fact]
        public void GetUnsafeField_ShouldReturnRawDataFieldNode_WhenNameIsNotKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );
            var result = sut.GetUnsafeField( "b" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "b" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.TestType().AssignableTo<SqlRawDataFieldNode>( dataField => dataField.Type.TestNull() ),
                    text.TestEquals( "[foo].[b] : ?" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldReturnQueryDataFieldNode_WhenNameIsKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var selection = dataSource.GetAll();
            var sut = dataSource.Select( selection ).AsSet( "foo" );
            var result = sut.GetField( "a" );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.QueryDataField ),
                    result.Name.TestEquals( "a" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Selection.TestRefEquals( selection ),
                    result.Expression.TestRefEquals( dataSource["common.t"]["a"] ),
                    text.TestEquals( "[foo].[a]" ) )
                .Go();
        }

        [Fact]
        public void GetField_ShouldThrowKeyNotFoundException_WhenNameIsNotKnown()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );

            var action = Lambda.Of( () => sut.GetField( "b" ) );

            action.Test( exc => exc.TestType().Exact<KeyNotFoundException>() ).Go();
        }

        [Fact]
        public void Indexer_ShouldBeEquivalentToGetField()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );

            var result = sut["a"];

            result.TestRefEquals( sut.GetField( "a" ) ).Go();
        }

        [Fact]
        public void GetRawField_ShouldReturnRawDataFieldNode()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" );
            var result = sut.GetRawField( "bar", TypeNullability.Create<int>() );
            var text = result.ToString();

            Assertion.All(
                    result.NodeType.TestEquals( SqlNodeType.RawDataField ),
                    result.Name.TestEquals( "bar" ),
                    result.RecordSet.TestRefEquals( sut ),
                    result.Type.TestEquals( TypeNullability.Create<int>() ),
                    text.TestEquals( "[foo].[bar] : System.Int32" ) )
                .Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnSelf_WhenOptionalityDoesNotChange(bool optional)
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" ).MarkAsOptional( optional );

            var result = sut.MarkAsOptional( optional );

            result.TestRefEquals( sut ).Go();
        }

        [Theory]
        [InlineData( false )]
        [InlineData( true )]
        public void MarkAsOptional_ShouldReturnQueryRecordSetNode_WhenOptionalityChanges(bool optional)
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" ).MarkAsOptional( ! optional );
            var result = sut.MarkAsOptional( optional );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Query.TestEquals( sut.Query ),
                    result.Info.TestEquals( SqlRecordSetInfo.Create( "foo" ) ),
                    result.Alias.TestEquals( "foo" ),
                    result.Identifier.TestEquals( "foo" ),
                    result.IsAliased.TestTrue(),
                    result.IsOptional.TestEquals( optional ) )
                .Go();
        }

        [Fact]
        public void FieldInit_ShouldThrowArgumentException_WhenMultipleKnownFieldsWithSameNameExist()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "a" } ).Node.ToDataSource();
            var sut = dataSource.Select( dataSource["common.t"].GetAll(), dataSource["common.t"]["a"].AsSelf() ).AsSet( "foo" );

            var action = Lambda.Of( () => sut.GetField( "a" ) );

            action.Test( exc => exc.TestType().Exact<ArgumentException>() ).Go();
        }

        [Fact]
        public void DataField_ReplaceRecordSet_ShouldReturnQueryDataFieldNodeWithReplacedRecordSet()
        {
            var dataSource = SqlTableMock.Create<int>( "t", new[] { "Col0" } ).Node.ToDataSource();
            var recordSet = SqlNode.RawRecordSet( "bar" );
            var sut = dataSource.Select( dataSource.GetAll() ).AsSet( "foo" )["Col0"];
            var result = sut.ReplaceRecordSet( recordSet );

            Assertion.All(
                    result.TestNotRefEquals( sut ),
                    result.Name.TestEquals( sut.Name ),
                    result.Selection.TestRefEquals( sut.Selection ),
                    result.Expression.TestRefEquals( sut.Expression ),
                    result.RecordSet.TestRefEquals( recordSet ) )
                .Go();
        }
    }
}
