using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteIndexTests : TestsBase
{
    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(bool isUnique)
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        var c2Builder = tableBuilder.Columns.Create( "C2" );
        tableBuilder.Indexes.Create( c1Builder.Asc(), c2Builder.Desc() ).SetName( "IX_TEST" ).MarkAsUnique( isUnique );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );
        var c2 = table.Columns.Get( "C2" );

        ISqlIndex sut = schema.Objects.GetIndex( "IX_TEST" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Index );
            sut.Name.Should().Be( "IX_TEST" );
            sut.FullName.Should().Be( "foo_IX_TEST" );
            sut.IsUnique.Should().Be( isUnique );
            ((SqliteIndex)sut).Columns.ToArray().Should().BeSequentiallyEqualTo( c1.Asc(), c2.Desc() );
            sut.Columns.ToArray().Should().BeSequentiallyEqualTo( c1.Asc(), c2.Desc() );
            sut.ToString().Should().Be( "[Index] foo_IX_TEST" );
        }
    }
}
