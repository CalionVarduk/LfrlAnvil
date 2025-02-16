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

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.View.TestRefEquals( view ),
                sut.Type.TestEquals( SqlObjectType.ViewDataField ),
                sut.Name.TestEquals( "a" ),
                sut.Node.TestRefEquals( view.Node["a"] ),
                sut.ToString().TestEquals( "[ViewDataField] foo.V.a" ) )
            .Go();
    }
}
