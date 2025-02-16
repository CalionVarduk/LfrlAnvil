using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteIndexTests : TestsBase
{
    [Theory]
    [InlineData( true, false )]
    [InlineData( false, true )]
    [InlineData( false, false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(bool isUnique, bool isVirtual)
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        var c2Builder = tableBuilder.Columns.Create( "C2" );

        tableBuilder.Constraints.CreateIndex( c1Builder.Asc(), c2Builder.Desc() )
            .SetName( "IX_TEST" )
            .MarkAsUnique( isUnique )
            .MarkAsVirtual( isVirtual );

        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );
        var c2 = table.Columns.Get( "C2" );

        var sut = schema.Objects.GetIndex( "IX_TEST" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Index ),
                sut.Name.TestEquals( "IX_TEST" ),
                sut.IsUnique.TestEquals( isUnique ),
                sut.IsVirtual.TestEquals( isVirtual ),
                sut.IsPartial.TestFalse(),
                sut.Columns.TestSequence(
                    [ new SqlIndexed<SqliteColumn>( c1, OrderBy.Asc ), new SqlIndexed<SqliteColumn>( c2, OrderBy.Desc ) ] ),
                sut.ToString().TestEquals( "[Index] foo_IX_TEST" ) )
            .Go();
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_ForPartialIndex()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.CreateIndex( c1Builder.Asc() ).SetName( "IX_TEST" ).SetFilter( SqlNode.True() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );

        var sut = schema.Objects.GetIndex( "IX_TEST" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Index ),
                sut.Name.TestEquals( "IX_TEST" ),
                sut.IsUnique.TestFalse(),
                sut.IsVirtual.TestFalse(),
                sut.IsPartial.TestTrue(),
                sut.Columns.TestSequence( [ new SqlIndexed<SqliteColumn>( c1, OrderBy.Asc ) ] ),
                sut.ToString().TestEquals( "[Index] foo_IX_TEST" ) )
            .Go();
    }
}
