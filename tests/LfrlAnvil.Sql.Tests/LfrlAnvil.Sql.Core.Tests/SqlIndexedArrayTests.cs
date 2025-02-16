using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlIndexedArrayTests : TestsBase
{
    [Fact]
    public void From_ShouldCreateCorrectArray()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var tableBuilder = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        tableBuilder.Columns.Create( "C2" );
        tableBuilder.Columns.Create( "C3" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var c1 = new SqlIndexed<SqlColumnMock>( ( SqlColumnMock )table.Columns.Get( "C1" ), OrderBy.Asc );
        var c2 = new SqlIndexed<SqlColumnMock>( ( SqlColumnMock )table.Columns.Get( "C2" ), OrderBy.Desc );
        var c3 = new SqlIndexed<SqlColumnMock>( null, OrderBy.Asc );

        var sut = SqlIndexedArray<SqlColumnMock>.From( new SqlIndexed<ISqlColumn>[] { c1, c2, c3 } );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.TestSequence( [ c1, c2, c3 ] ),
                sut[0].TestEquals( c1 ),
                sut[1].TestEquals( c2 ),
                sut[2].TestEquals( c3 ) )
            .Go();
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldCreateCorrectArray()
    {
        var dbBuilder = SqlDatabaseBuilderMock.Create();
        var tableBuilder = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        tableBuilder.Columns.Create( "C2" );
        tableBuilder.Columns.Create( "C3" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var c1 = new SqlIndexed<ISqlColumn>( table.Columns.Get( "C1" ), OrderBy.Asc );
        var c2 = new SqlIndexed<ISqlColumn>( table.Columns.Get( "C2" ), OrderBy.Desc );
        var c3 = new SqlIndexed<ISqlColumn>( null, OrderBy.Asc );
        var sut = SqlIndexedArray<SqlColumnMock>.From( new[] { c1, c2, c3 } );

        var result = sut.UnsafeReinterpretAs<SqlColumn>();

        Assertion.All(
                result.Count.TestEquals( 3 ),
                result.TestSequence(
                [
                    new SqlIndexed<SqlColumn>( table.Columns.Get( "C1" ), OrderBy.Asc ),
                    new SqlIndexed<SqlColumn>( table.Columns.Get( "C2" ), OrderBy.Desc ),
                    new SqlIndexed<SqlColumn>( null, OrderBy.Asc )
                ] ) )
            .Go();
    }
}
