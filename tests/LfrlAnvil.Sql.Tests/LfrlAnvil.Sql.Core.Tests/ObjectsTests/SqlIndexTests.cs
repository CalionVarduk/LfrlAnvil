using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.ObjectsTests;

public class SqlIndexTests : TestsBase
{
    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(bool isUnique)
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        var c2Builder = tableBuilder.Columns.Create( "C2" );
        tableBuilder.Constraints.CreateIndex( c1Builder.Asc(), c2Builder.Desc() ).SetName( "IX_TEST" ).MarkAsUnique( isUnique );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );
        var c2 = table.Columns.Get( "C2" );

        ISqlIndex sut = schema.Objects.GetIndex( "IX_TEST" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Index );
            sut.Name.Should().Be( "IX_TEST" );
            sut.IsUnique.Should().Be( isUnique );
            sut.IsPartial.Should().BeFalse();
            ((SqlIndex)sut).Columns.Should().BeSequentiallyEqualTo( c1.Asc(), c2.Desc() );
            sut.Columns.Should().BeSequentiallyEqualTo( c1.Asc(), c2.Desc() );
            sut.ToString().Should().Be( "[Index] foo.IX_TEST" );
        }
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_ForPartialIndex()
    {
        var schemaBuilder = SqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.CreateIndex( c1Builder.Asc() ).SetName( "IX_TEST" ).SetFilter( SqlNode.True() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = SqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );

        ISqlIndex sut = schema.Objects.GetIndex( "IX_TEST" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Index );
            sut.Name.Should().Be( "IX_TEST" );
            sut.IsUnique.Should().BeFalse();
            sut.IsPartial.Should().BeTrue();
            sut.Columns.Should().BeSequentiallyEqualTo( c1.Asc() );
            sut.ToString().Should().Be( "[Index] foo.IX_TEST" );
        }
    }
}
