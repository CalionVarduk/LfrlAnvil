using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDatabaseFactoryTests : TestsBase
{
    [Fact]
    public void RegisterMySql_ShouldAddMySqlFactory()
    {
        var sut = new SqlDatabaseFactoryProvider();
        var result = sut.RegisterMySql();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.SupportedDialects.Should().BeSequentiallyEqualTo( MySqlDialect.Instance );
            result.GetFor( MySqlDialect.Instance ).Should().BeOfType<MySqlDatabaseFactory>();
        }
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectOptions()
    {
        var defaultNamesProvider = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var options = MySqlDatabaseFactoryOptions.Default.SetDefaultNamesCreator( defaultNamesProvider );
        var sut = new MySqlDatabaseFactory( options );
        sut.Options.Should().BeEquivalentTo( options );
    }
}
