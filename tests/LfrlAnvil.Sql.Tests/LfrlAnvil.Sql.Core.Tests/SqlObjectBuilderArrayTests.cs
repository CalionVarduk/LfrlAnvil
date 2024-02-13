using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

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

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Should().BeSequentiallyEqualTo( table, c1, c2 );
            sut[0].Should().Be( table );
            sut[1].Should().Be( c1 );
            sut[2].Should().Be( c2 );
        }
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldReturnCorrectArray()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" );
        var c2 = table.Columns.Create( "C2" );
        var sut = SqlObjectBuilderArray<ISqlObjectBuilder>.From<SqlObjectBuilder>( new SqlObjectBuilder[] { table, c1, c2 } );

        var result = sut.UnsafeReinterpretAs<SqlObjectBuilder>();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 3 );
            result.Should().BeSequentiallyEqualTo( table, c1, c2 );
        }
    }
}
