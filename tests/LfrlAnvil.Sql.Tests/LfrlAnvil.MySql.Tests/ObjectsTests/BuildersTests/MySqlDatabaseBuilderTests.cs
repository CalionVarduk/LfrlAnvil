using System.Linq;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = MySqlDatabaseBuilderMock.Create();

        Assertion.All(
                sut.Schemas.Count.TestEquals( 1 ),
                sut.Schemas.Database.TestRefEquals( sut ),
                sut.Schemas.TestSequence( [ sut.Schemas.Default ] ),
                sut.Schemas.Default.Database.TestRefEquals( sut ),
                sut.Schemas.Default.Name.TestEquals( "common" ),
                sut.Schemas.Default.Objects.TestEmpty(),
                sut.Schemas.Default.Objects.Schema.TestRefEquals( sut.Schemas.Default ),
                sut.Dialect.TestRefEquals( MySqlDialect.Instance ),
                sut.ServerVersion.TestEquals( "0.0.0" ),
                sut.Changes.Database.TestRefEquals( sut ),
                sut.Changes.Mode.TestEquals( SqlDatabaseCreateMode.DryRun ),
                sut.Changes.IsAttached.TestTrue(),
                sut.Changes.ActiveObject.TestNull(),
                sut.Changes.ActiveObjectExistenceState.TestEquals( default( SqlObjectExistenceState ) ),
                sut.Changes.IsActive.TestTrue(),
                sut.Changes.GetPendingActions().ToArray().TestEmpty() )
            .Go();
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.TestRefEquals( sut ).Go();
    }

    [Fact]
    public void Changes_AddCreateGuidFunctionAction_ShouldAddCorrectAction()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.Changes.AddCreateGuidFunctionAction();

        var actions = sut.GetLastPendingActions( 0 );

        actions.Select( a => a.Sql )
            .TestSequence(
            [
                """
                CREATE FUNCTION `common`.`GUID`() RETURNS BINARY(16)
                BEGIN
                  SET @value = UNHEX(REPLACE(UUID(), '-', ''));
                  RETURN CONCAT(REVERSE(SUBSTRING(@value, 1, 4)), REVERSE(SUBSTRING(@value, 5, 2)), REVERSE(SUBSTRING(@value, 7, 2)), SUBSTRING(@value, 9));
                END;

                """
            ] )
            .Go();
    }

    [Fact]
    public void Changes_AddCreateDropIndexIfExistsProcedureAction_ShouldAddCorrectAction()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.Changes.AddCreateDropIndexIfExistsProcedureAction();

        var actions = sut.GetLastPendingActions( 0 );

        actions.Select( a => a.Sql )
            .TestSequence(
            [
                """
                CREATE PROCEDURE `common`.`_DROP_INDEX_IF_EXISTS`(`schema_name` VARCHAR(128), `table_name` VARCHAR(128), `index_name` VARCHAR(128))
                BEGIN
                  SET @schema_name = COALESCE(`schema_name`, DATABASE());
                  IF EXISTS (SELECT * FROM `information_schema`.`statistics` AS `s` WHERE `s`.`table_schema` = @schema_name AND `s`.`table_name` = `table_name` AND `s`.`index_name` = `index_name`) THEN
                    SET @text = CONCAT('DROP INDEX `', `index_name`, '` ON `', @schema_name, '`.`', `table_name`, '`;');
                    PREPARE stmt FROM @text;
                    EXECUTE stmt;
                  END IF;
                END;

                """
            ] )
            .Go();
    }

    [Fact]
    public void Helpers_ExtractConnectionStringEntries_ShouldReturnCorrectResult()
    {
        var connectionString = new MySqlConnectionStringBuilder(
            "Server=localhost;Port=3306;Database=tests;UserID=admin;Password=password;GuidFormat=None;AllowUserVariables=true;NoBackslashEscapes=true" );

        var result = MySqlHelpers.ExtractConnectionStringEntries( connectionString );

        result.TestSequence(
            [
                new SqlConnectionStringEntry( "Server", "localhost", false ), new SqlConnectionStringEntry( "Port", "3306", false ),
                new SqlConnectionStringEntry( "Database", "tests", true ), new SqlConnectionStringEntry( "User ID", "admin", true ),
                new SqlConnectionStringEntry( "Password", "password", true ), new SqlConnectionStringEntry( "GUID Format", "None", false ),
                new SqlConnectionStringEntry( "Allow User Variables", "True", false ),
                new SqlConnectionStringEntry( "No Backslash Escapes", "True", false )
            ] )
            .Go();
    }

    [Fact]
    public void Helpers_ExtendConnectionString_ShouldReturnCorrectResult()
    {
        var connectionString = new MySqlConnectionStringBuilder(
            "Server=localhost;Port=3306;Database=tests;UserID=admin;Password=password;GuidFormat=None;AllowUserVariables=true;NoBackslashEscapes=true" );

        var entries = MySqlHelpers.ExtractConnectionStringEntries( connectionString );
        var result = MySqlHelpers.ExtendConnectionString(
            entries,
            "Port=3307;Database=tests2;UserID=tester;Password=pwd;AllowUserVariables=false" );

        result.TestEquals(
                "Server=localhost;Port=3306;User ID=tester;Password=pwd;Database=tests2;Allow User Variables=True;GUID Format=None;No Backslash Escapes=True" )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenDatabaseIsMySql()
    {
        var action = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallAt( 0 ).Arguments.TestSequence( [ sut ] ) )
            .Go();
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenDatabaseIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var sut = Substitute.For<ISqlDatabaseBuilder>();

        var result = sut.ForMySql( action );

        Assertion.All(
                result.TestRefEquals( sut ),
                action.CallCount().TestEquals( 0 ) )
            .Go();
    }
}
