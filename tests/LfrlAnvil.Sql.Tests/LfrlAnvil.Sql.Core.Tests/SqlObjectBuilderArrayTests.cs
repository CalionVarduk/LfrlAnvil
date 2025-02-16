using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlObjectBuilderArrayTests : TestsBase
{
    [Fact]
    public void From_ShouldCreateCorrectArray()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );

        var sut = SqlObjectBuilderArray<SqlObjectBuilder>.From<SqlObjectBuilder>( new SqlObjectBuilder[] { table, c1, c2 } );

        Assertion.All(
                sut.Count.TestEquals( 3 ),
                sut.TestSequence( [ table, c1, c2 ] ),
                sut[0].TestEquals( table ),
                sut[1].TestEquals( c1 ),
                sut[2].TestEquals( c2 ) )
            .Go();
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldReturnCorrectArray()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var sut = SqlObjectBuilderArray<ISqlObjectBuilder>.From<SqlObjectBuilder>( new SqlObjectBuilder[] { table, c1, c2 } );

        var result = sut.UnsafeReinterpretAs<SqlObjectBuilder>();

        Assertion.All(
                result.Count.TestEquals( 3 ),
                result.TestSequence( [ table, c1, c2 ] ) )
            .Go();
    }
}
