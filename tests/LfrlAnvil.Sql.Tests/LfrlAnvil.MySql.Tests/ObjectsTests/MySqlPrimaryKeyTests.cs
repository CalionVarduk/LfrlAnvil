using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Tests.Helpers;

namespace LfrlAnvil.MySql.Tests.ObjectsTests;

public class MySqlPrimaryKeyTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).SetName( "PK_TEST" );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.GetSchema( "foo" );

        ISqlPrimaryKey sut = schema.Objects.GetTable( "T" ).Constraints.PrimaryKey;
        var index = schema.Objects.GetIndex( "UIX_T_C1A" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Index.Should().BeSameAs( index );
            sut.Type.Should().Be( SqlObjectType.PrimaryKey );
            sut.Name.Should().Be( "PK_TEST" );
            sut.ToString().Should().Be( "[PrimaryKey] foo.PK_TEST" );
        }
    }
}
