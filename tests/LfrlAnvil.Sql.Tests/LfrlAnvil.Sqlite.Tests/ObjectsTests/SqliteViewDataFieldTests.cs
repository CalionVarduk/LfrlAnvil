using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteViewDataFieldTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var view = schema.Objects.GetView( "V" );

        var sut = view.DataFields.Get( "a" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.View.Should().BeSameAs( view );
            sut.Type.Should().Be( SqlObjectType.ViewDataField );
            sut.Name.Should().Be( "a" );
            sut.Node.Should().BeSameAs( view.Node["a"] );
            sut.ToString().Should().Be( "[ViewDataField] foo_V.a" );
        }
    }
}
