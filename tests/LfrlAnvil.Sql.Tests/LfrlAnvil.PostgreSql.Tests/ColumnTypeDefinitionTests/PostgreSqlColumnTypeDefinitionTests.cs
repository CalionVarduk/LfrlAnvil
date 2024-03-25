using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Tests.ColumnTypeDefinitionTests;

public class PostgreSqlColumnTypeDefinitionTests : TestsBase
{
    [Theory]
    [InlineData( typeof( long ), "System.Int64 <=> 'INT8' (Bigint), DefaultValue: [\"0\" : System.Int64]" )]
    [InlineData( typeof( float ), "System.Single <=> 'FLOAT4' (Real), DefaultValue: [\"0\" : System.Single]" )]
    [InlineData( typeof( string ), "System.String <=> 'VARCHAR' (Varchar), DefaultValue: [\"\" : System.String]" )]
    [InlineData( typeof( char ), "System.Char <=> 'VARCHAR(1)' (Varchar), DefaultValue: [\"0\" : System.Char]" )]
    [InlineData( typeof( Guid ), "System.Guid <=> 'UUID' (Uuid), DefaultValue: [\"00000000-0000-0000-0000-000000000000\" : System.Guid]" )]
    public void ToString_ShouldReturnCorrectResult(Type type, string expected)
    {
        var provider = new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );
        var sut = provider.GetByType( type );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void ProviderShouldThrowKeyNotFoundException_WhenDefinitionDoesNotExistAndIsNotEnum()
    {
        var provider = new PostgreSqlColumnTypeDefinitionProvider( new PostgreSqlColumnTypeDefinitionProviderBuilder() );
        var action = Lambda.Of( () => provider.GetByType<NpgsqlParameter>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }
}
