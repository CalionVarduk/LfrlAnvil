using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests;

public class PostgreSqlCheckTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var column = tableBuilder.Columns.Create( "C" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );
        tableBuilder.Constraints.CreateCheck( "CHK_T_0", column.Node > SqlNode.Literal( 0 ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Get( "foo" ).Objects.GetTable( "T" );

        var sut = table.Constraints.GetCheck( "CHK_T_0" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Check );
            sut.Name.Should().Be( "CHK_T_0" );
            sut.ToString().Should().Be( "[Check] foo.CHK_T_0" );
        }
    }
}
