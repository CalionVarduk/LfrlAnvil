using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests;

public class PostgreSqlViewDataFieldTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
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
            sut.ToString().Should().Be( "[ViewDataField] foo.V.a" );
        }
    }
}
