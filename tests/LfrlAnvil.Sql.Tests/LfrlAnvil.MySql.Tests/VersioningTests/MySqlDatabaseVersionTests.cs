using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.MySql.Versioning;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.MySql.Tests.VersioningTests;

public class MySqlDatabaseVersionTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = Fixture.Create<string>();
        var apply = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var builder = MySqlDatabaseBuilderMock.Create();
        var sut = MySqlDatabaseVersion.Create( version, description, apply );

        sut.Apply( builder );

        Assertion.All(
                sut.Value.TestRefEquals( version ),
                sut.Description.TestEquals( description ),
                apply.CallAt( 0 ).Arguments.TestSequence( [ builder ] ) )
            .Go();
    }

    [Fact]
    public void Create_WithoutDescription_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var apply = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var builder = MySqlDatabaseBuilderMock.Create();
        var sut = MySqlDatabaseVersion.Create( version, apply );

        sut.Apply( builder );

        Assertion.All(
                sut.Value.TestRefEquals( version ),
                sut.Description.TestEmpty(),
                apply.CallAt( 0 ).Arguments.TestSequence( [ builder ] ) )
            .Go();
    }

    [Fact]
    public void Apply_ShouldThrowSqlObjectCastException_WhenDatabaseBuilderIsOfInvalidType()
    {
        var version = Version.Parse( "1.2.3.4" );
        var apply = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var builder = Substitute.For<ISqlDatabaseBuilder>();
        var sut = MySqlDatabaseVersion.Create( version, apply );

        var action = Lambda.Of( () => (( ISqlDatabaseVersion )sut).Apply( builder ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }
}
