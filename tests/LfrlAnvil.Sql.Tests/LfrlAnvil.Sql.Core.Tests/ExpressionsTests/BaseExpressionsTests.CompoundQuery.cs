using System.Linq;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Tests.Helpers;

namespace LfrlAnvil.Sql.Tests.ExpressionsTests;

public partial class BaseExpressionsTests
{
    public class CompoundQuery : TestsBase
    {
        [Fact]
        public void Selection_ShouldContainTypedSelections_WhenAllQueriesHaveSimilarSelections()
        {
            var set1 = SqlNode.RawRecordSet( "foo" ).ToDataSource();
            var set2 = SqlNode.RawRecordSet( "bar" ).ToDataSource();

            var query1 = set1.Select(
                set1.From.GetRawField( "a", SqlExpressionType.Create<int>() ).AsSelf(),
                set1.From.GetRawField( "b", SqlExpressionType.Create<string>() ).AsSelf() );

            var query2 = set2.Select(
                set2.From.GetRawField( "a", SqlExpressionType.Create<int>( isNullable: true ) ).AsSelf(),
                set2.From.GetRawField( "b", SqlExpressionType.Create<string>() ).AsSelf() );

            var sut = query1.CompoundWith( query2.ToUnion() );

            var result = sut.Selection.ToArray();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );

                result.ElementAtOrDefault( 0 )
                    .Should()
                    .BeEquivalentTo( SqlNode.RawSelect( "a", alias: null, SqlExpressionType.Create<int>( isNullable: true ) ) );

                result.ElementAtOrDefault( 1 )
                    .Should()
                    .BeEquivalentTo( SqlNode.RawSelect( "b", alias: null, SqlExpressionType.Create<string>() ) );
            }
        }

        [Fact]
        public void Selection_ShouldContainUntypedSelections_WhenFirstQueryIsRaw()
        {
            var set2 = SqlNode.RawRecordSet( "bar" ).ToDataSource();
            var query1 = SqlNode.RawQuery( "SELECT a, b FROM foo" );

            var query2 = set2.Select(
                set2.From.GetRawField( "a", SqlExpressionType.Create<int>( isNullable: true ) ).AsSelf(),
                set2.From.GetRawField( "b", SqlExpressionType.Create<string>() ).AsSelf() );

            var sut = query1.CompoundWith( query2.ToUnion() );

            var result = sut.Selection.ToArray();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.ElementAtOrDefault( 0 ).Should().BeEquivalentTo( SqlNode.RawSelect( "a", alias: null, type: null ) );
                result.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.RawSelect( "b", alias: null, type: null ) );
            }
        }

        [Fact]
        public void Selection_ShouldContainUntypedSelections_WhenFollowingQueryIsRaw()
        {
            var set1 = SqlNode.RawRecordSet( "bar" ).ToDataSource();
            var set3 = SqlNode.RawRecordSet( "qux" ).ToDataSource();

            var query1 = set1.Select(
                set1.From.GetRawField( "a", SqlExpressionType.Create<int>( isNullable: true ) ).AsSelf(),
                set1.From.GetRawField( "b", SqlExpressionType.Create<string>() ).AsSelf() );

            var query2 = SqlNode.RawQuery( "SELECT a, b FROM foo" );

            var query3 = set3.Select(
                set3.From.GetRawField( "a", SqlExpressionType.Create<int>() ).AsSelf(),
                set3.From.GetRawField( "b", SqlExpressionType.Create<long>() ).AsSelf() );

            var sut = query1.CompoundWith( query2.ToUnion(), query3.ToUnion() );

            var result = sut.Selection.ToArray();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.ElementAtOrDefault( 0 ).Should().BeEquivalentTo( SqlNode.RawSelect( "a", alias: null, type: null ) );
                result.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.RawSelect( "b", alias: null, type: null ) );
            }
        }

        [Fact]
        public void Selection_ShouldContainUntypedSelections_WhenFieldTypesAreIncompatible()
        {
            var set1 = SqlNode.RawRecordSet( "bar" ).ToDataSource();
            var set2 = SqlNode.RawRecordSet( "qux" ).ToDataSource();

            var query1 = set1.Select(
                set1.From.GetRawField( "a", SqlExpressionType.Create<int>( isNullable: true ) ).AsSelf(),
                set1.From.GetRawField( "b", SqlExpressionType.Create<string>() ).AsSelf() );

            var query2 = set2.Select(
                set2.From.GetRawField( "a", SqlExpressionType.Create<string>() ).AsSelf(),
                SqlNode.RawSelect( "b", alias: null, type: SqlExpressionType.Create<long>() ) );

            var sut = query1.CompoundWith( query2.ToUnion() );

            var result = sut.Selection.ToArray();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );
                result.ElementAtOrDefault( 0 ).Should().BeEquivalentTo( SqlNode.RawSelect( "a", alias: null, type: null ) );
                result.ElementAtOrDefault( 1 ).Should().BeEquivalentTo( SqlNode.RawSelect( "b", alias: null, type: null ) );
            }
        }

        [Fact]
        public void Selection_ShouldFlattenSelectAllNodesToKnownFields()
        {
            var set1 = TableMock.Create( "T1", areColumnsNullable: false, "a", "b" ).ToRecordSet().ToDataSource();
            var t2 = TableMock.Create( "T2", areColumnsNullable: false, "a", "b" ).ToRecordSet();
            var t3 = TableMock.Create( "T3", areColumnsNullable: false, "c" ).ToRecordSet();
            var set2 = t2.Join( t3.InnerOn( t2["a"] == t3["c"] ) );

            var query1 = set1.Select( set1.GetAll() );
            var query2 = set2.Select( set2["T2"].GetAll() );

            var sut = query1.CompoundWith( query2.ToUnion() );

            var result = sut.Selection.ToArray();

            using ( new AssertionScope() )
            {
                result.Should().HaveCount( 2 );

                result.ElementAtOrDefault( 0 )
                    .Should()
                    .BeEquivalentTo( SqlNode.RawSelect( "a", alias: null, SqlExpressionType.Create<int>() ) );

                result.ElementAtOrDefault( 1 )
                    .Should()
                    .BeEquivalentTo( SqlNode.RawSelect( "b", alias: null, SqlExpressionType.Create<int>() ) );
            }
        }
    }
}
