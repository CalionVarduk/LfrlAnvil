using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Tests;

public class MySqlDatabaseFactoryTests : TestsBase
{
    [Fact]
    public void RegisterMySql_ShouldAddMySqlFactory()
    {
        var sut = new SqlDatabaseFactoryProvider();
        var result = sut.RegisterMySql();

        Assertion.All(
                result.TestRefEquals( sut ),
                result.SupportedDialects.TestSequence( [ MySqlDialect.Instance ] ),
                result.GetFor( MySqlDialect.Instance ).TestType().AssignableTo<MySqlDatabaseFactory>() )
            .Go();
    }

    [Fact]
    public void Ctor_ShouldCreateWithCorrectOptions()
    {
        var defaultNamesProvider = Substitute.For<SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>>();
        var options = MySqlDatabaseFactoryOptions.Default.SetDefaultNamesCreator( defaultNamesProvider );
        var sut = new MySqlDatabaseFactory( options );
        sut.Options.TestEquals( options ).Go();
    }
}
