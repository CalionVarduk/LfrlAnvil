using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests;

public class SqlIndexColumnArrayTests : TestsBase
{
    [Fact]
    public void From_ShouldCreateCorrectArray()
    {
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var tableBuilder = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        tableBuilder.Columns.Create( "C2" );
        tableBuilder.Columns.Create( "C3" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" ).Asc().UnsafeReinterpretAs<SqlColumnMock>();
        var c2 = table.Columns.Get( "C2" ).Desc().UnsafeReinterpretAs<SqlColumnMock>();
        var c3 = table.Columns.Get( "C3" ).Asc().UnsafeReinterpretAs<SqlColumnMock>();

        var sut = SqlIndexColumnArray<SqlColumnMock>.From( new SqlIndexColumn<ISqlColumn>[] { c1, c2, c3 } );

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
        var dbBuilder = SqlDatabaseBuilderMockFactory.Create();
        var tableBuilder = dbBuilder.Schemas.Default.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() );
        tableBuilder.Columns.Create( "C2" );
        tableBuilder.Columns.Create( "C3" );
        var db = SqlDatabaseMock.Create( dbBuilder );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" ).Asc();
        var c2 = table.Columns.Get( "C2" ).Desc();
        var c3 = table.Columns.Get( "C3" ).Asc();
        var sut = SqlIndexColumnArray<SqlColumnMock>.From( new SqlIndexColumn<ISqlColumn>[] { c1, c2, c3 } );

        var result = sut.UnsafeReinterpretAs<SqlColumn>();

        using ( new AssertionScope() )
        {
            result.Count.Should().Be( 3 );
            result.Should()
                .BeSequentiallyEqualTo(
                    c1.UnsafeReinterpretAs<SqlColumn>(),
                    c2.UnsafeReinterpretAs<SqlColumn>(),
                    c3.UnsafeReinterpretAs<SqlColumn>() );
        }
    }
}
