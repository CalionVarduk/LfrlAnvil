using System.Collections.Generic;
using System.Data;
using System.Globalization;
using LfrlAnvil.Functional;
using LfrlAnvil.Sqlite.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionTests : TestsBase
{
    [Theory]
    [InlineData( typeof( long ), "System.Int64 <=> 'INTEGER' (Integer), DefaultValue: [\"0\" : System.Int64]" )]
    [InlineData( typeof( float ), "System.Single <=> 'REAL' (Real), DefaultValue: [\"0\" : System.Single]" )]
    [InlineData( typeof( string ), "System.String <=> 'TEXT' (Text), DefaultValue: [\"\" : System.String]" )]
    [InlineData( typeof( Guid ), "System.Guid <=> 'BLOB' (Blob), DefaultValue: [\"00000000-0000-0000-0000-000000000000\" : System.Guid]" )]
    public void ToString_ShouldReturnCorrectResult(Type type, string expected)
    {
        var provider = new SqliteColumnTypeDefinitionProvider();
        var sut = provider.GetByType( type );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void Extend_ShouldReturnCorrectDefinition()
    {
        var isNullable = Fixture.Create<bool>();
        var parameter = new SqliteParameter();
        var provider = new SqliteColumnTypeDefinitionProvider();
        var sut = provider.GetByType<string>();
        var definition = sut.Extend( v => v.ToString(), s => int.Parse( s, CultureInfo.InvariantCulture ), 123 );

        var dbLiteral = definition.ToDbLiteral( 10 );
        var parameterValue = definition.ToParameterValue( 10 );
        definition.SetParameterInfo( parameter, isNullable );

        using ( new AssertionScope() )
        {
            dbLiteral.Should().Be( "'10'" );
            parameterValue.Should().Be( "10" );
            parameter.DbType.Should().Be( DbType.String );
            parameter.SqliteType.Should().Be( SqliteType.Text );
            parameter.IsNullable.Should().Be( isNullable );
        }
    }

    [Fact]
    public void ProviderShouldThrowKeyNotFoundException_WhenDefinitionDoesNotExistAndIsNotEnum()
    {
        var provider = new SqliteColumnTypeDefinitionProvider();
        var action = Lambda.Of( () => provider.GetByType<SqliteParameter>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }
}
