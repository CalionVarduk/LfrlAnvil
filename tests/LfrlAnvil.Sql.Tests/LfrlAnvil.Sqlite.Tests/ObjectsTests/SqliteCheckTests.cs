using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
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
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );
        tableBuilder.Checks.Create( column.Node > SqlNode.Literal( 0 ) );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Get( "foo" ).Objects.GetTable( "T" );

        ISqlCheck sut = table.Checks.Get( "CHK_T_0" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Check );
            sut.Name.Should().Be( "CHK_T_0" );
            sut.FullName.Should().Be( "foo_CHK_T_0" );
            sut.ToString().Should().Be( "[Check] foo_CHK_T_0" );
        }
    }
}
