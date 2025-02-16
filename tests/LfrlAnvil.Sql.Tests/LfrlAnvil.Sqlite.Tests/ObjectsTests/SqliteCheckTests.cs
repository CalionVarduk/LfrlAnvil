using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteCheckTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var column = tableBuilder.Columns.Create( "C" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );
        tableBuilder.Constraints.CreateCheck( "CHK_T_0", column.Node > SqlNode.Literal( 0 ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Get( "foo" ).Objects.GetTable( "T" );

        var sut = table.Constraints.GetCheck( "CHK_T_0" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Check ),
                sut.Name.TestEquals( "CHK_T_0" ),
                sut.ToString().TestEquals( "[Check] foo_CHK_T_0" ) )
            .Go();
    }
}
