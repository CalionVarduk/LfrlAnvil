using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Objects.Builders;
using LfrlAnvil.Sqlite.Tests.Helpers;

namespace LfrlAnvil.Sqlite.Tests.ObjectsTests.BuildersTests;

public partial class SqliteDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = SqliteDatabaseBuilderMock.Create();

        Assertion.All(
                sut.Schemas.Count.TestEquals( 1 ),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.TestSequence( [ sut.Schemas.Default ] ),
                sut.Schemas.Default.Database.TestRefEquals( sut ),
                sut.Schemas.Default.Name.TestEmpty(),
                sut.Schemas.Default.Objects.TestEmpty(),
                sut.Schemas.Default.Objects.Schema.TestRefEquals( sut.Schemas.Default ),
                sut.Dialect.TestRefEquals( SqliteDialect.Instance ),
                sut.ServerVersion.TestEquals( "0.0.0" ),
                sut.Changes.Database.TestRefEquals( sut ),
                sut.Changes.Mode.TestEquals( SqlDatabaseCreateMode.DryRun ),
                sut.Changes.IsAttached.TestTrue(),
                sut.Changes.ActiveObject.TestNull(),
                sut.Changes.ActiveObjectExistenceState.TestEquals( default ),
                sut.Changes.IsActive.TestTrue(),
                sut.Changes.GetPendingActions().ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        var sut = SqliteDatabaseBuilderMock.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void ForSqlite_ShouldInvokeAction_WhenDatabaseIsSqlite()
    {
        var action = Substitute.For<Action<SqliteDatabaseBuilder>>();
        var sut = SqliteDatabaseBuilderMock.Create();

        var result = sut.ForSqlite( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForSqlite_ShouldNotInvokeAction_WhenDatabaseIsNotSqlite()
    {
        var action = Substitute.For<Action<SqliteDatabaseBuilder>>();
        var sut = Substitute.For<ISqlDatabaseBuilder>();

        var result = sut.ForSqlite( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
