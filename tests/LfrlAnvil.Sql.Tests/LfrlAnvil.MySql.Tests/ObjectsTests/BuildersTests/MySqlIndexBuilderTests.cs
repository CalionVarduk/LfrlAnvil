using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlIndexBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).SetName( "bar" );

        var result = sut.ToString();

        result.Should().Be( "[Index] foo.bar" );
    }

    [Fact]
    public void Creation_ShouldNotMarkTableForAlteration()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        var ixc2 = c2.Asc();

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( ixc2 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().MatchRegex( "IX_T_C2A" );
            sut.Columns.Expressions.Should().BeSequentiallyEqualTo( ixc2 );
            sut.ReferencedColumns.Should().BeSequentiallyEqualTo( c2 );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void Creation_ShouldNotMarkTableForAlteration_WhenIndexContainsExpressions()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        var c3 = table.Columns.Create( "C3" ).SetType<int>();
        var ixc1 = c2.Asc();
        var ixc2 = (c3.Node + SqlNode.Literal( 1 )).Desc();

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( ixc1, ixc2 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().MatchRegex( "IX_T_C2A_E1D" );
            sut.Columns.Expressions.Should().BeSequentiallyEqualTo( ixc1, ixc2 );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql( "CREATE INDEX `IX_T_C2A_E1D` ON `foo`.`T` (`C2` ASC, (`C3` + 1) DESC);" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemoval_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateIndex( c2.Asc() );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Creation_ShouldNotCreateIndex_WhenIndexIsAttachedToPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" ).SetType<int>();
        var ixc2 = c2.Asc();

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Constraints.CreateUniqueIndex( ixc2 );
        table.Constraints.SetPrimaryKey( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeSameAs( sut );
            schema.Objects.TryGet( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().MatchRegex( "UIX_T_C2A" );
            sut.Columns.Expressions.Should().BeSequentiallyEqualTo( ixc2 );
            sut.ReferencedColumns.Should().BeSequentiallyEqualTo( c2 );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `PK_T` PRIMARY KEY (`C2` ASC);" );
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( sut.Name );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetName( oldName );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `IX_T_C2A` TO `bar`;" );
        }
    }

    [Fact]
    public void SetName_ShouldUpdateNameAndNotRecreateOriginatingForeignKeys()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `IX_T_C2A` TO `bar`;" );
        }
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( " " )]
    [InlineData( "`" )]
    [InlineData( "'" )]
    [InlineData( "f`oo" )]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNameIsInvalid(string name)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var action = Lambda.Of( () => sut.SetName( "T" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenNameChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldDoNothing_WhenIndexIsAssignedToPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index.SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "UIX_T_C1A" );
            table.Constraints.TryGet( "UIX_T_C1A" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "UIX_T_C1A" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "IX_T_C2A" );
            table.Constraints.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "IX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `bar` TO `IX_T_C2A`;" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldUpdateName_WhenNewNameIsDifferentFromOldNameAndIndexIsUnique()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" ).MarkAsUnique();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultName();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "UIX_T_C2A" );
            table.Constraints.TryGet( "UIX_T_C2A" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( "bar" ).Should().BeNull();
            schema.Objects.TryGet( "UIX_T_C2A" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( "bar" ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      RENAME INDEX `bar` TO `UIX_T_C2A`;" );
        }
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInSchemaObjects()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetName( "bar" );
        pk.SetName( "IX_T_C2A" );

        var action = Lambda.Of( () => sut.SetDefaultName() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsUnique_ShouldDoNothing_WhenUniquenessFlagDoesNotChange(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsUnique();
        var result = sut.MarkAsUnique( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUnique_WhenValueChangesToFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsUnique();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldRecreateOriginatingForeignKeys_WhenValueChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexUniquenessChangesToFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.MarkAsUnique( false ) );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenVirtualIndexUniquenessChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).MarkAsVirtual();

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenIndexWithExpressionsUniquenessChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( (table.Columns.Create( "C2" ).Node + SqlNode.Literal( 1 )).Asc() );

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenUniquenessChangesToFalseAndIndexIsReferencedByForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => sut.MarkAsUnique( false ) );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void MarkAsUnique_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsUnique() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsUnique_ShouldUpdateIsUniqueAndNameCorrectly_WhenIsUniqueAndNameChangeAtTheSameTime()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique().SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeTrue();
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `bar` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsVirtual_ShouldDoNothing_WhenVirtualityFlagDoesNotChange(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsVirtual( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsVirtual();
        var result = sut.MarkAsVirtual( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldUpdateIsVirtual_WhenValueChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsVirtual.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP INDEX `IX_T_C2A` ON `foo`.`T`;" );
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldUpdateIsVirtual_WhenValueChangesToFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsVirtual();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsVirtual.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldRecreateOriginatingForeignKeys_WhenValueChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsVirtual.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldRecreateOriginatingForeignKeys_WhenValueChangesToFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsVirtual();
        table.Constraints.CreateForeignKey( sut, pk.Index );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsVirtual( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsVirtual.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexVirtualityChangesToFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.MarkAsVirtual( false ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenPartialIndexVirtualityChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).SetFilter( SqlNode.True() );

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenUniqueIndexVirtualityChangesToTrue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() ).MarkAsUnique();

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenVirtualityChangesToTrueAndIndexIsReferencedByForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( table.Constraints.CreateIndex( table.Columns.Create( "C3" ).Asc() ), sut );

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 2 );
    }

    [Fact]
    public void MarkAsVirtual_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsVirtual() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueDoesNotChange()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).SetFilter( SqlNode.True() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( SqlNode.True() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void SetFilter_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetFilter( SqlNode.True() );
        var result = sut.SetFilter( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().HaveCount( 0 );
        }
    }

    [Fact]
    public void SetFilter_ShouldIgnoreChange_WhenValueChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( t => t["C2"] != null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeNull();
            result.ReferencedFilterColumns.Should().BeEmpty();

            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo( SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ) );

            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenValueChangesAndIndexFiltersAreForbidden()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Forbid )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() );

        var action = Lambda.Of( () => sut.SetFilter( t => t["C2"] != null ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNonNullAndIndexFiltersAreIncluded()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( t => t["C2"] != null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeEquivalentTo( table.ToRecordSet().GetField( "C2" ) != null );
            result.ReferencedFilterColumns.Should().BeSequentiallyEqualTo( column );

            column.ReferencingObjects.Should().HaveCount( 2 );
            column.ReferencingObjects.Should()
                .BeEquivalentTo(
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ),
                    SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut, property: "Filter" ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC) WHERE (`C2` IS NOT NULL);" );
        }
    }

    [Fact]
    public void SetFilter_ShouldUpdateFilterAndFilterColumns_WhenValueChangesToNullAndIndexFiltersAreIncluded()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var column = table.Columns.Create( "C2" ).SetType<int>();
        var sut = table.Constraints.CreateIndex( column.Asc() ).SetFilter( t => t["C2"] != null );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeNull();
            result.ReferencedFilterColumns.Should().BeEmpty();

            column.ReferencingObjects.Should()
                .BeSequentiallyEqualTo( SqlObjectBuilderReference.Create( SqlObjectBuilderReferenceSource.Create( sut ), column ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC);" );
        }
    }

    [Theory]
    [InlineData( SqlOptionalFunctionalityResolution.Include )]
    [InlineData( SqlOptionalFunctionalityResolution.Forbid )]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenFilterIsInvalid(SqlOptionalFunctionalityResolution indexFilterResolution)
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution ).Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C" ).Asc() );

        var action = Lambda.Of( () => sut.SetFilter( _ => SqlNode.WindowFunctions.RowNumber() == SqlNode.Literal( 0 ) ) );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenPrimaryKeyIndexFilterChangesToNonNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C" ).Asc() ).Index;

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenReferencedIndexFilterChangesToNonNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsUnique();
        table.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenIndexIsVirtual()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() ).MarkAsVirtual();

        var action = Lambda.Of( () => sut.SetFilter( SqlNode.True() ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldThrowSqlObjectBuilderException_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetFilter( null ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetFilter_ShouldRecreateOriginatingForeignKeys_WhenValueChangesAndIndexFiltersAreIncluded()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        var pk = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        table.Constraints.CreateForeignKey( sut, pk.Index );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetFilter( SqlNode.True() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.Filter.Should().BeSameAs( SqlNode.True() );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C2_REF_T`;",
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE INDEX `IX_T_C2A` ON `foo`.`T` (`C2` ASC) WHERE TRUE;",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C2_REF_T` FOREIGN KEY (`C2`) REFERENCES `foo`.`T` (`C1`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void
        SetFilter_ShouldUpdateFilterAndIsUniqueAndNameCorrectly_WhenFilterAndIsUniqueAndNameChangeAtTheSameTimeAndIndexFiltersAreIncluded()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );
        var oldName = sut.Name;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsUnique().SetFilter( t => t["C2"] != null ).SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsUnique.Should().BeTrue();
            sut.Filter.Should().BeEquivalentTo( table.ToRecordSet().GetField( "C2" ) != null );
            sut.Name.Should().Be( "bar" );
            table.Constraints.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Constraints.TryGet( oldName ).Should().BeNull();
            schema.Objects.TryGet( "bar" ).Should().BeSameAs( sut );
            schema.Objects.TryGet( oldName ).Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `IX_T_C2A` ON `foo`.`T`;",
                    "CREATE UNIQUE INDEX `bar` ON `foo`.`T` (`C2` ASC) WHERE (`C2` IS NOT NULL);" );
        }
    }

    [Fact]
    public void PrimaryKeyAssignment_ShouldRecreateOriginatingForeignKeys()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var ix = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() ).MarkAsUnique();
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C3" ).SetType<int>().Asc() ).MarkAsUnique();
        var ix2 = table.Constraints.CreateIndex( table.Columns.Create( "C4" ).SetType<int>().Asc() );
        table.Constraints.CreateForeignKey( sut, ix );
        table.Constraints.CreateForeignKey( ix2, sut );

        var actionCount = schema.Database.GetPendingActionCount();
        table.Constraints.SetPrimaryKey( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP FOREIGN KEY `FK_T_C3_REF_T`;",
                    "DROP INDEX `IX_T_C3A` ON `foo`.`T`;",
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `PK_T` PRIMARY KEY (`C3` ASC);",
                    @"ALTER TABLE `foo`.`T`
                      ADD CONSTRAINT `FK_T_C3_REF_T` FOREIGN KEY (`C3`) REFERENCES `foo`.`T` (`C2`) ON DELETE RESTRICT ON UPDATE RESTRICT;" );
        }
    }

    [Fact]
    public void PrimaryKeyAssignment_ShouldDropIndexByItsOldName_WhenIndexNameAlsoChanges()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).SetType<int>().Asc() );
        var sut = table.Constraints.CreateUniqueIndex( table.Columns.Create( "C2" ).SetType<int>().Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" );
        table.Constraints.SetPrimaryKey( sut );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    "DROP INDEX `UIX_T_C2A` ON `foo`.`T`;",
                    @"ALTER TABLE `foo`.`T`
                      DROP PRIMARY KEY,
                      ADD CONSTRAINT `PK_T` PRIMARY KEY (`C2` ASC);" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndClearReferencedColumnsAndReferencedFilterColumns()
    {
        var schema = MySqlDatabaseBuilderMock.Create( indexFilterResolution: SqlOptionalFunctionalityResolution.Include )
            .Schemas.Create( "foo" );

        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var c2 = table.Columns.Create( "C2" );
        var sut = table.Constraints.CreateIndex( c2.Asc() ).SetFilter( t => t["C2"] != null );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Constraints.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            sut.Columns.Expressions.Should().BeEmpty();
            sut.ReferencedColumns.Should().BeEmpty();
            sut.ReferencedFilterColumns.Should().BeEmpty();
            sut.Filter.Should().BeNull();
            c2.ReferencingObjects.Should().BeEmpty();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 ).Sql.Should().SatisfySql( "DROP INDEX `IX_T_C2A` ON `foo`.`T`;" );
        }
    }

    [Fact]
    public void Remove_ShouldRemoveIndexAndAssignedPrimaryKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var column = table.Columns.Create( "C" );
        var pk = table.Constraints.SetPrimaryKey( column.Asc() );
        var sut = pk.Index;

        sut.Remove();

        using ( new AssertionScope() )
        {
            table.Constraints.TryGetPrimaryKey().Should().BeNull();
            table.Constraints.TryGet( sut.Name ).Should().BeNull();
            table.Constraints.TryGet( pk.Name ).Should().BeNull();
            schema.Objects.TryGet( sut.Name ).Should().BeNull();
            schema.Objects.TryGet( pk.Name ).Should().BeNull();
            sut.IsRemoved.Should().BeTrue();
            sut.PrimaryKey.Should().BeNull();
            sut.Columns.Expressions.Should().BeEmpty();
            sut.ReferencedColumns.Should().BeEmpty();
            pk.IsRemoved.Should().BeTrue();
            column.ReferencingObjects.Should().BeEmpty();
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenIndexIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenIndexHasOriginatingForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var ix = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( sut, ix );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenIndexHasReferencingForeignKey()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() ).Index;
        var ix = table.Constraints.CreateIndex( table.Columns.Create( "C2" ).Asc() );
        table.Constraints.CreateForeignKey( ix, sut );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenIndexIsMySql()
    {
        var action = Substitute.For<Action<MySqlIndexBuilder>>();
        var table = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" );
        var sut = table.Constraints.CreateIndex( table.Columns.Create( "C1" ).Asc() );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenIndexIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlIndexBuilder>>();
        var sut = Substitute.For<ISqlIndexBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }
}
