using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlIndexColumnBuilderArrayTests : TestsBase
{
    [Fact]
    public void From_ShouldCreateCorrectArray()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" ).Asc();
        var c2 = table.Columns.Create( "C2" ).Desc();
        var c3 = table.Columns.Create( "C3" ).Asc();

        var sut = SqlIndexColumnBuilderArray<SqlColumnBuilderMock>.From( new SqlIndexColumnBuilder<ISqlColumnBuilder>[] { c1, c2, c3 } );

        using ( new AssertionScope() )
        {
            sut.Count.Should().Be( 3 );
            sut.Should().BeSequentiallyEqualTo( c1, c2, c3 );
            sut[0].Should().Be( c1 );
            sut[1].Should().Be( c2 );
            sut[2].Should().Be( c3 );
        }
    }

    [Fact]
    public void UnsafeReinterpretAs_ShouldCreateCorrectArray()
    {
        var table = SqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var c1 = table.Columns.Create( "C1" ).Asc();
        var c2 = table.Columns.Create( "C2" ).Desc();
        var c3 = table.Columns.Create( "C3" ).Asc();
        var sut = SqlIndexColumnBuilderArray<ISqlColumnBuilder>.From( new SqlIndexColumnBuilder<ISqlColumnBuilder>[] { c1, c2, c3 } );

        var result = sut.UnsafeReinterpretAs<SqlColumnBuilder>();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 3 );
            result.Should()
                .BeSequentiallyEqualTo(
                    c1.UnsafeReinterpretAs<SqlColumnBuilder>(),
                    c2.UnsafeReinterpretAs<SqlColumnBuilder>(),
                    c3.UnsafeReinterpretAs<SqlColumnBuilder>() );
        }
    }
}
