using System.Linq;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public partial class MySqlDatabaseBuilderTests : TestsBase
{
    [Fact]
    public void Ctor_ShouldCreateBuilderWithDefaultSchema()
    {
        var sut = MySqlDatabaseBuilderMock.Create();

        using ( new AssertionScope() )
        {
            sut.Schemas.Count.Should().Be( 1 );
            sut.Schemas.Database.Should().BeSameAs( sut );
            sut.Schemas.Should().BeSequentiallyEqualTo( sut.Schemas.Default );

            sut.Schemas.Default.Database.Should().BeSameAs( sut );
            sut.Schemas.Default.Name.Should().Be( "common" );
            sut.Schemas.Default.Objects.Should().BeEmpty();
            sut.Schemas.Default.Objects.Schema.Should().BeSameAs( sut.Schemas.Default );

            sut.Dialect.Should().BeSameAs( MySqlDialect.Instance );
            sut.ServerVersion.Should().Be( "0.0.0" );

            sut.Changes.Database.Should().BeSameAs( sut );
            sut.Changes.Mode.Should().Be( SqlDatabaseCreateMode.DryRun );
            sut.Changes.IsAttached.Should().BeTrue();
            sut.Changes.ActiveObject.Should().BeNull();
            sut.Changes.ActiveObjectExistenceState.Should().Be( default( SqlObjectExistenceState ) );
            sut.Changes.IsActive.Should().BeTrue();
            sut.Changes.GetPendingActions().ToArray().Should().BeEmpty();
        }
    }

    [Fact]
    public void AddConnectionChangeCallback_ShouldNotThrow()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        var result = sut.AddConnectionChangeCallback( _ => { } );
        result.Should().BeSameAs( sut );
    }

    [Fact]
    public void Changes_AddCreateGuidFunctionAction_ShouldAddCorrectAction()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.Changes.AddCreateGuidFunctionAction();

        var actions = sut.GetLastPendingActions( 0 );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"CREATE FUNCTION `common`.`GUID`() RETURNS BINARY(16)
BEGIN
  SET @value = UNHEX(REPLACE(UUID(), '-', ''));
  RETURN CONCAT(REVERSE(SUBSTRING(@value, 1, 4)), REVERSE(SUBSTRING(@value, 5, 2)), REVERSE(SUBSTRING(@value, 7, 2)), SUBSTRING(@value, 9));
END;
" );
        }
    }

    [Fact]
    public void Changes_AddCreateDropIndexIfExistsProcedureAction_ShouldAddCorrectAction()
    {
        var sut = MySqlDatabaseBuilderMock.Create();
        sut.Changes.AddCreateDropIndexIfExistsProcedureAction();

        var actions = sut.GetLastPendingActions( 0 );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .Be(
                    @"CREATE PROCEDURE `common`.`_DROP_INDEX_IF_EXISTS`(`schema_name` VARCHAR(128), `table_name` VARCHAR(128), `index_name` VARCHAR(128))
BEGIN
  SET @schema_name = COALESCE(`schema_name`, DATABASE());
  IF EXISTS (SELECT * FROM `information_schema`.`statistics` AS `s` WHERE `s`.`table_schema` = @schema_name AND `s`.`table_name` = `table_name` AND `s`.`index_name` = `index_name`) THEN
    SET @text = CONCAT('DROP INDEX `', `index_name`, '` ON `', @schema_name, '`.`', `table_name`, '`;');
    PREPARE stmt FROM @text;
    EXECUTE stmt;
  END IF;
END;
" );
        }
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenDatabaseIsMySql()
    {
        var action = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenDatabaseIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlDatabaseBuilder>>();
        var sut = Substitute.For<ISqlDatabaseBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
