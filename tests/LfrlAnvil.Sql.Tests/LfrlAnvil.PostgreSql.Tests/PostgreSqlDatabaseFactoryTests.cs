using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDatabaseFactoryTests : TestsBase
{
    [Fact]
    public void RegisterPostgreSql_ShouldAddPostgreSqlFactory()
    {
        var sut = new SqlDatabaseFactoryProvider();
        var result = sut.RegisterPostgreSql();

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.SupportedDialects.Should().BeSequentiallyEqualTo( PostgreSqlDialect.Instance );
            result.GetFor( PostgreSqlDialect.Instance ).Should().BeOfType<PostgreSqlDatabaseFactory>();
        }
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectOptions()
    {
        var defaultNamesProvider = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var options = PostgreSqlDatabaseFactoryOptions.Default.SetDefaultNamesCreator( defaultNamesProvider );
        var sut = new PostgreSqlDatabaseFactory( options );
        sut.Options.Should().BeEquivalentTo( options );
    }
}
