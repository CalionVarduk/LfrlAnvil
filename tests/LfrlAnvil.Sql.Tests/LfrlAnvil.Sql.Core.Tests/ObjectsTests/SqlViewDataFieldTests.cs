using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlViewDataFieldTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        schemaBuilder.Objects.CreateView( "V", SqlNode.RawRecordSet( "bar" ).ToDataSource().Select( s => new[] { s.From["a"].AsSelf() } ) );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var view = schema.Objects.GetView( "V" );

        ISqlViewDataField sut = view.DataFields.Get( "a" );

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
