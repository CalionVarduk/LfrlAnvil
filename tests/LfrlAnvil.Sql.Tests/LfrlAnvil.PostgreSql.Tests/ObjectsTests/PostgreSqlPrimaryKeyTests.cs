using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests;

public class PostgreSqlPrimaryKeyTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).SetName( "PK_TEST" );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        var sut = schema.Objects.GetTable( "T" ).Constraints.PrimaryKey;
        var index = schema.Objects.GetIndex( "UIX_T_C1A" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Index.TestRefEquals( index ),
                sut.Type.TestEquals( SqlObjectType.PrimaryKey ),
                sut.Name.TestEquals( "PK_TEST" ),
                sut.ToString().TestEquals( "[PrimaryKey] foo.PK_TEST" ) )
            .Go();
    }
}
