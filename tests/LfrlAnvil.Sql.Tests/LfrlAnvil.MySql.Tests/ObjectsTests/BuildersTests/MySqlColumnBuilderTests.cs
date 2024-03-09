using System.Linq;
using LfrlAnvil.Functional;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.MySql.Tests.Helpers;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.TestExtensions.FluentAssertions;
using LfrlAnvil.TestExtensions.Sql;
using LfrlAnvil.TestExtensions.Sql.FluentAssertions;

namespace LfrlAnvil.MySql.Tests.ObjectsTests.BuildersTests;

public class MySqlColumnBuilderTests : TestsBase
{
    [Fact]
    public void ToString_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = sut.ToString();

        result.Should().Be( "[Column] foo.T.C" );
    }

    [Fact]
    public void Asc_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = sut.Asc();

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut );
            result.Ordering.Should().BeSameAs( OrderBy.Asc );
            result.ToString().Should().Be( "[Column] foo.T.C ASC" );
        }
    }

    [Fact]
    public void Desc_ShouldReturnCorrectResult()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        var sut = table.Columns.Create( "C" );

        var result = sut.Desc();

        using ( new AssertionScope() )
        {
            result.Column.Should().BeSameAs( sut );
            result.Ordering.Should().BeSameAs( OrderBy.Desc );
            result.ToString().Should().Be( "[Column] foo.T.C DESC" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlterationAndAutomaticallySetDefaultValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeSameAs( sut.TypeDefinition.DefaultValue );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      ADD COLUMN `C2` LONGBLOB NOT NULL DEFAULT (X'');" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WithoutDefaultValueWhenColumnIsNullable()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      ADD COLUMN `C2` LONGBLOB;" );
        }
    }

    [Fact]
    public void Creation_ShouldMarkTableForAlteration_WhenDefaultValueIsDefinedExplicitly()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( new byte[] { 1, 2, 3 } );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      ADD COLUMN `C2` LONGBLOB NOT NULL DEFAULT (X'010203');" );
        }
    }

    [Fact]
    public void Creation_WithReusedRemovedColumnName_ShouldTreatTheColumnAsModified()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var removed = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        removed.SetName( "C3" ).Remove();
        table.Columns.Create( "C3" ).MarkAsNullable();
        var sut = table.Columns.Create( "C2" ).SetType<string>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();
            removed.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGTEXT NOT NULL,
                      ADD COLUMN `C3` LONGBLOB;" );
        }
    }

    [Fact]
    public void Creation_WithReusedRemovedColumnName_ShouldTreatTheColumnAsModified_WithOriginalIsNullableChange()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var removed = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        removed.SetName( "C3" ).MarkAsNullable( false ).Remove();
        var sut = table.Columns.Create( "C2" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();
            removed.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL;" );
        }
    }

    [Fact]
    public void Creation_WithReusedRemovedColumnName_ShouldTreatTheColumnAsModified_WithOriginalTypeDefinitionChange()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var removed = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        removed.SetName( "C3" ).SetType<string>().Remove();
        var sut = table.Columns.Create( "C2" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Get( sut.Name ).Should().BeSameAs( sut );
            sut.Name.Should().Be( "C2" );
            sut.DefaultValue.Should().BeNull();
            removed.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL;" );
        }
    }

    [Fact]
    public void Creation_FollowedByRemove_ShouldDoNothing()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var sut = table.Columns.Create( "C2" );
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void SetName_ShouldDoNothing_WhenNewNameEqualsOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

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
        var sut = table.Columns.Create( "C2" );
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
    public void SetName_ShouldUpdateName_WhenNewNameIsDifferentFromOldName()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        var node = sut.Node;

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetName( "bar" );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.Name.Should().Be( "bar" );
            table.Columns.TryGet( "bar" ).Should().BeSameAs( sut );
            table.Columns.TryGet( "C2" ).Should().BeNull();
            node.Name.Should().Be( "bar" );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `bar` LONGBLOB NOT NULL;" );
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
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetName( name ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetName( "bar" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenNewNameAlreadyExistsInTableColumns()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetName( "C1" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetName( "C3" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetName_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.SetName( "C3" ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenNewTypeEqualsOldType()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType( schema.Database.TypeDefinitions.GetByType<object>() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenTypeChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetType( MySqlDataType.Int );
        var result = sut.SetType( schema.Database.TypeDefinitions.GetByType<object>() );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldUpdateTypeAndSetDefaultValueToNull_WhenNewTypeIsDifferentFromOldType()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType<int>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.TypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<int>() );
            sut.DefaultValue.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` INT NOT NULL;" );
        }
    }

    [Fact]
    public void SetType_ShouldDoNothing_WhenNewTypeIsDifferentFromOldTypeButMySqlTypeRemainsTheSameAndDefaultValueIsNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create( new TypeDefinitionMock() ).Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetType( MySqlDataType.Int );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType<int>();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.TypeDefinition.Should().BeSameAs( schema.Database.TypeDefinitions.GetByType<int>() );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.SetType<int>() );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectBuilderException_WhenTypeDefinitionDoesNotBelongToDatabase()
    {
        var definition = MySqlDatabaseBuilderMock.Create().TypeDefinitions.GetByType<int>();
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetType( definition ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetType_ShouldThrowSqlObjectCastException_WhenTypeDefinitionIsOfInvalidType()
    {
        var definition = Substitute.For<ISqlColumnTypeDefinition>();
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => ((ISqlColumnBuilder)sut).SetType( definition ) );

        action.Should()
            .ThrowExactly<SqlObjectCastException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Expected == typeof( SqlColumnTypeDefinition ) );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenNewValueEqualsOldValue(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( value );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.MarkAsNullable( ! value );
        var result = sut.MarkAsNullable( value );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToTrue_WhenOldValueIsFalse()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.IsNullable.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB;" );
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToFalse_WhenOldValueIsTrueAndColumnDoesNotHaveDefaultValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL;" );
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToFalse_WhenOldValueIsTrueAndColumnHasDefaultValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable().SetDefaultValue( new byte[] { 1, 2, 3 } );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.MarkAsNullable( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL DEFAULT (X'010203');" );
        }
    }

    [Fact]
    public void MarkAsNullable_ShouldUpdateIsNullableToFalse_WhenOldValueIsTrueAndColumnTypeDefinitionChanged()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable();

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetType( MySqlDataType.Int ).MarkAsNullable( false );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            result.IsNullable.Should().BeFalse();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` INT NOT NULL;" );
        }
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        sut.Remove();

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInIndex(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        table.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Theory]
    [InlineData( true )]
    [InlineData( false )]
    public void MarkAsNullable_ShouldThrowSqlObjectBuilderException_WhenColumnIsUsedInView(bool value)
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).MarkAsNullable( ! value );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.MarkAsNullable( value ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenNewValueEqualsOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( sut.DefaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldDoNothing_WhenValueChangeIsFollowedByChangeToOriginal()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );
        var originalDefaultValue = sut.DefaultValue;

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetDefaultValue( (int?)42 );
        var result = sut.SetDefaultValue( originalDefaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            actions.Should().BeEmpty();
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsDifferentFromOldValue()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( 42 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 42 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL DEFAULT (42);" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsNull()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetDefaultValue( 123 );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( null );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeNull();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL;" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldUpdateDefaultValue_WhenNewValueIsValidComplexExpression()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" ).SetType<long>();

        var actionCount = schema.Database.GetPendingActionCount();
        var defaultValue = SqlNode.Literal( 10 ) + SqlNode.Literal( 50 ) + SqlNode.Literal( 100 ).Max( SqlNode.Literal( 80 ) );
        var result = sut.SetDefaultValue( defaultValue );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeSameAs( defaultValue );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` BIGINT NOT NULL DEFAULT ((10 + 50) + GREATEST(100, 80));" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldBePossible_WhenColumnIsUsedInIndex()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        table.Constraints.CreateIndex( sut.Asc() );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( 123 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 123 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL DEFAULT (123);" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldBePossible_WhenColumnIsUsedInView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", table.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var actionCount = schema.Database.GetPendingActionCount();
        var result = sut.SetDefaultValue( (int?)123 );
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            result.Should().BeSameAs( sut );
            sut.DefaultValue.Should().BeEquivalentTo( SqlNode.Literal( 123 ) );

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      CHANGE COLUMN `C2` `C2` LONGBLOB NOT NULL DEFAULT (123);" );
        }
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenColumnIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );
        sut.Remove();

        var action = Lambda.Of( () => sut.SetDefaultValue( 42 ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void SetDefaultValue_ShouldThrowSqlObjectBuilderException_WhenExpressionIsInvalid()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var action = Lambda.Of( () => sut.SetDefaultValue( table.ToRecordSet().GetField( "C1" ) ) );

        action.Should()
            .ThrowExactly<SqlObjectBuilderException>()
            .AndMatch( e => e.Dialect == MySqlDialect.Instance && e.Errors.Count == 1 );
    }

    [Fact]
    public void Remove_ShouldRemoveColumn()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        var actionCount = schema.Database.GetPendingActionCount();
        sut.SetName( "bar" ).Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        using ( new AssertionScope() )
        {
            table.Columns.Contains( sut.Name ).Should().BeFalse();
            sut.IsRemoved.Should().BeTrue();

            actions.Should().HaveCount( 1 );
            actions.ElementAtOrDefault( 0 )
                .Sql.Should()
                .SatisfySql(
                    @"ALTER TABLE `foo`.`T`
                      DROP COLUMN `C2`;" );
        }
    }

    [Fact]
    public void Remove_ShouldDoNothing_WhenColumnIsRemoved()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var table = schema.Objects.CreateTable( "T" );
        table.Constraints.SetPrimaryKey( table.Columns.Create( "C1" ).Asc() );
        var sut = table.Columns.Create( "C2" );

        schema.Database.Changes.CompletePendingChanges();
        sut.Remove();

        var actionCount = schema.Database.GetPendingActionCount();
        sut.Remove();
        var actions = schema.Database.GetLastPendingActions( actionCount );

        actions.Should().BeEmpty();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByIndex()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Constraints.CreateIndex( sut.Asc() );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByIndexFilter()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Constraints.CreateIndex( t1.Columns.Create( "C3" ).Asc() ).SetFilter( t => t["C2"] != null );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByView()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        schema.Objects.CreateView( "V", t1.ToRecordSet().ToDataSource().Select( s => new[] { s.From["C2"].AsSelf() } ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void Remove_ShouldThrowSqlObjectBuilderException_WhenColumnIsReferencedByCheck()
    {
        var schema = MySqlDatabaseBuilderMock.Create().Schemas.Create( "foo" );
        var t1 = schema.Objects.CreateTable( "T1" );
        t1.Constraints.SetPrimaryKey( t1.Columns.Create( "C1" ).Asc() );
        var sut = t1.Columns.Create( "C2" );
        t1.Constraints.CreateCheck( t1.Node["C2"] != SqlNode.Literal( 0 ) );

        var action = Lambda.Of( () => sut.Remove() );

        action.Should().ThrowExactly<SqlObjectBuilderException>();
    }

    [Fact]
    public void ForMySql_ShouldInvokeAction_WhenColumnIsMySql()
    {
        var action = Substitute.For<Action<MySqlColumnBuilder>>();
        var sut = MySqlDatabaseBuilderMock.Create().Schemas.Default.Objects.CreateTable( "T" ).Columns.Create( "C" );

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallAt( 0 ).Exists().And.Arguments.Should().BeSequentiallyEqualTo( sut );
    }

    [Fact]
    public void ForMySql_ShouldNotInvokeAction_WhenColumnIsNotMySql()
    {
        var action = Substitute.For<Action<MySqlColumnBuilder>>();
        var sut = Substitute.For<ISqlColumnBuilder>();

        var result = sut.ForMySql( action );

        result.Should().BeSameAs( sut );
        action.Verify().CallCount.Should().Be( 0 );
    }

    private sealed class TypeDefinitionMock : MySqlColumnTypeDefinition<int>
    {
        public TypeDefinitionMock()
            : base( MySqlDataType.Int, 0, (r, o) => r.GetInt32( o ) ) { }

        public override string ToDbLiteral(int value)
        {
            return value.ToString();
        }

        public override object ToParameterValue(int value)
        {
            return value;
        }
    }
}
