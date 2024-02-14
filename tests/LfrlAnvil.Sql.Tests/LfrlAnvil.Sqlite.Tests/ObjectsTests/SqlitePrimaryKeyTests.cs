using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqlitePrimaryKeyTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).SetName( "PK_TEST" );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        ISqlPrimaryKey sut = schema.Objects.GetTable( "T" ).Constraints.PrimaryKey;
        var index = schema.Objects.GetIndex( "UIX_T_C1A" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Index.Should().BeSameAs( index );
            sut.Type.Should().Be( SqlObjectType.PrimaryKey );
            sut.Name.Should().Be( "PK_TEST" );
            sut.ToString().Should().Be( "[PrimaryKey] foo_PK_TEST" );
        }
    }
}
