﻿using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests;

public class MySqlIndexTests : TestsBase
{
    [Theory]
    [InlineData( true, false )]
    [InlineData( false, true )]
    [InlineData( false, false )]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder(bool isUnique, bool isVirtual)
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        var c2Builder = tableBuilder.Columns.Create( "C2" );

        tableBuilder.Constraints.CreateIndex( c1Builder.Asc(), c2Builder.Desc() )
            .SetName( "IX_TEST" )
            .MarkAsUnique( isUnique )
            .MarkAsVirtual( isVirtual );

        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );
        var c2 = table.Columns.Get( "C2" );

        var sut = schema.Objects.GetIndex( "IX_TEST" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Index );
            sut.Name.Should().Be( "IX_TEST" );
            sut.IsUnique.Should().Be( isUnique );
            sut.IsVirtual.Should().Be( isVirtual );
            sut.IsPartial.Should().BeFalse();
            sut.Columns.Should()
                .BeSequentiallyEqualTo( new SqlIndexed<MySqlColumn>( c1, OrderBy.Asc ), new SqlIndexed<MySqlColumn>( c2, OrderBy.Desc ) );

            sut.ToString().Should().Be( "[Index] foo.IX_TEST" );
        }
    }

    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder_ForPartialIndex()
    {
        var schemaBuilder = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        var c1Builder = tableBuilder.Columns.Create( "C1" );
        tableBuilder.Constraints.CreateIndex( c1Builder.Asc() ).SetName( "IX_TEST" ).SetFilter( SqlNode.True() );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "X" ).Asc() );

        var db = MySqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );
        var table = schema.Objects.GetTable( "T" );
        var c1 = table.Columns.Get( "C1" );

        var sut = schema.Objects.GetIndex( "IX_TEST" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Table.Should().BeSameAs( table );
            sut.Type.Should().Be( SqlObjectType.Index );
            sut.Name.Should().Be( "IX_TEST" );
            sut.IsUnique.Should().BeFalse();
            sut.IsVirtual.Should().BeFalse();
            sut.IsPartial.Should().BeTrue();
            sut.Columns.Should().BeSequentiallyEqualTo( new SqlIndexed<MySqlColumn>( c1, OrderBy.Asc ) );
            sut.ToString().Should().Be( "[Index] foo.IX_TEST" );
        }
    }
}
