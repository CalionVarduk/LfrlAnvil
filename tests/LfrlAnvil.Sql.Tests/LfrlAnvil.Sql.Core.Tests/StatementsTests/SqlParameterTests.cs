using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterTests : TestsBase
{
    [Fact]
    public void Named_ShouldCreateNamedParameter()
    {
        var result = SqlParameter.Named( "foo", 1 );
        using ( new AssertionScope() )
        {
            result.Name.Should().Be( "foo" );
            result.Value.Should().Be( 1 );
            result.IsPositional.Should().BeFalse();
        }
    }

    [Fact]
    public void Positional_ShouldCreatePositionalParameter()
    {
        var result = SqlParameter.Positional( "foo" );
        using ( new AssertionScope() )
        {
            result.Name.Should().BeNull();
            result.Value.Should().Be( "foo" );
            result.IsPositional.Should().BeTrue();
        }
    }
}
