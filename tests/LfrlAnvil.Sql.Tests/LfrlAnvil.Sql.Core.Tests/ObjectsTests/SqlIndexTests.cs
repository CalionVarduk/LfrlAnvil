using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlIndexTests : TestsBase
{
    [Theory]
    [InlineData( true, false )]
    [InlineData( false, true )]
    [InlineData( false, false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(bool isUnique, bool isVirtual)
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        var c2Builder = tableBuilder.Columns.Create( "C2" );

        tableBuilder.Constraints.CreateIndex( c1Builder.Asc(), c2Builder.Desc() )
            .SetName( "IX_TEST" )
            .MarkAsUnique( isUnique )
            .MarkAsVirtual( isVirtual );

        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );
        var c2 = table.Columns.Get( "C2" );

        ISqlIndex sut = schema.Objects.GetIndex( "IX_TEST" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Index ),
                sut.Name.TestEquals( "IX_TEST" ),
                sut.IsUnique.TestEquals( isUnique ),
                sut.IsVirtual.TestEquals( isVirtual ),
                sut.IsPartial.TestFalse(),
                (( SqlIndex )sut).Columns.TestSequence(
                    [ new SqlIndexed<SqlColumn>( c1, OrderBy.Asc ), new SqlIndexed<SqlColumn>( c2, OrderBy.Desc ) ] ),
                sut.Columns.TestSequence(
                    [ new SqlIndexed<ISqlColumn>( c1, OrderBy.Asc ), new SqlIndexed<ISqlColumn>( c2, OrderBy.Desc ) ] ),
                sut.ToString().TestEquals( "[Index] foo.IX_TEST" ) )
            .Go();
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_ForPartialIndex()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.CreateIndex( c1Builder.Asc() ).SetName( "IX_TEST" ).SetFilter( SqlNode.True() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );

        ISqlIndex sut = schema.Objects.GetIndex( "IX_TEST" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Index ),
                sut.Name.TestEquals( "IX_TEST" ),
                sut.IsUnique.TestFalse(),
                sut.IsVirtual.TestFalse(),
                sut.IsPartial.TestTrue(),
                sut.Columns.TestSequence( [ new SqlIndexed<ISqlColumn>( c1, OrderBy.Asc ) ] ),
                sut.ToString().TestEquals( "[Index] foo.IX_TEST" ) )
            .Go();
    }
}
