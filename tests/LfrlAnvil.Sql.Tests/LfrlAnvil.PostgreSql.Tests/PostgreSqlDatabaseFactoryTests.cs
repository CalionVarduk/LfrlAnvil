using LfrlAnvil.PostgreSql.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Tests;

public class PostgreSqlDatabaseFactoryTests : TestsBase
{
    [Fact]
    public void RegisterPostgreSql_ShouldAddPostgreSqlFactory()
    {
        var sut = new SqlDatabaseFactoryProvider();
        var result = sut.RegisterPostgreSql();

        Assertion.All(
                result.TestRefEquals( sut ),
                result.SupportedDialects.TestSequence( [ PostgreSqlDialect.Instance ] ),
                result.GetFor( PostgreSqlDialect.Instance ).TestType().AssignableTo<PostgreSqlDatabaseFactory>() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectOptions()
    {
        var defaultNamesProvider = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var options = PostgreSqlDatabaseFactoryOptions.Default.SetDefaultNamesCreator( defaultNamesProvider );
        var sut = new PostgreSqlDatabaseFactory( options );
        sut.Options.TestEquals( options ).Go();
    }
}
