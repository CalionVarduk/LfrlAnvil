using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.MySqlColumnTypeDefinitionTests;

public class MySqlColumnTypeDefinitionTests : TestsBase
{
    [Theory]
    [InlineData( typeof( long ), "System.Int64 <=> 'BIGINT' (Int64), DefaultValue: [\"0\" : System.Int64]" )]
    [InlineData( typeof( float ), "System.Single <=> 'FLOAT' (Float), DefaultValue: [\"0\" : System.Single]" )]
    [InlineData( typeof( string ), "System.String <=> 'LONGTEXT' (LongText), DefaultValue: [\"\" : System.String]" )]
    [InlineData( typeof( char ), "System.Char <=> 'CHAR(1)' (String), DefaultValue: [\"0\" : System.Char]" )]
    [InlineData(
        typeof( Guid ),
        "System.Guid <=> 'BINARY(16)' (Binary), DefaultValue: [\"00000000-0000-0000-0000-000000000000\" : System.Guid]" )]
    public void ToString_ShouldReturnCorrectResult(Type type, string expected)
    {
        var provider = new MySqlColumnTypeDefinitionProvider( new MySqlDataTypeProvider() );
        var sut = provider.GetByType( type );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void ProviderShouldThrowKeyNotFoundException_WhenDefinitionDoesNotExistAndIsNotEnum()
    {
        var provider = new MySqlColumnTypeDefinitionProvider( new MySqlDataTypeProvider() );
        var action = Lambda.Of( () => provider.GetByType<MySqlParameter>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }
}
