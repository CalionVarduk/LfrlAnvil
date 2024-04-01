﻿using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql.Tests.ObjectsTests;

public class PostgreSqlPrimaryKeyTests : TestsBase
{
    [Fact]
    public void Properties_ShouldBeCorrectlyCopiedFromBuilder()
    {
        var schemaBuilder = PostgreSqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var tableBuilder = schemaBuilder.Objects.CreateTable( "T" );
        tableBuilder.Constraints.SetPrimaryKey( tableBuilder.Columns.Create( "C1" ).Asc() ).SetName( "PK_TEST" );

        var db = PostgreSqlDatabaseMock.Create( schemaBuilder.Database );
        var schema = db.Schemas.Get( "foo" );

        var sut = schema.Objects.GetTable( "T" ).Constraints.PrimaryKey;
        var index = schema.Objects.GetIndex( "UIX_T_C1A" );

        using ( new AssertionScope() )
        {
            sut.Database.Should().BeSameAs( db );
            sut.Index.Should().BeSameAs( index );
            sut.Type.Should().Be( SqlObjectType.PrimaryKey );
            sut.Name.Should().Be( "PK_TEST" );
            sut.ToString().Should().Be( "[PrimaryKey] foo.PK_TEST" );
        }
    }
}
