using System.Data;
using LfrlAnvil.Sqlite.Extensions;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.SqliteColumnTypeDefinitionTests;

public class SqliteColumnTypeDefinitionTests : TestsBase
{
    [Theory]
    [InlineData( typeof( long ), "System.Int64 <=> 'INTEGER' (Integer), DefaultValue: [0]" )]
    [InlineData( typeof( float ), "System.Single <=> 'REAL' (Real), DefaultValue: [0]" )]
    [InlineData( typeof( string ), "System.String <=> 'TEXT' (Text), DefaultValue: []" )]
    [InlineData( typeof( Guid ), "System.Guid <=> 'BLOB' (Blob), DefaultValue: [00000000-0000-0000-0000-000000000000]" )]
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
        var parameter = new SqliteParameter();
        var provider = new SqliteColumnTypeDefinitionProvider();
        var sut = provider.GetByType<string>();
        var definition = sut.Extend( v => v.ToString(), 123 );

        var dbLiteral = definition.ToDbLiteral( 10 );
        definition.SetParameter( parameter, 10 );

        using ( new AssertionScope() )
        {
            dbLiteral.Should().Be( "'10'" );
            parameter.DbType.Should().Be( DbType.String );
            parameter.Value.Should().Be( "10" );
        }
    }
}
