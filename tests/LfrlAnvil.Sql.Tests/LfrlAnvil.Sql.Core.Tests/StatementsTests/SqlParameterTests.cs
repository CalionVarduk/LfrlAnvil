using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Tests.StatementsTests;

public class SqlParameterTests : TestsBase
{
    [Fact]
    public void Named_ShouldCreateNamedParameter()
    {
        var result = SqlParameter.Named( "foo", 1 );
        Assertion.All(
                result.Name.TestEquals( "foo" ),
                result.Value.TestEquals( 1 ),
                result.IsPositional.TestFalse() )
            .Go();
    }

    [Fact]
    public void Positional_ShouldCreatePositionalParameter()
    {
        var result = SqlParameter.Positional( "foo" );
        Assertion.All(
                result.Name.TestNull(),
                result.Value.TestEquals( "foo" ),
                result.IsPositional.TestTrue() )
            .Go();
    }
}
