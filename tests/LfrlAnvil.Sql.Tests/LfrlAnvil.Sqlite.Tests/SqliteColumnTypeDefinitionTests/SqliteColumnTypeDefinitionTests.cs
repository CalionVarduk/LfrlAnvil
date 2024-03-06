using System.Collections.Generic;
using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Extensions;
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
        var provider = new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );
        var sut = provider.GetByType( type );

        var result = sut.ToString();

        result.Should().Be( expected );
    }

    [Fact]
    public void ProviderShouldThrowKeyNotFoundException_WhenDefinitionDoesNotExistAndIsNotEnum()
    {
        var provider = new SqliteColumnTypeDefinitionProvider( new SqliteColumnTypeDefinitionProviderBuilder() );
        var action = Lambda.Of( () => provider.GetByType<SqliteParameter>() );
        action.Should().ThrowExactly<KeyNotFoundException>();
    }
}
