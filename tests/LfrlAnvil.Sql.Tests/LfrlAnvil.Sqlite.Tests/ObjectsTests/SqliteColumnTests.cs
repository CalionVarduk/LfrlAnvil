using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteColumnTests : TestsBase
{
    [Theory]
    [InlineData( typeof( int ), null, true )]
    [InlineData( typeof( int ), 10, true )]
    [InlineData( typeof( int ), null, false )]
    [InlineData( typeof( int ), 10, false )]
    [InlineData( typeof( double ), null, true )]
    [InlineData( typeof( double ), 123.5, true )]
    [InlineData( typeof( string ), null, false )]
    [InlineData( typeof( string ), "foo", false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(Type type, object? defaultValue, bool isNullable)
    {
        var schemaBuilder = new SqliteDatabaseBuilder().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var columnBuilder = tableBuilder.Columns.Create( "C" ).SetType( type ).SetDefaultValue( defaultValue ).MarkAsNullable( isNullable );
        tableBuilder.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = new SqliteDatabaseMock( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        ISqlColumn sut = table.Columns.Get( "C" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Column );
            sut.Name.Should().Be( "C" );
            sut.FullName.Should().Be( "T.C" );
            sut.IsNullable.Should().Be( isNullable );
            sut.TypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType( type ) );
            sut.DefaultValue.Should().BeSameAs( columnBuilder.DefaultValue );
            sut.ToString().Should().Be( "[Column] T.C" );
        }
    }
}
