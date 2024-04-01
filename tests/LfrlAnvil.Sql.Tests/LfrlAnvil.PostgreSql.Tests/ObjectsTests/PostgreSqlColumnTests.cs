using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests;

public class PostgreSqlColumnTests : TestsBase
{
    [Theory]
    [InlineData( typeof( int ), true )]
    [InlineData( typeof( int ), false )]
    [InlineData( typeof( double ), true )]
    [InlineData( typeof( string ), false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(Type type, bool isNullable)
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetType( type ).MarkAsNullable( isNullable );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Column );
            sut.Name.Should().Be( "C" );
            sut.IsNullable.Should().Be( isNullable );
            sut.HasDefaultValue.Should().BeFalse();
            sut.TypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType( type ) );
            sut.ComputationStorage.Should().BeNull();
            sut.Node.Should().BeSameAs( table.Node["C"] );
            sut.ToString().Should().Be( "[Column] public.T.C" );
        }
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithDefaultValue()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetDefaultValue( SqlNode.Literal( 0 ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Column );
            sut.Name.Should().Be( "C" );
            sut.IsNullable.Should().BeFalse();
            sut.HasDefaultValue.Should().BeTrue();
            sut.TypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
            sut.ComputationStorage.Should().BeNull();
            sut.Node.Should().BeSameAs( table.Node["C"] );
            sut.ToString().Should().Be( "[Column] public.T.C" );
        }
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual )]
    [InlineData( SqlColumnComputationStorage.Stored )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithComputation(SqlColumnComputationStorage storage)
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetComputation( new SqlColumnComputation( SqlNode.Literal( 1 ), storage ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Column );
            sut.Name.Should().Be( "C" );
            sut.IsNullable.Should().BeFalse();
            sut.HasDefaultValue.Should().BeFalse();
            sut.TypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
            sut.ComputationStorage.Should().Be( SqlColumnComputationStorage.Stored );
            sut.Node.Should().BeSameAs( table.Node["C"] );
            sut.ToString().Should().Be( "[Column] public.T.C" );
        }
    }

    [Theory]
    [InlineData( SqlColumnComputationStorage.Virtual )]
    [InlineData( SqlColumnComputationStorage.Stored )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithComputationAndVirtualStorageIncluded(SqlColumnComputationStorage storage)
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock
            .Create( virtualGeneratedColumnStorageResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Default;

        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetComputation( new SqlColumnComputation( SqlNode.Literal( 1 ), storage ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        var sut = table.Columns.Get( "C" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Column );
            sut.Name.Should().Be( "C" );
            sut.IsNullable.Should().BeFalse();
            sut.HasDefaultValue.Should().BeFalse();
            sut.TypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
            sut.ComputationStorage.Should().Be( storage );
            sut.Node.Should().BeSameAs( table.Node["C"] );
            sut.ToString().Should().Be( "[Column] public.T.C" );
        }
    }
}
