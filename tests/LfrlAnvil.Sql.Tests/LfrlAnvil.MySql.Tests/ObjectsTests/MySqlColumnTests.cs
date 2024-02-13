using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Tests.Helpers;

namespace LfrlAnvil.MySql.Tests.ObjectsTests;

public class MySqlColumnTests : TestsBase
{
    [Theory]
    [InlineData( typeof( int ), true )]
    [InlineData( typeof( int ), false )]
    [InlineData( typeof( double ), true )]
    [InlineData( typeof( string ), false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(Type type, bool isNullable)
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetType( type ).MarkAsNullable( isNullable );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        ISqlColumn sut = table.Columns.GetColumn( "C" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Column );
            sut.Name.Should().Be( "C" );
            sut.IsNullable.Should().Be( isNullable );
            sut.HasDefaultValue.Should().BeFalse();
            sut.TypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType( type ) );
            sut.Node.Should().BeSameAs( table.Node["C"] );
            sut.ToString().Should().Be( "[Column] common.T.C" );
        }
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_WithDefaultValue()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Default;
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Columns.Create( "C" ).SetDefaultValue( SqlNode.Literal( 0 ) );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        var table = db.Schemas.Default.Objects.GetTable( "T" );

        ISqlColumn sut = table.Columns.GetColumn( "C" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Column );
            sut.Name.Should().Be( "C" );
            sut.IsNullable.Should().BeFalse();
            sut.HasDefaultValue.Should().BeTrue();
            sut.TypeDefinition.Should().BeSameAs( db.TypeDefinitions.GetByType<object>() );
            sut.Node.Should().BeSameAs( table.Node["C"] );
            sut.ToString().Should().Be( "[Column] common.T.C" );
        }
    }
}
