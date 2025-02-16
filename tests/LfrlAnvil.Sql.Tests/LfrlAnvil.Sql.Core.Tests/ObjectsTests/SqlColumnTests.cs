using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlColumnTests : TestsBase
{
    [Theory]
    [InlineData( typeof( int ), true )]
    [InlineData( typeof( int ), false )]
    [InlineData( typeof( double ), true )]
    [InlineData( typeof( string ), false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(Type type, bool isNullable)
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetType( type ).MarkAsNullable( isNullable );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        ISqlColumn sut = table.Columns.Get( "C" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Column ),
                sut.Name.TestEquals( "C" ),
                sut.IsNullable.TestEquals( isNullable ),
                sut.HasDefaultValue.TestFalse(),
                sut.TypeDefinition.TestRefEquals( db.TypeDefinitions.GetByType( type ) ),
                sut.ComputationStorage.TestNull(),
                sut.Node.TestRefEquals( table.Node["C"] ),
                sut.ToString().TestEquals( "[Column] common.T.C" ) )
            .Go();
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithDefaultValue()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetDefaultValue( SqlNode.Literal( 0 ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        ISqlColumn sut = table.Columns.Get( "C" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Column ),
                sut.Name.TestEquals( "C" ),
                sut.IsNullable.TestFalse(),
                sut.HasDefaultValue.TestTrue(),
                sut.TypeDefinition.TestRefEquals( db.TypeDefinitions.GetByType<object>() ),
                sut.ComputationStorage.TestNull(),
                sut.Node.TestRefEquals( table.Node["C"] ),
                sut.ToString().TestEquals( "[Column] common.T.C" ) )
            .Go();
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual )]
    [InlineData( SqlColumnComputationStorage.Stored )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithComputation(SqlColumnComputationStorage storage)
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetComputation( new SqlColumnComputation( SqlNode.Literal( 1 ), storage ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        ISqlColumn sut = table.Columns.Get( "C" );

        Assertion.All(
                sut.Database.TestRefEquals( db ),
                sut.Table.TestRefEquals( table ),
                sut.Type.TestEquals( SqlObjectType.Column ),
                sut.Name.TestEquals( "C" ),
                sut.IsNullable.TestFalse(),
                sut.HasDefaultValue.TestFalse(),
                sut.TypeDefinition.TestRefEquals( db.TypeDefinitions.GetByType<object>() ),
                sut.ComputationStorage.TestEquals( storage ),
                sut.Node.TestRefEquals( table.Node["C"] ),
                sut.ToString().TestEquals( "[Column] common.T.C" ) )
            .Go();
    }

    [Fact]
    public void Asc_ShouldReturnCorrectResult()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn sut = table.Columns.Get( "C" );

        var result = sut.Asc();

        Assertion.All(
                result.Expression.TestRefEquals( sut.Node ),
                result.Ordering.TestEquals( OrderBy.Asc ) )
            .Go();
    }

    [Fact]
    public void Desc_ShouldReturnCorrectResult()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );
        ISqlColumn sut = table.Columns.Get( "C" );

        var result = sut.Desc();

        Assertion.All(
                result.Expression.TestRefEquals( sut.Node ),
                result.Ordering.TestEquals( OrderBy.Desc ) )
            .Go();
    }
}
