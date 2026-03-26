using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests;

public class SqliteColumnTests : TestsBase
{
    [Theory]
    [InlineData( typeof( int ), true )]
    [InlineData( typeof( int ), false )]
    [InlineData( typeof( double ), true )]
    [InlineData( typeof( string ), false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(Type type, bool isNullable)
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetType( type ).MarkAsNullable( isNullable );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Column ),
                sut.Name.TestEquals( "C" ),
                sut.IsNullable.TestEquals( isNullable ),
                sut.HasDefaultValue.TestFalse(),
                sut.TypeDefinition.TestRefEquals( db.TypeDefinitions.GetByType( type ) ),
                sut.ComputationStorage.TestNull(),
                sut.Identity.TestNull(),
                sut.Node.TestRefEquals( table.Node["C"] ),
                sut.ToString().TestEquals( "[Column] T.C" ) )
            .Go();
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithDefaultValue()
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetDefaultValue( SqlNode.Literal( 0 ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Column ),
                sut.Name.TestEquals( "C" ),
                sut.IsNullable.TestFalse(),
                sut.HasDefaultValue.TestTrue(),
                sut.TypeDefinition.TestRefEquals( db.TypeDefinitions.GetByType<object>() ),
                sut.ComputationStorage.TestNull(),
                sut.Identity.TestNull(),
                sut.Node.TestRefEquals( table.Node["C"] ),
                sut.ToString().TestEquals( "[Column] T.C" ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual )]
    [InlineData( SqlColumnComputationStorage.Stored )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithComputation(SqlColumnComputationStorage storage)
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetComputation( new SqlColumnComputation( SqlNode.Literal( 1 ), storage ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Column ),
                sut.Name.TestEquals( "C" ),
                sut.IsNullable.TestFalse(),
                sut.HasDefaultValue.TestFalse(),
                sut.TypeDefinition.TestRefEquals( db.TypeDefinitions.GetByType<object>() ),
                sut.ComputationStorage.TestEquals( storage ),
                sut.Identity.TestNull(),
                sut.Node.TestRefEquals( table.Node["C"] ),
                sut.ToString().TestEquals( "[Column] T.C" ) )
            .Go();
    }

    [Theory]
    [InlineData( null )]
    [InlineData( 123 )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithIdentity(int? cache)
    {
        var schemaBuilder = SqliteDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetIdentity( new SqlColumnIdentity( cache ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Get( "C" ).Asc() );

        var db = SqliteDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Column ),
                sut.Name.TestEquals( "C" ),
                sut.IsNullable.TestFalse(),
                sut.HasDefaultValue.TestFalse(),
                sut.TypeDefinition.TestRefEquals( db.TypeDefinitions.GetByType<object>() ),
                sut.ComputationStorage.TestNull(),
                sut.Identity.TestEquals( SqlColumnIdentity.Default ),
                sut.Node.TestRefEquals( table.Node["C"] ),
                sut.ToString().TestEquals( "[Column] T.C" ) )
            .Go();
    }
}
