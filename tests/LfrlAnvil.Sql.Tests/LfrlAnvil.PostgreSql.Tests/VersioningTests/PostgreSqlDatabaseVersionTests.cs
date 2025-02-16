using LfrlAnvil.Functional;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.PostgreSql.Tests.Helpers;
using LfrlAnvil.PostgreSql.Versioning;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.PostgreSql.Tests.VersioningTests;

public class PostgreSqlDatabaseVersionTests : TestsBase
{
    [Fact]
    public void Create_ShouldReturnCorrectResult()
    {
        var version = Version.Parse( "1.2.3.4" );
        var description = Fixture.Create<string>();
        var apply = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var builder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseVersion.Create( version, description, apply );

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
        var apply = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var builder = PostgreSqlDatabaseBuilderMock.Create();
        var sut = PostgreSqlDatabaseVersion.Create( version, apply );

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
        var apply = Substitute.For<Action<PostgreSqlDatabaseBuilder>>();
        var builder = Substitute.For<ISqlDatabaseBuilder>();
        var sut = PostgreSqlDatabaseVersion.Create( version, apply );

        var action = Lambda.Of( () => (( ISqlDatabaseVersion )sut).Apply( builder ) );

        action.Test( exc => exc.TestType().Exact<SqlObjectCastException>() ).Go();
    }
}
