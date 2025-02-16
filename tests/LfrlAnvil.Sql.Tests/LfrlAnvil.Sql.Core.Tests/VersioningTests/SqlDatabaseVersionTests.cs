using LfrlAnvil.Functional;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.Sql.Mocks;

namespace LfrlAnvil.Sql.Tests.VersioningTests;

public class SqlDatabaseVersionTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = Fixture.Create<string>();
        var apply = Substitute.For<Action<SqlDatabaseBuilder>>();
        var builder = SqlDatabaseBuilderMock.Create();
        var sut = SqlDatabaseVersion.Create( version, description, apply );

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
        var apply = Substitute.For<Action<SqlDatabaseBuilder>>();
        var builder = SqlDatabaseBuilderMock.Create();
        var sut = SqlDatabaseVersion.Create( version, apply );

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
        var apply = Substitute.For<Action<SqlDatabaseBuilder>>();
        var builder = Substitute.For<ISqlDatabaseBuilder>();
        var sut = SqlDatabaseVersion.Create( version, apply );

        var action = Lambda.Of( () => (( ISqlDatabaseVersion )sut).Apply( builder ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenVersionHasDescription()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = "SQL database version";
        var apply = Substitute.For<Action<ISqlDatabaseBuilder>>();
        var sut = SqlDatabaseVersion.Create( version, description, apply );

        var result = sut.ToString();

        result.TestEquals( "1.2.3.4 (SQL database version)" ).Go();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectResult_WhenVersionDoesNotHaveDescription()
    {
        var version = Version.Parse( "1.2.3.4" );
        var apply = Substitute.For<Action<ISqlDatabaseBuilder>>();
        var sut = SqlDatabaseVersion.Create( version, apply );

        var result = sut.ToString();

        result.TestEquals( "1.2.3.4" ).Go();
    }
}
